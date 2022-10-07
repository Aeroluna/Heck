using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    // FUCK
    // this is done because processor needs the mirrored line index
    [HeckPatch(PatchType.Features)]
    internal static class LeftHandedMirrorNoteData
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NoteData), nameof(NoteData.Mirror))]
        [HarmonyPatch(typeof(SliderData), nameof(SliderData.Mirror))]
        private static void FlipLineIndex(BeatmapObjectData __instance)
        {
            static void Flip(CustomData customData, string key)
            {
                IEnumerable<float?>? position = customData.GetNullableFloats(key)?.ToList();
                float? x = position?.ElementAtOrDefault(0);
                if (x != null)
                {
                    customData[key] = new List<object?> { x.Value.MirrorLineIndex(), position!.ElementAtOrDefault(1) };
                }
            }

            static void Wipe(CustomData customData)
            {
                customData.TryRemove(INTERNAL_FLIPYSIDE, out _);
                customData.TryRemove(INTERNAL_FLIPLINEINDEX, out _);
                customData.TryRemove(INTERNAL_STARTNOTELINELAYER, out _);
                customData.TryRemove(INTERNAL_TAILSTARTNOTELINELAYER, out _);
            }

            switch (__instance)
            {
                case CustomNoteData noteData:
                    {
                        CustomData customData = noteData.customData;
                        Flip(customData, V2_POSITION);
                        Flip(customData, NOTE_OFFSET);
                        Flip(customData, V2_FLIP);
                        Flip(customData, FLIP);
                        Wipe(customData);
                    }

                    break;

                case CustomSliderData sliderData:
                    {
                        CustomData customData = sliderData.customData;
                        Flip(customData, NOTE_OFFSET);
                        Flip(customData, TAIL_NOTE_OFFSET);
                        Wipe(customData);
                    }

                    break;
            }
        }
    }
}
