using Chroma.Events;
using Chroma.HarmonyPatches;
using Chroma.Settings;
using Chroma.Utils;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
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
            ChromaLogger.Log("ChromaBehaviour attempting creation.", ChromaLogger.Level.DEBUG);
            ClearInstance();

            LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
            ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();

            GameObject instanceObject = new GameObject("ChromaBehaviour");
            ChromaBehaviour behaviour = instanceObject.AddComponent<ChromaBehaviour>();
            _instance = behaviour;
            ChromaLogger.Log("ChromaBehaviour instantiated.", ChromaLogger.Level.DEBUG);
            return behaviour;
        }

        internal static float songBPM = 120f;
        internal static AudioTimeSyncController ATSC;
        internal static bool LightingRegistered;
        internal static bool LegacyOverride;

        private void OnDestroy()
        {
            ChromaLogger.Log("ChromaBehaviour destroyed.", ChromaLogger.Level.DEBUG);
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
            songBPM = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First().GetPrivateField<float>("_beatsPerMinute");
            BeatmapObjectCallbackController coreSetup = GetBeatmapObjectCallbackController();
            if (coreSetup != null)
            {
                ChromaLogger.Log("Found BOCC properly!", ChromaLogger.Level.DEBUG);
                GCSSFound(coreSetup);
            }

            Saber[] sabers = FindObjectsOfType<Saber>();
            if (sabers != null)
            {
                Extensions.SaberColourizer.InitializeSabers(sabers);
            }

            VFX.TechnicolourController.InitializeGradients();
        }

        private BeatmapObjectCallbackController GetBeatmapObjectCallbackController()
        {
            BeatmapObjectCallbackController s = FindObjectOfType<BeatmapObjectCallbackController>();
            if (s == null)
            {
                s = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().FirstOrDefault();
            }
            return s;
        }

        private void GCSSFound(BeatmapObjectCallbackController gcss)
        {
            ChromaLogger.Log("Found BOCC!", ChromaLogger.Level.DEBUG);

            if (gcss == null)
            {
                ChromaLogger.Log("Failed to obtain BeatmapObjectCallbackController", ChromaLogger.Level.WARNING);
                return;
            }

            //Map

            BeatmapData _beatmapData = gcss.GetPrivateField<BeatmapData>("_beatmapData");
            if (_beatmapData == null) ChromaLogger.Log("{XXX} : NULL BEATMAP DATA", ChromaLogger.Level.ERROR);
            BeatmapData beatmapData = CreateTransformedData(_beatmapData);
            if (beatmapData != null) gcss.SetPrivateField("_beatmapData", beatmapData);

            ColourManager.RefreshLights();

            if (ChromaConfig.LightshowModifier)
            {
                foreach (Saber saber in FindObjectsOfType<Saber>())
                {
                    saber.gameObject.SetActive(false);
                }

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
                    dynamic dynData = _customBeatmap.beatmapCustomData;
                    List<object> objectsToKill = Trees.at(dynData, "_environmentRemoval");
                    if (objectsToKill != null)
                    {
                        GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                        foreach (string s in objectsToKill?.Cast<string>())
                        {
                            foreach (GameObject n in gameObjects.Where(obj => obj.name.Contains(s) && obj.scene.name.Contains("Environment"))) {
                                n.SetActive(false);
                            }
                        }
                    }

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
                    }
                }

                // SimpleCustomEvents subscriptions
                // TODO: decide what i'm gonna do with simplecustomevents
                if (ChromaUtils.IsModInstalled("CustomEvents")) RegisterCustomEvents(gcss);
            }

            // Legacy Chroma Events are handled by just sliding them in as if they were a normal rgb light event
            ChromaLegacyRGBEvent.Activate(beatmapData.beatmapEventData);
        }

        private static void RegisterCustomEvents(BeatmapObjectCallbackController gcss)
        {
            CustomEvents.CustomEventCallbackController cecc = gcss.GetComponentInParent<CustomEvents.CustomEventCallbackController>();
            if (cecc == null) return;
            cecc.AddCustomEventCallback(ChromaGradientEvent.Callback, "AddGradient", 0);
            if (ChromaConfig.NoteColourEventsEnabled)
            {
                cecc.AddCustomEventCallback(ChromaSaberColourEvent.Callback, "SetSaberColor", 0);
            }
        }

        private static BeatmapData CreateTransformedData(BeatmapData beatmapData)
        {
            ColourManager.TechnicolourLightsForceDisabled = false;
            ColourManager.TechnicolourBlocksForceDisabled = false;
            ColourManager.TechnicolourBarriersForceDisabled = false;
            ColourManager.TechnicolourBombsForceDisabled = false;

            if (beatmapData == null) ChromaLogger.Log("Null beatmapData", ChromaLogger.Level.ERROR);

            if (ChromaConfig.LightshowModifier)
            {
                foreach (BeatmapLineData b in beatmapData.beatmapLinesData)
                {
                    b.beatmapObjectsData = b.beatmapObjectsData.Where((source, index) => b.beatmapObjectsData[index].beatmapObjectType != BeatmapObjectType.Note).ToArray();
                }
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Chroma");
            }

            /*
             * LIGHTING EVENTS
             */

            if (ChromaConfig.CustomColourEventsEnabled)
            {
                BeatmapEventData[] bevData = beatmapData.beatmapEventData;
                foreach (BeatmapEventData b in bevData)
                {
                    if (LightingRegistered && b is CustomBeatmapEventData customData)
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
                        if (LightingRegistered)
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

            return beatmapData;
        }
    }
}