using HarmonyLib;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.SceneTransition
{
    // Force disable Noodle Extensions in tutorial scene
    [HarmonyPatch(typeof(TutorialScenesTransitionSetupDataSO))]
    [HarmonyPatch("Init")]
    internal static class TutorialScenesTransitionSetupDataSOInit
    {
        [UsedImplicitly]
        private static void Prefix()
        {
            NoodleController.ToggleNoodlePatches(false);
        }
    }
}
