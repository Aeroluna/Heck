namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
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

        private static List<GameObjectInfo> _gameObjectInfos;

        internal static Dictionary<TrackLaneRing, bool> SkipRingUpdate { get; } = new Dictionary<TrackLaneRing, bool>();

        internal static Dictionary<TrackLaneRing, Vector3> RingRotationOffsets { get; } = new Dictionary<TrackLaneRing, Vector3>();

        internal static void SubscribeTrackManagerCreated()
        {
            TrackManager.TrackManagerCreated += CreateEnvironmentTracks;
        }

        internal static void CreateEnvironmentTracks(object trackManager, CustomBeatmapData customBeatmapData)
        {
            List<dynamic> environmentData = Trees.at(customBeatmapData.customData, ENVIRONMENT);
            if (environmentData != null)
            {
                foreach (dynamic gameObjectData in environmentData)
                {
                    string trackName = Trees.at(gameObjectData, "_track");
                    if (trackName != null)
                    {
                        ((TrackManager)trackManager).AddTrack(trackName);
                    }
                }
            }
        }

        internal static void Init(CustomBeatmapData customBeatmapData, float noteLinesDistance)
        {
            List<dynamic> environmentData = Trees.at(customBeatmapData.customData, ENVIRONMENT);
            GetAllGameObjects();
            if (environmentData != null)
            {
                SkipRingUpdate.Clear();
                RingRotationOffsets.Clear();
                ParametricBoxControllerParameters.TransformParameters.Clear();

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"=====================================");
                }

                foreach (dynamic gameObjectData in environmentData)
                {
                    string id = Trees.at(gameObjectData, ID);

                    string lookupString = Trees.at(gameObjectData, LOOKUPMETHOD);
                    LookupMethod lookupMethod = (LookupMethod)Enum.Parse(typeof(LookupMethod), lookupString);

                    int? dupeAmount = (int?)Trees.at(gameObjectData, DUPLICATIONAMOUNT);

                    bool? active = (bool?)Trees.at(gameObjectData, ACTIVE);

                    Vector3? scale = GetVectorData(gameObjectData, SCALE);
                    Vector3? position = GetVectorData(gameObjectData, POSITION);
                    Vector3? rotation = GetVectorData(gameObjectData, OBJECTROTATION);
                    Vector3? localPosition = GetVectorData(gameObjectData, LOCALPOSITION);
                    Vector3? localRotation = GetVectorData(gameObjectData, LOCALROTATION);

                    List<GameObjectInfo> foundObjects = LookupID(id, lookupMethod);
                    if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        ChromaLogger.Log($"ID [\"{id}\"] using method [{lookupMethod:G}] found:");
                        foundObjects.ForEach(n => ChromaLogger.Log(n.FullID));
                    }

                    List<GameObjectInfo> gameObjectInfos;

                    if (dupeAmount.HasValue)
                    {
                        gameObjectInfos = new List<GameObjectInfo>();
                        foreach (GameObjectInfo gameObjectInfo in foundObjects)
                        {
                            if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                            {
                                ChromaLogger.Log($"Duplicating [{gameObjectInfo.FullID}]:");
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
                                ComponentInitializer.InitializeComponents(newGameObject.transform, gameObject.transform, _gameObjectInfos, componentDatas);
                                gameObjectInfos.AddRange(_gameObjectInfos.Where(n => n.GameObject == newGameObject));
                            }
                        }
                    }
                    else
                    {
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
                            }

                            if (rotation.HasValue || localRotation.HasValue)
                            {
                                RingRotationOffsets[trackLaneRing] = transform.localEulerAngles;
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

                        if (NoodleExtensionsInstalled)
                        {
                            GameObjectTrackController.HandleTrackData(gameObject, gameObjectData, customBeatmapData, noteLinesDistance, trackLaneRing, parametricBoxController);
                        }
                    }

                    if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        ChromaLogger.Log($"=====================================");
                    }
                }
            }

            LegacyEnvironmentRemoval.Init(customBeatmapData);
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

        private static Vector3? GetVectorData(dynamic dynData, string name)
        {
            IEnumerable<float> data = ((List<object>)Trees.at(dynData, name))?.Select(n => Convert.ToSingle(n));
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

            foreach (GameObject gameObject in gameObjects)
            {
                _gameObjectInfos.Add(new GameObjectInfo(gameObject));

                // seriously what the fuck beat games
                // GradientBackground permanently yeeted because it looks awful and can ruin multi-colored chroma maps
                if (gameObject.name == "GradientBackground")
                {
                    gameObject.SetActive(false);
                }
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
