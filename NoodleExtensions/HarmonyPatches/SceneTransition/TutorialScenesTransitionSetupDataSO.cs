namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    // Force disable Noodle Extensions in tutorial scene
    [HarmonyPatch(typeof(TutorialScenesTransitionSetupDataSO))]
    [HarmonyPatch("Init")]
    internal static class TutorialScenesTransitionSetupDataSOInit
    {
        private static void Prefix()
        {
            NoodleController.ToggleNoodlePatches(false);
        }
    }
}
