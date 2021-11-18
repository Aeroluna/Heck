namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    // Force disable Chroma in tutorial scene
    [HarmonyPatch(typeof(TutorialScenesTransitionSetupDataSO))]
    [HarmonyPatch("Init")]
    internal static class TutorialScenesTransitionSetupDataSOInit
    {
        private static void Prefix()
        {
            ChromaController.ToggleChromaPatches(false);
            ChromaController.DoColorizerSabers = false;
        }
    }
}
