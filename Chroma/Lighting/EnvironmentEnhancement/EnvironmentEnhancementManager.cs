using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Chroma.ChromaController;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Chroma.Lighting.EnvironmentEnhancement
{
    internal enum LookupMethod
    {
        Regex,
        Exact,
        Contains
    }

    internal static class EnvironmentEnhancementManager
    {
        private const string LOOKUPDLLDIRECTORY = @"UserData\Chroma";
        private const string LOOKUPDLLPATH = LOOKUPDLLDIRECTORY + @"\LookupID.dll";

        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _rotZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_rotZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _posZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_posZ");

        private static List<GameObjectInfo> _gameObjectInfos = new();

        internal static Dictionary<TrackLaneRing, Quaternion> RingRotationOffsets { get; private set; } = new();

        internal static Dictionary<BeatmapObjectsAvoidance, Vector3> AvoidancePosition { get; private set; } = new();

        internal static Dictionary<BeatmapObjectsAvoidance, Quaternion> AvoidanceRotation { get; private set; } = new();

        internal static void Init(CustomBeatmapData customBeatmapData, float noteLinesDistance)
        {
            IEnumerable<Dictionary<string, object?>>? environmentData = customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Cast<Dictionary<string, object?>>();
            GetAllGameObjects();

            RingRotationOffsets = new Dictionary<TrackLaneRing, Quaternion>();
            AvoidancePosition = new Dictionary<BeatmapObjectsAvoidance, Vector3>();
            AvoidanceRotation = new Dictionary<BeatmapObjectsAvoidance, Quaternion>();
            ParametricBoxControllerParameters.TransformParameters = new Dictionary<ParametricBoxController, ParametricBoxControllerParameters>();

            if (environmentData != null)
            {
                RingRotationOffsets.Clear();
                ParametricBoxControllerParameters.TransformParameters.Clear();

                if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    Log.Logger.Log("=====================================");
                }

                string[] gameObjectInfoIds = _gameObjectInfos.Select(n => n.FullID).ToArray();

                bool useLegacy = false;
                if (!File.Exists(LOOKUPDLLPATH))
                {
                    Log.Logger.Log($"Failed to find [{LOOKUPDLLPATH}], using legacy lookup method. PREPARE FOR LONG LOAD TIMES.", Logger.Level.Error);
                    useLegacy = true;
                }

                foreach (Dictionary<string, object?> gameObjectData in environmentData)
                {
                    string id = gameObjectData.Get<string>(ID) ?? throw new InvalidOperationException("Id was not defined.");

                    string lookupString = gameObjectData.Get<string>(LOOKUP_METHOD) ?? throw new InvalidOperationException("Lookup method was not defined.");
                    LookupMethod lookupMethod = (LookupMethod)Enum.Parse(typeof(LookupMethod), lookupString);

                    int? dupeAmount = gameObjectData.Get<int?>(DUPLICATION_AMOUNT);

                    bool? active = gameObjectData.Get<bool?>(ACTIVE);

                    Vector3? scale = gameObjectData.GetVector3(SCALE);
                    Vector3? position = gameObjectData.GetVector3(POSITION);
                    Vector3? rotation = gameObjectData.GetVector3(OBJECT_ROTATION);
                    Vector3? localPosition = gameObjectData.GetVector3(LOCAL_POSITION);
                    Vector3? localRotation = gameObjectData.GetVector3(LOCAL_ROTATION);

                    int? lightID = gameObjectData.Get<int?>(LIGHT_ID);

                    List<GameObjectInfo> foundObjects = useLegacy ? LookupID_Legacy(id, lookupMethod)
                        : LookupID(gameObjectInfoIds, id, lookupMethod);
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
                                ComponentInitializer.PrefillComponentsData(gameObject.transform, componentDatas);
                                GameObject newGameObject = Object.Instantiate(gameObject);
                                ComponentInitializer.PostfillComponentsData(newGameObject.transform, gameObject.transform, componentDatas);
                                SceneManager.MoveGameObjectToScene(newGameObject, scene);

                                // ReSharper disable once Unity.InstantiateWithoutParent
                                // need to move shit to right scene first
                                newGameObject.transform.SetParent(parent, true);
                                ComponentInitializer.InitializeComponents(newGameObject.transform, gameObject.transform, _gameObjectInfos, componentDatas, lightID);

                                List<GameObjectInfo> gameObjects = _gameObjectInfos.Where(n => n.GameObject == newGameObject).ToList();
                                gameObjectInfos.AddRange(gameObjects);

                                if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                                {
                                    gameObjects.ForEach(n => Log.Logger.Log(n.FullID));
                                }
                            }
                        }
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

                    if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Log.Logger.Log("=====================================");
                    }
                }
            }

            try
            {
                LegacyEnvironmentRemoval.Init(customBeatmapData);
            }
            catch (Exception e)
            {
                Log.Logger.Log("Could not run Legacy Enviroment Removal");
                Log.Logger.Log(e);
            }
        }

        // Why does c++ have to be so much faster??
        internal static void SaveLookupIDDLL()
        {
            try
            {
                if (File.Exists(LOOKUPDLLPATH))
                {
                    Log.Logger.Log($"Already exists: [{LOOKUPDLLPATH}].", Logger.Level.Trace);
                    return;
                }

                Log.Logger.Log($"Saving: [{LOOKUPDLLPATH}].", Logger.Level.Trace);
                Directory.CreateDirectory(LOOKUPDLLDIRECTORY);
                using Stream? resource = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Chroma.Lighting.EnvironmentEnhancement.LookupID.dll");
                using FileStream file = new(LOOKUPDLLPATH, FileMode.Create, FileAccess.Write);
                resource?.CopyTo(file);
            }
            catch (Exception e)
            {
                Log.Logger.Log($"Failed to save [{LOOKUPDLLPATH}], you may experience long load times.", Logger.Level.Error);
                Log.Logger.Log(e, Logger.Level.Error);
            }
        }

        // whatever the fuck rider is recommending causes shit to crash so we disable it
