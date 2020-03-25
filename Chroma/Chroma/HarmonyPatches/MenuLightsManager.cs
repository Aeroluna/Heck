using Harmony;

namespace Chroma.HarmonyPatches
{
    //TODO: Find a way to not make this run a hundred times
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(MenuLightsManager))]
    [HarmonyPatch("SetColorsFromPreset")]
    internal class MenuLightsManagerSetColorsFromPreset
    {
        private static void Postfix()
        {
            ColourManager.RefreshLights();
        }
    }
}