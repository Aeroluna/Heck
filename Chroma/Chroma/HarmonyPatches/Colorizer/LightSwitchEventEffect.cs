namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Colorizer;
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(LightSwitchEventEffect))]
    [HeckPatch("SetColor")]
    internal static class LightSwitchEventEffectSetColor
    {
        private static bool Prefix(BeatmapEventType ____event, Color color)
        {
            if (LightColorManager.LightIDOverride != null && ____event.TryGetLightColorizer(out LightColorizer lightColorizer))
            {
                int type = (int)____event;
                IEnumerable<int> newIds = LightColorManager.LightIDOverride.Select(n => LightIDTableManager.GetActiveTableValue(type, n) ?? n);
                foreach (int id in newIds)
                {
                    ILightWithId lightWithId = lightColorizer.Lights.ElementAtOrDefault(id);
                    if (lightWithId != null)
                    {
                        if (lightWithId.isRegistered)
                        {
                            lightWithId.ColorWasSet(color);
                        }
                    }
                    else
                    {
                        Plugin.Logger.Log($"Type [{type}] does not contain id [{id}].", IPA.Logging.Logger.Level.Warning);
                    }
                }

                LightColorManager.LightIDOverride = null;

                return false;
            }

            // Legacy Prop Id stuff
            if (LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride != null)
            {
                ILightWithId[] lights = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride;
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].ColorWasSet(color);
                }

                LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride = null;

                return false;
            }

            return true;
        }
    }

    [HeckPatch(typeof(LightSwitchEventEffect))]
    [HeckPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static ILightWithId[] LegacyLightOverride { get; set; }

        private static void Prefix(BeatmapEventData beatmapEventData, BeatmapEventType ____event)
        {
            if (beatmapEventData.type == ____event)
            {
                LightColorManager.ColorLightSwitch(beatmapEventData);
            }
        }
    }
}
