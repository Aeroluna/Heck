namespace NoodleExtensions
{
    using static NoodleExtensions.Plugin;

    public static class NoodleController
    {
        public static bool NoodleExtensionsActive { get; private set; }

        public static void ToggleNoodlePatches(bool value)
        {
            if (value != NoodleExtensionsActive)
            {
                Heck.HeckData.TogglePatches(_harmonyInstance, value);

                NoodleExtensionsActive = value;
                if (NoodleExtensionsActive)
                {
                    CustomJSONData.CustomEventCallbackController.didInitEvent += Animation.AnimationController.CustomEventCallbackInit;
                }
                else
                {
                    CustomJSONData.CustomEventCallbackController.didInitEvent -= Animation.AnimationController.CustomEventCallbackInit;
                }
            }
        }
    }
}
