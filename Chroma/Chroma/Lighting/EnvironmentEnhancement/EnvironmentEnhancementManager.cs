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
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _prevRotZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_prevRotZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _rotZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_rotZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _prevPosZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_prevPosZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _posZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_posZ");

        private static List<GameObjectInfo> _gameObjectInfos;

        internal static Dictionary<TrackLaneRing, bool> SkipRingUpdate { get; set; }

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
                SkipRingUpdate = new Dictionary<TrackLaneRing, bool>();

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
                                GameObject newGameObject = UnityEngine.Object.Instantiate(gameObject);
                                SceneManager.MoveGameObjectToScene(newGameObject, scene);
                                newGameObject.transform.SetParent(parent, true);
                                ComponentInitializer.InitializeComponents(newGameObject.transform, gameObject.transform);
                                GameObjectInfo newGameObjectInfo = new GameObjectInfo(newGameObject);
                                gameObjectInfos.Add(newGameObjectInfo);
                            }
                        }

                        _gameObjectInfos.AddRange(gameObjectInfos);
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
                                _positionOffsetAccessor(ref trackLaneRing) = transform.position;
                                float zPosition = transform.position.z;
                                _prevPosZAccessor(ref trackLaneRing) = zPosition;
                                _posZAccessor(ref trackLaneRing) = zPosition;
                            }

                            if (rotation.HasValue || localRotation.HasValue)
                            {
                                float zRotation = transform.rotation.z;
                                _prevRotZAccessor(ref trackLaneRing) = zRotation;
                                _rotZAccessor(ref trackLaneRing) = zRotation;
                            }
                        }

                        if (NoodleExtensionsInstalled)
                        {
                            GameObjectTrackController.HandleTrackData(gameObject, gameObjectData, customBeatmapData, noteLinesDistance, trackLaneRing);
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

            GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject gameObject in gameObjects)
            {
                // 14 = "Environment" layer
                // 15 = "Neon Tube" layer
                if (gameObject.activeInHierarchy && (gameObject.scene.name.Contains("Environment") || gameObject.layer == 14 || gameObject.layer == 13))
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
        }
    }
}
