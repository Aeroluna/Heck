using HarmonyLib;

namespace Chroma.HarmonyPatches
{
    //TODO: Find a way to not make this run a hundred times
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