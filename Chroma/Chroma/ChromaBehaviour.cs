using Chroma.Events;
using Chroma.HarmonyPatches;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma
{
    internal class ChromaBehaviour : MonoBehaviour
    {
        private static ChromaBehaviour _instance = null;

        private static void ClearInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        internal static ChromaBehaviour CreateNewInstance()
        {
            ClearInstance();

            LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
            ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();

            GameObject instanceObject = new GameObject("Chroma_ChromaBehaviour");
            ChromaBehaviour behaviour = instanceObject.AddComponent<ChromaBehaviour>();
            _instance = behaviour;
            return behaviour;
        }

        internal static float songBPM = 120f;
        internal static AudioTimeSyncController ATSC;
        internal static bool LightingRegistered;
        internal static bool LegacyOverride;

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void Start()
        {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0f);
            ATSC = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().First();
            songBPM = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First().currentBPM;
            BeatmapObjectCallbackController coreSetup = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().First();
            BeatmapData beatmapData = coreSetup.GetField<BeatmapData, BeatmapObjectCallbackController>("_beatmapData");
            CheckTechnicolour(beatmapData);

            if (ChromaConfig.LightshowModifier)
            {
                foreach (BeatmapLineData b in beatmapData.beatmapLinesData)
                {
                    b.beatmapObjectsData = b.beatmapObjectsData.Where((source, index) => b.beatmapObjectsData[index].beatmapObjectType != BeatmapObjectType.Note).ToArray();
                }
                foreach (Saber saber in FindObjectsOfType<Saber>())
                {
                    saber.gameObject.SetActive(false);
                }
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Chroma");

                if (ChromaConfig.PlayersPlace) GameObject.Find("PlayersPlace")?.SetActive(false);
                if (ChromaConfig.Spectrograms) GameObject.Find("Spectrograms")?.SetActive(false);
                if (ChromaConfig.BackColumns) GameObject.Find("BackColumns")?.SetActive(false);
                if (ChromaConfig.Buildings) GameObject.Find("Buildings")?.SetActive(false);
            }

            // CustomJSONData
            if (LightingRegistered)
            {
                if (beatmapData is CustomBeatmapData _customBeatmap)
                {
                    if (ChromaConfig.EnvironmentEnhancementsEnabled)
                    {
                        dynamic dynData = _customBeatmap.beatmapCustomData;
                        List<object> objectsToKill = Trees.at(dynData, "_environmentRemoval");
                        if (objectsToKill != null)
                        {
                            GameObject[] gameObjects = FindObjectsOfType<GameObject>();
                            foreach (string s in objectsToKill?.Cast<string>())
                            {
                                if (s == "TrackLaneRing" || s == "BigTrackLaneRing")
                                {
                                    foreach (GameObject n in gameObjects.Where(obj => obj.name.Contains(s)))
                                    {
                                        if (s == "TrackLaneRing" && n.name.Contains("Big")) continue;
                                        n.SetActive(false);
                                    }
                                }
                                else
                                    foreach (GameObject n in gameObjects.Where(obj => obj.name.Contains(s) && obj.scene.name.Contains("Environment") && !obj.scene.name.Contains("Menu")))
                                    {
                                        n.SetActive(false);
                                    }
                            }
                        }
                    }
                    /*
                    Dictionary<string, List<CustomEventData>> _customEventData = _customBeatmap.customEventData;
                    foreach (KeyValuePair<string, List<CustomEventData>> n in _customEventData)
                    {
                        switch (n.Key)
                        {
                            case "SetObstacleColor":
                                ChromaObstacleColourEvent.Activate(n.Value);
                                break;

                            case "SetNoteColor":
                                if (ChromaConfig.NoteColourEventsEnabled) ChromaNoteColourEvent.Activate(n.Value);
                                break;

                            case "SetBombColor":
                                ChromaBombColourEvent.Activate(n.Value);
                                break;

                            case "SetLightColor":
                                ChromaLightColourEvent.Activate(n.Value);
                                break;
                        }
                    }*/
                }
            }

            // Legacy Chroma Events are handled by just sliding them in as if they were a normal rgb light event
            if (LegacyOverride) ChromaLegacyRGBEvent.Activate(beatmapData.beatmapEventData);

            if (LightingRegistered || ColourManager.TechnicolourLights) Extensions.SaberColourizer.InitializeSabers(FindObjectsOfType<Saber>());

            VFX.TechnicolourController.InitializeGradients();
        }

        private void CheckTechnicolour(BeatmapData beatmapData)
        {
            ColourManager.TechnicolourLightsForceDisabled = false;
            ColourManager.TechnicolourBlocksForceDisabled = false;
            ColourManager.TechnicolourBarriersForceDisabled = false;
            ColourManager.TechnicolourBombsForceDisabled = false;

            if (ChromaConfig.CustomColourEventsEnabled)
            {
                BeatmapEventData[] bevData = beatmapData.beatmapEventData;
                foreach (BeatmapEventData b in bevData)
                {
                    if (b is CustomBeatmapEventData customData)
                    {
                        dynamic dynData = customData.customData;
                        if (Trees.at(dynData, "_color") != null)
                        {
                            ColourManager.TechnicolourLightsForceDisabled = true;
                            continue;
                        }
                    }
                    if (b.value >= ChromaLegacyRGBEvent.RGB_INT_OFFSET) ColourManager.TechnicolourLightsForceDisabled = true;
                }

                BeatmapLineData[] bData = beatmapData.beatmapLinesData;
                foreach (BeatmapLineData b in bData)
                {
                    foreach (BeatmapObjectData beatmapObjectsData in b.beatmapObjectsData)
                    {
                        if (beatmapObjectsData is CustomNoteData customNoteData)
                        {
                            dynamic dynData = customNoteData.customData;
                            if (Trees.at(dynData, "_color") != null)
                            {
                                if (customNoteData.noteType == NoteType.Bomb) ColourManager.TechnicolourBombsForceDisabled = true;
                                else ColourManager.TechnicolourBlocksForceDisabled = ChromaConfig.NoteColourEventsEnabled;
                            }
                        }
                        else if (beatmapObjectsData is CustomObstacleData customObstacleData)
                        {
                            dynamic dynData = customObstacleData.customData;
                            if (Trees.at(dynData, "_color") != null) ColourManager.TechnicolourBarriersForceDisabled = true;
                        }
                    }
                }
            }
        }
    }
}