namespace Chroma
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Chroma.Extensions;
    using Chroma.Settings;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using static Chroma.Plugin;

    internal static class ChromaController
    {
        private static List<ChromaPatchData> _chromaPatches;

        internal static float SongBPM { get; private set; }

        internal static AudioTimeSyncController AudioTimeSyncController { get; private set; }

        internal static BeatmapObjectManager BeatmapObjectManager { get; private set; }

        public static void ToggleChromaPatches(bool value)
        {
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

                        methodNames.ForEach(n => _chromaPatches.Add(new ChromaPatchData(AccessTools.Method(declaringType, n), prefix, postfix, transpiler)));
                    }
                }
            }
        }

        internal static IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();
            AudioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().First();
            BeatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().First();
            SongBPM = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First().currentBPM;
            BeatmapObjectCallbackController coreSetup = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().First();
            BeatmapData beatmapData = coreSetup.GetField<BeatmapData, BeatmapObjectCallbackController>("_beatmapData");

            BeatmapObjectManager.noteWasCutEvent -= NoteColorManager.ColorizeSaber;
            BeatmapObjectManager.noteWasCutEvent += NoteColorManager.ColorizeSaber;

            if (ChromaConfig.Instance.LightshowModifier)
            {
                foreach (BeatmapLineData b in beatmapData.beatmapLinesData)
                {
                    b.beatmapObjectsData = b.beatmapObjectsData.Where((source, index) => b.beatmapObjectsData[index].beatmapObjectType != BeatmapObjectType.Note).ToArray();
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

                SaberColorizer.InitializeSabers(Resources.FindObjectsOfTypeAll<Saber>());

                // please let me kill legacy
                LegacyLightHelper.Activate(beatmapData.beatmapEventData);
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
                SaberColorizer.CurrentAColor = null;
                SaberColorizer.CurrentBColor = null;
            }
        }
    }
}
