namespace Chroma
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Events;
    using Chroma.Settings;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using UnityEngine;

    internal class ChromaBehaviour : MonoBehaviour
    {
        private static ChromaBehaviour _instance = null;

        internal static float SongBPM { get; private set; }

        internal static AudioTimeSyncController ATSC { get; private set; }

        internal static bool LightingRegistered { get; set; }

        internal static bool LegacyOverride { get; set; }

        internal static ChromaBehaviour CreateNewInstance()
        {
            ClearInstance();

            GameObject instanceObject = new GameObject("Chroma_ChromaBehaviour");
            ChromaBehaviour behaviour = instanceObject.AddComponent<ChromaBehaviour>();
            _instance = behaviour;
            return behaviour;
        }

        private static void ClearInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

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
            SongBPM = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First().currentBPM;
            BeatmapObjectCallbackController coreSetup = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().First();
            BeatmapData beatmapData = coreSetup.GetField<BeatmapData, BeatmapObjectCallbackController>("_beatmapData");

            if (ChromaConfig.Instance.LightshowModifier)
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

            // CustomJSONData
            if (LightingRegistered)
            {
                if (beatmapData is CustomBeatmapData customBeatmap)
                {
                    if (ChromaConfig.Instance.EnvironmentEnhancementsEnabled)
                    {
                        dynamic dynData = customBeatmap.beatmapCustomData;
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
                                        if (s == "TrackLaneRing" && n.name.Contains("Big"))
                                        {
                                            continue;
                                        }

                                        n.SetActive(false);
                                    }
                                }
                                else
                                {
                                    foreach (GameObject n in gameObjects.Where(obj => obj.name.Contains(s) && obj.scene.name.Contains("Environment") && !obj.scene.name.Contains("Menu")))
                                    {
                                        n.SetActive(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Legacy Chroma Events are handled by just sliding them in as if they were a normal rgb light event
            if (LegacyOverride)
            {
                ChromaLegacyRGBEvent.Activate(beatmapData.beatmapEventData);
            }

            if (LightingRegistered)
            {
                Extensions.SaberColorizer.InitializeSabers(FindObjectsOfType<Saber>());
            }
        }
    }
}
