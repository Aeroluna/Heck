namespace Heck.HarmonyPatches
{
    using Heck.Animation;

    internal static class SceneTransitionHelper
    {
        internal static void Patch(PlayerSpecificSettings playerSpecificSettings)
        {
            AnimationHelper.LeftHandedMode = playerSpecificSettings.leftHanded;
        }
    }
}
