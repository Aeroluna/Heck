using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using UnityEngine;
using static Chroma.ChromaController;

namespace Chroma.EnvironmentEnhancement
{
    internal static class LegacyEnvironmentRemoval
    {
        internal static bool Init(CustomBeatmapData customBeatmap)
        {
            IEnumerable<string>? objectsToKill = customBeatmap.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Cast<string>();

            if (objectsToKill == null)
            {
                return false;
            }

            Plugin.Log.LogWarning("Legacy Environment Removal Detected...");
            Plugin.Log.LogWarning("Please do not use Legacy Environment Removal for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed");

            IEnumerable<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (string s in objectsToKill)
            {
                if (s is "TrackLaneRing" or "BigTrackLaneRing")
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

            return true;
        }
    }
}
