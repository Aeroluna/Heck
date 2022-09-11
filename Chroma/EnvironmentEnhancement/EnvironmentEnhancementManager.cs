using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement.Component;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Heck.Animation.Transform;
using JetBrains.Annotations;
using SiraUtil.Affinity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static Chroma.ChromaController;
using static Heck.HeckController;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement
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
    internal class EnvironmentEnhancementManager : IAffinity
    {
        private readonly CustomBeatmapData _beatmapData;
        private readonly Dictionary<string, Track> _tracks;
        private readonly bool _leftHanded;
        private readonly GeometryFactory _geometryFactory;
        private readonly TrackLaneRingOffset _trackLaneRingOffset;
        private readonly ParametricBoxControllerTransformOverride _parametricBoxControllerTransformOverride;
        private readonly BeatmapObjectsAvoidanceTransformOverride _beatmapObjectsAvoidanceTransformOverride;
        private readonly DuplicateInitializer _duplicateInitializer;
        private readonly ComponentCustomizer _componentCustomizer;
        private readonly TransformControllerFactory _controllerFactory;

        private EnvironmentEnhancementManager(
            BeatmapObjectSpawnController spawnController,
            IReadonlyBeatmapData beatmapData,
            Dictionary<string, Track> tracks,
            [Inject(Id = LEFT_HANDED_ID)] bool leftHanded,
            GeometryFactory geometryFactory,
            TrackLaneRingOffset trackLaneRingOffset,
            ParametricBoxControllerTransformOverride parametricBoxControllerTransformOverride,
            BeatmapObjectsAvoidanceTransformOverride beatmapObjectsAvoidanceTransformOverride,
            DuplicateInitializer duplicateInitializer,
            ComponentCustomizer componentCustomizer,
            TransformControllerFactory controllerFactory)
        {
            _beatmapData = (CustomBeatmapData)beatmapData;
            _tracks = tracks;
            _leftHanded = leftHanded;
            _geometryFactory = geometryFactory;
            _trackLaneRingOffset = trackLaneRingOffset;
            _parametricBoxControllerTransformOverride = parametricBoxControllerTransformOverride;
            _beatmapObjectsAvoidanceTransformOverride = beatmapObjectsAvoidanceTransformOverride;
            _duplicateInitializer = duplicateInitializer;
            _componentCustomizer = componentCustomizer;
            _controllerFactory = controllerFactory;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectSpawnController), nameof(BeatmapObjectSpawnController.Start))]
        private void Start(BeatmapObjectSpawnController __instance)
        {
            __instance.StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return null; // Wait for material
            yield return new WaitForEndOfFrame();

            bool v2 = _beatmapData.version2_6_0AndEarlier;

            IEnumerable<CustomData>? environmentData = _beatmapData.customData
                .Get<List<object>>(v2 ? V2_ENVIRONMENT : ENVIRONMENT)?
                .Cast<CustomData>();
            List<GameObjectInfo> allGameObjectInfos = GetAllGameObjects.Get();

            if (v2)
            {
                try
                {
                    LegacyEnvironmentRemoval.Init(_beatmapData);
                }
                catch (Exception e)
                {
                    Log.Logger.Log("Could not run Legacy Enviroment Removal", Logger.Level.Error);
                    Log.Logger.Log(e, Logger.Level.Error);
                }
            }

            if (environmentData == null)
            {
                yield break;
            }

            if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
            {
                Log.Logger.Log("=====================================");
            }

            string[] gameObjectInfoIds = allGameObjectInfos.Select(n => n.FullID).ToArray();

            foreach (CustomData gameObjectData in environmentData)
            {
                int? dupeAmount = gameObjectData.Get<int?>(v2 ? V2_DUPLICATION_AMOUNT : DUPLICATION_AMOUNT);
                bool? active = gameObjectData.Get<bool?>(v2 ? V2_ACTIVE : ACTIVE);
                TransformData spawnData = new(gameObjectData, v2);
                int? lightID = gameObjectData.Get<int?>(V2_LIGHT_ID);

                List<GameObjectInfo> foundObjects;
                CustomData? geometryData = gameObjectData.Get<CustomData?>(v2 ? V2_GEOMETRY : GEOMETRY);
                if (geometryData != null)
                {
                    GameObjectInfo newObjectInfo = new(_geometryFactory.Create(geometryData));
                    allGameObjectInfos.Add(newObjectInfo);
                    foundObjects = new List<GameObjectInfo> { newObjectInfo };
                    if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Log.Logger.Log("Created new geometry object:");
                        Log.Logger.Log(newObjectInfo.FullID);
                    }

                    // cause i know ppl are gonna fck it up
                    string? id = gameObjectData.Get<string>(GAMEOBJECT_ID);
                    LookupMethod? lookupMethod = gameObjectData.GetStringToEnum<LookupMethod?>(LOOKUP_METHOD);
                    if (id != null || lookupMethod != null)
                    {
                        throw new InvalidOperationException("you cant have geometry and an id you goofball");
                    }
                }
                else
                {
                    string id = gameObjectData.GetRequired<string>(v2 ? V2_GAMEOBJECT_ID : GAMEOBJECT_ID);
                    LookupMethod lookupMethod = gameObjectData.GetStringToEnumRequired<LookupMethod>(v2 ? V2_LOOKUP_METHOD : LOOKUP_METHOD);
                    foundObjects = LookupID.Get(allGameObjectInfos, gameObjectInfoIds, id, lookupMethod);

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
                }

                CustomData? componentData = null;
                if (!v2)
                {
                    componentData = gameObjectData.Get<CustomData>(ComponentConstants.COMPONENTS);
                }
                else if (lightID != null)
                {
                    componentData = new CustomData(new[]
                    {
                        new KeyValuePair<string, object?>(
                            ComponentConstants.LIGHT_WITH_ID,
                            new CustomData(new[]
                            {
                                new KeyValuePair<string, object?>(LIGHT_ID, lightID.Value)
                            }))
                    });
                }

                List<GameObject> gameObjects;

                // handle duplicating
                if (dupeAmount.HasValue)
                {
                    gameObjects = new List<GameObject>();
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
                            DuplicateInitializer.PrefillComponentsData(gameObject.transform, componentDatas);
                            GameObject newGameObject = Object.Instantiate(gameObject);
                            _duplicateInitializer.PostfillComponentsData(
                                newGameObject.transform,
                                gameObject.transform,
                                componentDatas);
                            SceneManager.MoveGameObjectToScene(newGameObject, scene);

                            // ReSharper disable once Unity.InstantiateWithoutParent
                            // need to move shit to right scene first
                            newGameObject.transform.SetParent(parent, true);
                            _duplicateInitializer.InitializeComponents(
                                newGameObject.transform,
                                gameObject.transform,
                                allGameObjectInfos,
                                componentDatas);

                            List<GameObjectInfo> gameObjectInfos =
                                allGameObjectInfos.Where(n => n.GameObject == newGameObject).ToList();
                            gameObjects.AddRange(gameObjectInfos.Select(n => n.GameObject));

                            if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                            {
                                gameObjectInfos.ForEach(n => Log.Logger.Log(n.FullID));
                            }
                        }
                    }

                    // Update array with new duplicated objects
                    gameObjectInfoIds = allGameObjectInfos.Select(n => n.FullID).ToArray();
                }
                else
                {
                    if (lightID.HasValue)
                    {
                        Log.Logger.Log("LightID requested but no duplicated object to apply to.", Logger.Level.Error);
                    }

                    gameObjects = foundObjects.Select(n => n.GameObject).ToList();
                }

                foreach (GameObject gameObject in gameObjects)
                {
                    if (active.HasValue)
                    {
                        gameObject.SetActive(active.Value);
                    }

                    Transform transform = gameObject.transform;

                    spawnData.Apply(transform, _leftHanded, v2);

                    // Handle TrackLaneRing
                    TrackLaneRing? trackLaneRing = gameObject.GetComponentInChildren<TrackLaneRing>();
                    if (trackLaneRing != null)
                    {
                        _trackLaneRingOffset.SetTransform(trackLaneRing, spawnData);
                    }

                    // Handle ParametricBoxController
                    ParametricBoxController parametricBoxController =
                        gameObject.GetComponentInChildren<ParametricBoxController>();
                    if (parametricBoxController != null)
                    {
                        _parametricBoxControllerTransformOverride.SetTransform(parametricBoxController, spawnData);
                    }

                    // Handle BeatmapObjectsAvoidance
                    BeatmapObjectsAvoidance beatmapObjectsAvoidance =
                        gameObject.GetComponentInChildren<BeatmapObjectsAvoidance>();
                    if (beatmapObjectsAvoidance != null)
                    {
                        _beatmapObjectsAvoidanceTransformOverride.SetTransform(beatmapObjectsAvoidance, spawnData);
                    }

                    if (componentData != null)
                    {
                        _componentCustomizer.Customize(transform, componentData);
                    }

                    Track? track = gameObjectData.GetNullableTrack(_tracks, v2);
                    if (track == null)
                    {
                        continue;
                    }

                    TransformController controller = _controllerFactory.Create(gameObject, track, true);
                    if (trackLaneRing != null)
                    {
                        controller.RotationUpdated += () => _trackLaneRingOffset.UpdateRotation(trackLaneRing);
                        controller.PositionUpdated += () => TrackLaneRingOffset.UpdatePosition(trackLaneRing);
                    }
                    else if (parametricBoxController != null)
                    {
                        controller.PositionUpdated += () => _parametricBoxControllerTransformOverride.UpdatePosition(parametricBoxController);
                        controller.ScaleUpdated += () => _parametricBoxControllerTransformOverride.UpdateScale(parametricBoxController);
                    }
                    else if (beatmapObjectsAvoidance != null)
                    {
                        controller.RotationUpdated += () => _beatmapObjectsAvoidanceTransformOverride.UpdateRotation(beatmapObjectsAvoidance);
                        controller.PositionUpdated += () => _beatmapObjectsAvoidanceTransformOverride.UpdatePosition(beatmapObjectsAvoidance);
                    }

                    track.AddGameObject(gameObject);
                }

                if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    Log.Logger.Log("=====================================");
                }
            }
        }
    }
}