#pragma warning disable CA2101
        [DllImport(LOOKUPDLLPATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LookupID_internal([In, Out] string[] array, int size, ref IntPtr returnArray, ref int returnSize, [MarshalAs(UnmanagedType.LPStr)] string id, int method);
#pragma warning restore CA2101

        // this is where i pretend to know what any of this is doing.
        private static List<GameObjectInfo> LookupID(string[] gameObjectIds, string id, LookupMethod lookupMethod)
        {
            int length = gameObjectIds.Length;
            IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(length) * length);
            LookupID_internal(gameObjectIds, length, ref buffer, ref length, id, (int)lookupMethod);

            int[] arrayRes = new int[length];
            Marshal.Copy(buffer, arrayRes, 0, length);
            Marshal.FreeCoTaskMem(buffer);

            List<GameObjectInfo> returnList = new(length);
            returnList.AddRange(arrayRes.Select(index => _gameObjectInfos[index]));

            return returnList;
        }

        private static List<GameObjectInfo> LookupID_Legacy(string id, LookupMethod lookupMethod)
        {
            Func<GameObjectInfo, bool> predicate;
            switch (lookupMethod)
            {
                case LookupMethod.Regex:
                    Regex regex = new(id, RegexOptions.CultureInvariant);
                    predicate = n => regex.IsMatch(n.FullID);
                    break;

                case LookupMethod.Exact:
                    predicate = n => n.FullID == id;
                    break;

                case LookupMethod.Contains:
                    predicate = n => n.FullID.Contains(id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(lookupMethod), "Invalid lookup method.");
            }

            return _gameObjectInfos.Where(predicate).ToList();
        }

        private static void GetAllGameObjects()
        {
            _gameObjectInfos = new List<GameObjectInfo>();

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
