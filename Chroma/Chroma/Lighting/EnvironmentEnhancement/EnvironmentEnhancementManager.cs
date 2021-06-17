namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using IPA.Utilities;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using static Chroma.Plugin;

    internal enum LookupMethod
    {
        Regex,
        Exact,
        Contains,
    }

    internal static class EnvironmentEnhancementManager
    {
        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _rotZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_rotZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _posZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_posZ");

        private static List<GameObjectInfo> _gameObjectInfos;

        internal static Dictionary<TrackLaneRing, Quaternion> RingRotationOffsets { get; } = new Dictionary<TrackLaneRing, Quaternion>();

        internal static Dictionary<BeatmapObjectsAvoidance, Vector3> AvoidancePosition { get; } = new Dictionary<BeatmapObjectsAvoidance, Vector3>();

        internal static Dictionary<BeatmapObjectsAvoidance, Quaternion> AvoidanceRotation { get; } = new Dictionary<BeatmapObjectsAvoidance, Quaternion>();

        internal static void SubscribeTrackManagerCreated()
        {
            TrackBuilder.TrackManagerCreated += CreateEnvironmentTracks;
        }

        internal static void CreateEnvironmentTracks(TrackBuilder trackManager, CustomBeatmapData customBeatmapData)
        {
            IEnumerable<Dictionary<string, object>> environmentData = customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Cast<Dictionary<string, object>>();
            if (environmentData != null)
            {
                foreach (Dictionary<string, object> gameObjectData in environmentData)
                {
                    string trackName = gameObjectData.Get<string>("_track");
                    if (trackName != null)
                    {
                        trackManager.AddTrack(trackName);
                    }
                }
            }
        }

        internal static void Init(CustomBeatmapData customBeatmapData, float noteLinesDistance)
        {
            IEnumerable<Dictionary<string, object>> environmentData = customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Cast<Dictionary<string, object>>();
            GetAllGameObjects();
            if (environmentData != null)
            {
                RingRotationOffsets.Clear();
                ParametricBoxControllerParameters.TransformParameters.Clear();

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    Plugin.Logger.Log($"=====================================");
                }

                foreach (Dictionary<string, object> gameObjectData in environmentData)
                {
                    string id = gameObjectData.Get<string>(ID);

                    string lookupString = gameObjectData.Get<string>(LOOKUPMETHOD);
                    LookupMethod lookupMethod = (LookupMethod)Enum.Parse(typeof(LookupMethod), lookupString);

                    int? dupeAmount = gameObjectData.Get<int?>(DUPLICATIONAMOUNT);

                    bool? active = gameObjectData.Get<bool?>(ACTIVE);

                    Vector3? scale = GetVectorData(gameObjectData, SCALE);
                    Vector3? position = GetVectorData(gameObjectData, POSITION);
                    Vector3? rotation = GetVectorData(gameObjectData, OBJECTROTATION);
                    Vector3? localPosition = GetVectorData(gameObjectData, LOCALPOSITION);
                    Vector3? localRotation = GetVectorData(gameObjectData, LOCALROTATION);

                    int? lightID = gameObjectData.Get<int?>(LIGHTID);

                    List<GameObjectInfo> foundObjects = LookupID(id, lookupMethod);
                    if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        if (foundObjects.Count > 0)
                        {
                            Plugin.Logger.Log($"ID [\"{id}\"] using method [{lookupMethod:G}] found:");
                            foundObjects.ForEach(n => Plugin.Logger.Log(n.FullID));
                        }
                        else
                        {
                            Plugin.Logger.Log($"ID [\"{id}\"] using method [{lookupMethod:G}] found nothing.", IPA.Logging.Logger.Level.Error);
                        }
                    }

                    List<GameObjectInfo> gameObjectInfos;

                    if (dupeAmount.HasValue)
                    {
                        gameObjectInfos = new List<GameObjectInfo>();
                        foreach (GameObjectInfo gameObjectInfo in foundObjects)
                        {
                            if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                            {
                                Plugin.Logger.Log($"Duplicating [{gameObjectInfo.FullID}]:");
                            }

                            GameObject gameObject = gameObjectInfo.GameObject;
                            Transform parent = gameObject.transform.parent;
                            Scene scene = gameObject.scene;

                            for (int i = 0; i < dupeAmount.Value; i++)
                            {
                                List<IComponentData> componentDatas = new List<IComponentData>();
                                ComponentInitializer.PrefillComponentsData(gameObject.transform, componentDatas);
                                GameObject newGameObject = UnityEngine.Object.Instantiate(gameObject);
                                ComponentInitializer.PostfillComponentsData(newGameObject.transform, gameObject.transform, componentDatas);
                                SceneManager.MoveGameObjectToScene(newGameObject, scene);
                                newGameObject.transform.SetParent(parent, true);
                                ComponentInitializer.InitializeComponents(newGameObject.transform, gameObject.transform, _gameObjectInfos, componentDatas, lightID);

                                List<GameObjectInfo> gameObjects = _gameObjectInfos.Where(n => n.GameObject == newGameObject).ToList();
                                gameObjectInfos.AddRange(gameObjects);

                                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                                {
                                    gameObjects.ForEach(n => Plugin.Logger.Log(n.FullID));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (lightID.HasValue)
                        {
                            Plugin.Logger.Log($"LightID requested but no duplicated object to apply to.", IPA.Logging.Logger.Level.Error);
                        }

                        gameObjectInfos = foundObjects;
                    }

                    foreach (GameObjectInfo gameObjectInfo in gameObjectInfos)
                    {
                        GameObject gameObject = gameObjectInfo.GameObject;

                        if (active.HasValue)
                        {
                            gameObjectInfo.GameObject.SetActive(active.Value);
                        }

                        Transform transform = gameObject.transform;

                        if (scale.HasValue)
                        {
                            transform.localScale = scale.Value;
                        }

                        if (position.HasValue)
                        {
                            transform.position = position.Value * noteLinesDistance;
                        }

                        if (rotation.HasValue)
                        {
                            transform.eulerAngles = rotation.Value;
                        }

                        if (localPosition.HasValue)
                        {
                            transform.localPosition = localPosition.Value * noteLinesDistance;
                        }

                        if (localRotation.HasValue)
                        {
                            transform.localEulerAngles = localRotation.Value;
                        }

                        // Handle TrackLaneRing
                        TrackLaneRing trackLaneRing = gameObject.GetComponent<TrackLaneRing>();
                        if (trackLaneRing != null)
                        {
                            if (position.HasValue || localPosition.HasValue)
                            {
                                _positionOffsetAccessor(ref trackLaneRing) = transform.localPosition;
                                _posZAccessor(ref trackLaneRing) = 0;
                            }

                            if (rotation.HasValue || localRotation.HasValue)
                            {
                                RingRotationOffsets[trackLaneRing] = transform.localRotation;
                                _rotZAccessor(ref trackLaneRing) = 0;
                            }
                        }

                        // Handle ParametricBoxController
                        ParametricBoxController parametricBoxController = gameObject.GetComponent<ParametricBoxController>();
                        if (parametricBoxController != null)
                        {
                            if (position.HasValue || localPosition.HasValue)
                            {
                                ParametricBoxControllerParameters.SetTransformPosition(parametricBoxController, transform.localPosition);
                            }

                            if (scale.HasValue)
                            {
                                ParametricBoxControllerParameters.SetTransformScale(parametricBoxController, transform.localScale);
                            }
                        }

                        // Handle BeatmapObjectsAvoidance
                        BeatmapObjectsAvoidance beatmapObjectsAvoidance = gameObject.GetComponent<BeatmapObjectsAvoidance>();
                        if (beatmapObjectsAvoidance != null)
                        {
                            if (position.HasValue || localPosition.HasValue)
                            {
                                AvoidancePosition[beatmapObjectsAvoidance] = transform.localPosition;
                            }

                            if (rotation.HasValue || localRotation.HasValue)
                            {
                                AvoidanceRotation[beatmapObjectsAvoidance] = transform.localRotation;
                            }
                        }

                        GameObjectTrackController.HandleTrackData(gameObject, gameObjectData, customBeatmapData, noteLinesDistance, trackLaneRing, parametricBoxController, beatmapObjectsAvoidance);
                    }

                    if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Plugin.Logger.Log($"=====================================");
                    }
                }
            }

            try
            {
                LegacyEnvironmentRemoval.Init(customBeatmapData);
            }
            catch (Exception e)
            {
                Plugin.Logger.Log("Could not run Legacy Enviroment Removal");
                Plugin.Logger.Log(e);
            }
        }

        private static List<GameObjectInfo> LookupID(string id, LookupMethod lookupMethod)
        {
            Func<GameObjectInfo, bool> predicate;
            switch (lookupMethod)
            {
                case LookupMethod.Regex:
                    Regex regex = new Regex(id, RegexOptions.CultureInvariant);
                    predicate = n => regex.IsMatch(n.FullID);
                    break;

                case LookupMethod.Exact:
                    predicate = n => n.FullID == id;
                    break;

                case LookupMethod.Contains:
                    predicate = n => n.FullID.Contains(id);
                    break;

                default:
                    return null;
            }

            return _gameObjectInfos.Where(predicate).ToList();
        }

        private static Vector3? GetVectorData(Dictionary<string, object> dynData, string name)
        {
            IEnumerable<float> data = dynData.Get<List<object>>(name)?.Select(n => Convert.ToSingle(n));
            Vector3? final = null;
            if (data != null)
            {
                final = new Vector3(data.ElementAt(0), data.ElementAt(1), data.ElementAt(2));
            }

            return final;
        }

        private static void GetAllGameObjects()
        {
            _gameObjectInfos = new List<GameObjectInfo>();

            // I'll probably revist this formula for getting objects by only grabbing the root objects and adding all the children
            List<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(n =>
            {
                if (n != null)
                {
                    string sceneName = n.scene.name;
                    if (sceneName != null)
                    {
                        if ((sceneName.Contains("Environment") && !sceneName.Contains("Menu")) || n.GetComponent<TrackLaneRing>() != null)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }).ToList();

            // Adds the children of whitelist GameObjects
            // Mainly for grabbing cone objects in KaleidoscopeEnvironment
            gameObjects.ToList().ForEach(n =>
            {
                List<Transform> allChildren = new List<Transform>();
                GetChildRecursive(n.transform, ref allChildren);

                foreach (Transform transform in allChildren)
                {
                    if (!gameObjects.Contains(transform.gameObject))
                    {
                        gameObjects.Add(transform.gameObject);
                    }
                }
            });

            List<string> objectsToPrint = new List<string>();

            foreach (GameObject gameObject in gameObjects)
            {
                GameObjectInfo gameObjectInfo = new GameObjectInfo(gameObject);
                _gameObjectInfos.Add(new GameObjectInfo(gameObject));
                objectsToPrint.Add(gameObjectInfo.FullID);

                // seriously what the fuck beat games
                // GradientBackground permanently yeeted because it looks awful and can ruin multi-colored chroma maps
                if (gameObject.name == "GradientBackground")
                {
                    gameObject.SetActive(false);
                }
            }

            if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
            {
                objectsToPrint.Sort();
                objectsToPrint.ForEach(n => Plugin.Logger.Log(n));
            }
        }

        private static void GetChildRecursive(Transform gameObject, ref List<Transform> children)
        {
            foreach (Transform child in gameObject)
            {
                children.Add(child);
                GetChildRecursive(child, ref children);
            }
        }
    }
}
