using Heck.Animation;

namespace Heck.HarmonyPatches.SceneTransition
{
    internal static class SceneTransitionHelper
    {
        internal static void Patch(PlayerSpecificSettings playerSpecificSettings)
        {
            AnimationHelper.LeftHandedMode = playerSpecificSettings.leftHanded;
        }
    }
}
