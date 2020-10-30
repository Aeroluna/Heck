namespace Chroma
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Chroma.Colorizer;
    using Chroma.HarmonyPatches;
    using Chroma.Settings;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using static Chroma.Plugin;

    public static class ChromaController
    {
        private static readonly FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.Accessor _beatmapObjectSpawnAccessor = FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.GetAccessor("_beatmapObjectSpawner");
        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");
        private static readonly FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.Accessor _audioTimeSourceAccessor = FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.GetAccessor("_audioTimeSource");
        private static readonly FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.Accessor _beatmapDataAccessor = FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.GetAccessor("_beatmapData");

        private static List<ChromaPatchData> _chromaPatches;

        public static bool ChromaIsActive { get; private set; }

        public static bool DoColorizerSabers { get; set; }

        internal static BeatmapObjectSpawnController BeatmapObjectSpawnController => BeatmapObjectSpawnControllerStart.BeatmapObjectSpawnController;

        internal static IAudioTimeSource IAudioTimeSource { get; private set; }

        public static void ToggleChromaPatches(bool value)
        {
            ChromaIsActive = value;

            if (value)
            {
                if (!Harmony.HasAnyPatches(HARMONYID))
                {
                    _chromaPatches.ForEach(n => _harmonyInstance.Patch(
                        n.OriginalMethod,
                        n.Prefix != null ? new HarmonyMethod(n.Prefix) : null,
                        n.Postfix != null ? new HarmonyMethod(n.Postfix) : null,
                        n.Transpiler != null ? new HarmonyMethod(n.Transpiler) : null));
                }
            }
            else
            {
                _harmonyInstance.UnpatchAll(HARMONYID);
            }
        }

        internal static void InitChromaPatches()
        {
            if (_chromaPatches == null)
            {
                _chromaPatches = new List<ChromaPatchData>();
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    object[] noodleattributes = type.GetCustomAttributes(typeof(ChromaPatch), true);
                    if (noodleattributes.Length > 0)
                    {
                        Type declaringType = null;
                        List<string> methodNames = new List<string>();
                        foreach (ChromaPatch n in noodleattributes)
                        {
                            if (n.DeclaringType != null)
                            {
                                declaringType = n.DeclaringType;
                            }

                            if (n.MethodName != null)
                            {
                                methodNames.Add(n.MethodName);
                            }
                        }

                        if (declaringType == null || !methodNames.Any())
                        {
                            throw new ArgumentException("Type or Method Name not described");
                        }

                        MethodInfo prefix = AccessTools.Method(type, "Prefix");
                        MethodInfo postfix = AccessTools.Method(type, "Postfix");
                        MethodInfo transpiler = AccessTools.Method(type, "Transpiler");

                        foreach (string methodName in methodNames)
                        {
                            MethodInfo methodInfo = declaringType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                            if (methodInfo == null)
                            {
                                throw new ArgumentException($"Could not find method '{methodName}' of '{declaringType}'");
                            }

                            _chromaPatches.Add(new ChromaPatchData(methodInfo, prefix, postfix, transpiler));
                        }
                    }
                }
            }
        }

        internal static IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();
            BeatmapObjectSpawnController beatmapObjectSpawnController = BeatmapObjectSpawnController;
            BeatmapObjectManager beatmapObjectManager = _beatmapObjectSpawnAccessor(ref beatmapObjectSpawnController) as BeatmapObjectManager;
            BeatmapObjectCallbackController coreSetup = BeatmapObjectCallbackControllerStart.BeatmapObjectCallbackController;
            IAudioTimeSource = _audioTimeSourceAccessor(ref coreSetup);
            IReadonlyBeatmapData beatmapData = _beatmapDataAccessor(ref coreSetup);

            beatmapObjectManager.noteWasCutEvent -= NoteColorizer.ColorizeSaber;
            beatmapObjectManager.noteWasCutEvent += NoteColorizer.ColorizeSaber;

            if (ChromaConfig.Instance.LightshowModifier)
            {
                foreach (BeatmapLineData b in beatmapData.beatmapLinesData)
                {
                    BeatmapLineData refBeatmapLineData = b;
                    _beatmapObjectsDataAccessor(ref refBeatmapLineData) = b.beatmapObjectsData.Where((source, index) => b.beatmapObjectsData[index].beatmapObjectType != BeatmapObjectType.Note).ToList();
                }

                foreach (Saber saber in Resources.FindObjectsOfTypeAll<Saber>())
                {
                    saber.gameObject.SetActive(false);
                }

                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Chroma");

                if (ChromaConfig.Instance.PlayersPlace)
                {
                    GameObject.Find("PlayersPlace")?.SetActive(false);
                }

                if (ChromaConfig.Instance.Spectrograms)
                {
                    GameObject.Find("Spectrograms")?.SetActive(false);
                }

                if (ChromaConfig.Instance.BackColumns)
                {
                    GameObject.Find("BackColumns")?.SetActive(false);
                }

                if (ChromaConfig.Instance.Buildings)
                {
                    GameObject.Find("Buildings")?.SetActive(false);
                }
            }

            if (Harmony.HasAnyPatches(HARMONYID))
            {
                if (beatmapData is CustomBeatmapData customBeatmap)
                {
                    if (ChromaConfig.Instance.EnvironmentEnhancementsEnabled)
                    {
                        // Spaghetti code below until I can figure out a better way of doing this
                        dynamic dynData = customBeatmap.beatmapCustomData;
                        List<object> objectsToKill = Trees.at(dynData, "_environmentRemoval");
                        if (objectsToKill != null)
                        {
                            IEnumerable<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                            foreach (string s in objectsToKill.Cast<string>())
                            {
                                if (s == "TrackLaneRing" || s == "BigTrackLaneRing")
                                {
                                    foreach (GameObject n in gameObjects.Where(obj => obj.name.Contains(s)))
                                    {
                                        if (s == "TrackLaneRing" && n.name.Contains("Big"))
                                        {
                                            continue;
                                        }

                                        n.SetActive(false);
                                    }
                                }
                                else
                                {
                                    foreach (GameObject n in gameObjects
                                        .Where(obj => obj.name.Contains(s) && (obj.scene.name?.Contains("Environment") ?? false) && (!obj.scene.name?.Contains("Menu") ?? false)))
                                    {
                                        n.SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                }

                // please let me kill legacy
                LegacyLightHelper.Activate(beatmapData.beatmapEventsData);
            }
        }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        internal static void OnActiveSceneChanged(Scene current, Scene _)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (current.name == "GameCore")
            {
                LightColorizer.ClearLSEColorManagers();
                ObstacleColorizer.ClearOCColorManagers();
                BombColorizer.ClearBNCColorManagers();
                NoteColorizer.ClearCNVColorManagers();
                SaberColorizer.ClearBSMColorManagers();
            }
        }
    }
}
