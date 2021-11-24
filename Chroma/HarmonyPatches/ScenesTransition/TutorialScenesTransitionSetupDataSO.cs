using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.ScenesTransition
{
    // Force disable Chroma in tutorial scene
    [HarmonyPatch(typeof(TutorialScenesTransitionSetupDataSO))]
    [HarmonyPatch("Init")]
    internal static class TutorialScenesTransitionSetupDataSOInit
    {
        [UsedImplicitly]
        private static void Prefix()
        {
            ChromaController.ToggleChromaPatches(false);
            ChromaController.DoColorizerSabers = false;
        }
    }
}
