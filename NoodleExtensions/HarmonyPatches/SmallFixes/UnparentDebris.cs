using HarmonyLib;
using Heck;

namespace NoodleExtensions.HarmonyPatches.SmallFixes;

[HeckPatch(PatchType.Features)]
internal static class UnparentDebris
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NoteDebris), nameof(NoteDebris.Init))]
    private static void Unparent(NoteDebris __instance)
    {
        __instance.transform.SetParent(null, true);
    }
}
