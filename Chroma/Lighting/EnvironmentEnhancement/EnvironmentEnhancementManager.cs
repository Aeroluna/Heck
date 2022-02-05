using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static Chroma.ChromaController;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Chroma.Lighting.EnvironmentEnhancement
{
    // ReSharper disable UnusedMember.Global
    internal enum LookupMethod
    {
        Regex,
        Exact,
        Contains,
        StartsWith,
        EndsWith
    }

    [UsedImplicitly]
    internal class EnvironmentEnhancementManager : IDisposable
    {
        private const string LOOKUPDLL = @"LookupID.dll";

        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _rotZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_rotZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _posZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_posZ");

        private readonly List<GameObjectInfo> _gameObjectInfos = new();

        private readonly CustomBeatmapData _beatmapData;
        private readonly float _noteLinesDistance;
        private readonly Dictionary<string, Track> _tracks;
        private readonly ParametricBoxControllerParameters _parametricBoxControllerParameters;
        private readonly LazyInject<ComponentInitializer> _componentInitializer;
        private readonly GameObjectTrackController.Factory _trackControllerFactory;

        private readonly HashSet<GameObjectTrackController> _gameObjectTrackControllers = new();

        private EnvironmentEnhancementManager(
            BeatmapObjectSpawnController spawnController,
            IReadonlyBeatmapData beatmapData,
            Dictionary<string, Track> tracks,
            ParametricBoxControllerParameters parametricBoxControllerParameters,
            LazyInject<ComponentInitializer> componentInitializer,
            GameObjectTrackController.Factory trackControllerFactory)
        {
            if (beatmapData is not CustomBeatmapData customBeatmapData)
            {
                throw new ArgumentNullException(nameof(beatmapData));
            }

            _beatmapData = customBeatmapData;
            _noteLinesDistance = spawnController.noteLinesDistance;
            _tracks = tracks;
            _parametricBoxControllerParameters = parametricBoxControllerParameters;
            _componentInitializer = componentInitializer;
            _trackControllerFactory = trackControllerFactory;
            spawnController.StartCoroutine(DelayedStart());
        }

        internal Dictionary<TrackLaneRing, Quaternion> RingRotationOffsets { get; } = new();

        internal Dictionary<BeatmapObjectsAvoidance, Vector3> AvoidancePosition { get; } = new();

        internal Dictionary<BeatmapObjectsAvoidance, Quaternion> AvoidanceRotation { get; } = new();

        public void Dispose()
        {
            _gameObjectTrackControllers.Do(Object.Destroy);
        }

        internal IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            IEnumerable<Dictionary<string, object?>>? environmentData = _beatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Cast<Dictionary<string, object?>>();
            GetAllGameObjects();

            if (environmentData != null)
            {
                if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    Log.Logger.Log("=====================================");
                }

                string[] gameObjectInfoIds = _gameObjectInfos.Select(n => n.FullID).ToArray();

                foreach (Dictionary<string, object?> gameObjectData in environmentData)
                {
                    string id = gameObjectData.Get<string>(GAMEOBJECT_ID) ?? throw new InvalidOperationException("Id was not defined.");

                    LookupMethod lookupMethod = gameObjectData.GetStringToEnum<LookupMethod?>(LOOKUP_METHOD) ?? throw new InvalidOperationException("Lookup method was not defined.");

                    int? dupeAmount = gameObjectData.Get<int?>(DUPLICATION_AMOUNT);

                    bool? active = gameObjectData.Get<bool?>(ACTIVE);

                    Vector3? scale = gameObjectData.GetVector3(SCALE);
                    Vector3? position = gameObjectData.GetVector3(POSITION);
                    Vector3? rotation = gameObjectData.GetVector3(OBJECT_ROTATION);
                    Vector3? localPosition = gameObjectData.GetVector3(LOCAL_POSITION);
                    Vector3? localRotation = gameObjectData.GetVector3(LOCAL_ROTATION);

                    int? lightID = gameObjectData.Get<int?>(LIGHT_ID);

                    List<GameObjectInfo> foundObjects = LookupID(gameObjectInfoIds, id, lookupMethod);
                    if (foundObjects.Count > 0)
                    {
                        if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                        {
                            Log.Logger.Log($"ID [\"{id}\"] using method [{lookupMethod:G}] found:");
                            foundObjects.ForEach(n => Log.Logger.Log(n.FullID));
                        }
                    }
                    else
                    {
                        Log.Logger.Log($"ID [\"{id}\"] using method [{lookupMethod:G}] found nothing.", Logger.Level.Error);
                    }

                    List<GameObjectInfo> gameObjectInfos;

                    if (dupeAmount.HasValue)
                    {
                        gameObjectInfos = new List<GameObjectInfo>();
                        foreach (GameObjectInfo gameObjectInfo in foundObjects)
                        {
                            if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                            {
                                Log.Logger.Log($"Duplicating [{gameObjectInfo.FullID}]:");
                            }

                            GameObject gameObject = gameObjectInfo.GameObject;
                            Transform parent = gameObject.transform.parent;
                            Scene scene = gameObject.scene;

                            for (int i = 0; i < dupeAmount.Value; i++)
                            {
                                List<IComponentData> componentDatas = new();
                                _componentInitializer.Value.PrefillComponentsData(gameObject.transform, componentDatas);
                                GameObject newGameObject = Object.Instantiate(gameObject);
                                _componentInitializer.Value.PostfillComponentsData(newGameObject.transform, gameObject.transform, componentDatas);
                                SceneManager.MoveGameObjectToScene(newGameObject, scene);

                                // ReSharper disable once Unity.InstantiateWithoutParent
                                // need to move shit to right scene first
                                newGameObject.transform.SetParent(parent, true);
                                _componentInitializer.Value.InitializeComponents(newGameObject.transform, gameObject.transform, _gameObjectInfos, componentDatas, lightID);

                                List<GameObjectInfo> gameObjects = _gameObjectInfos.Where(n => n.GameObject == newGameObject).ToList();
                                gameObjectInfos.AddRange(gameObjects);

                                if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                                {
                                    gameObjects.ForEach(n => Log.Logger.Log(n.FullID));
                                }
                            }
                        }

                        // Update array with new duplicated objects
                        gameObjectInfoIds = _gameObjectInfos.Select(n => n.FullID).ToArray();
                    }
                    else
                    {
                        if (lightID.HasValue)
                        {
                            Log.Logger.Log("LightID requested but no duplicated object to apply to.", Logger.Level.Error);
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
                            transform.position = position.Value * _noteLinesDistance;
                        }

                        if (rotation.HasValue)
                        {
                            transform.eulerAngles = rotation.Value;
                        }

                        if (localPosition.HasValue)
                        {
                            transform.localPosition = localPosition.Value * _noteLinesDistance;
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
                                _parametricBoxControllerParameters.SetTransformPosition(parametricBoxController, transform.localPosition);
                            }

                            if (scale.HasValue)
                            {
                                _parametricBoxControllerParameters.SetTransformScale(parametricBoxController, transform.localScale);
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

                        GameObjectTrackController? trackController = GameObjectTrackController.HandleTrackData(
                            _trackControllerFactory,
                            gameObject,
                            gameObjectData,
                            _noteLinesDistance,
                            trackLaneRing,
                            parametricBoxController,
                            beatmapObjectsAvoidance,
                            _tracks);
                        if (trackController != null)
                        {
                            _gameObjectTrackControllers.Add(trackController);
                        }
                    }

                    if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Log.Logger.Log("=====================================");
                    }
                }
            }

            try
            {
                LegacyEnvironmentRemoval.Init(_beatmapData);
            }
            catch (Exception e)
            {
                Log.Logger.Log("Could not run Legacy Enviroment Removal");
                Log.Logger.Log(e);
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

        // whatever the fuck rider is recommending causes shit to crash so we disable it
#pragma warning disable CA2101
        [DllImport(LOOKUPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LookupID_internal([In, Out] string[] array, int size, out IntPtr returnArray, ref int returnSize, [MarshalAs(UnmanagedType.LPStr)] string id, LookupMethod method);
#pragma warning restore CA2101

        // this is where i pretend to know what any of this is doing.
        private List<GameObjectInfo> LookupID(string[] gameObjectIds, string id, LookupMethod lookupMethod)
        {
            try
            {
                int length = gameObjectIds.Length;
                LookupID_internal(gameObjectIds, length, out IntPtr buffer, ref length, id, lookupMethod);

                int[] arrayRes = new int[length];
                Marshal.Copy(buffer, arrayRes, 0, length);
                Marshal.FreeCoTaskMem(buffer);

                List<GameObjectInfo> returnList = new(length);
                returnList.AddRange(arrayRes.Select(index => _gameObjectInfos[index]));
                return returnList;
            }
            catch (Exception e)
            {
                Log.Logger.Log("Error running LookupID, falling back to managed code.", Logger.Level.Error);
                Log.Logger.Log("Expect long load times...", Logger.Level.Error);
                Log.Logger.Log(e.ToString(), Logger.Level.Error);

                return LookupID_Legacy(id, lookupMethod);
            }
        }

        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        private List<GameObjectInfo> LookupID_Legacy(string id, LookupMethod lookupMethod)
        {
            Func<GameObjectInfo, bool> predicate;
            switch (lookupMethod)
            {
                case LookupMethod.Regex:
                    Regex regex = new(id, RegexOptions.CultureInvariant | RegexOptions.ECMAScript | RegexOptions.Compiled);
                    predicate = n => regex.IsMatch(n.FullID);
                    break;

                case LookupMethod.Exact:
                    predicate = n => n.FullID == id;
                    break;

                case LookupMethod.Contains:
                    predicate = n => n.FullID.Contains(id);
                    break;

                case LookupMethod.StartsWith:
                    predicate = n => n.FullID.StartsWith(id);
                    break;

                case LookupMethod.EndsWith:
                    predicate = n => n.FullID.EndsWith(id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(lookupMethod), "Invalid lookup method.");
            }

            return _gameObjectInfos.Where(predicate).ToList();
        }

        private void GetAllGameObjects()
        {
            // I'll probably revist this formula for getting objects by only grabbing the root objects and adding all the children
            List<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(n =>
            {
                if (n == null)
                {
                    return false;
                }

                string sceneName = n.scene.name;
                if (sceneName == null)
                {
                    return false;
                }

                return (sceneName.Contains("Environment") && !sceneName.Contains("Menu")) || n.GetComponent<TrackLaneRing>() != null;
            }).ToList();

            // Adds the children of whitelist GameObjects
            // Mainly for grabbing cone objects in KaleidoscopeEnvironment
            gameObjects.ToList().ForEach(n =>
            {
                List<Transform> allChildren = new();
                GetChildRecursive(n.transform, ref allChildren);

                foreach (Transform transform in allChildren)
                {
                    if (!gameObjects.Contains(transform.gameObject))
                    {
                        gameObjects.Add(transform.gameObject);
                    }
                }
            });

            List<string> objectsToPrint = new();

            foreach (GameObject gameObject in gameObjects)
            {
                GameObjectInfo gameObjectInfo = new(gameObject);
                _gameObjectInfos.Add(new GameObjectInfo(gameObject));
                objectsToPrint.Add(gameObjectInfo.FullID);

                // seriously what the fuck beat games
                // GradientBackground permanently yeeted because it looks awful and can ruin multi-colored chroma maps
                if (gameObject.name == "GradientBackground")
                {
                    gameObject.SetActive(false);
                }
            }

            if (!ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
            {
                return;
            }

            objectsToPrint.Sort();
            objectsToPrint.ForEach(n => Log.Logger.Log(n));
        }
    }
}
