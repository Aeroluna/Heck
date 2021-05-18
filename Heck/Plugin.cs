namespace Heck
{
    using System.Reflection;
    using HarmonyLib;
    using IPA;
    using IPALogger = IPA.Logging.Logger;

    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string HARMONYID = "com.aeroluna.BeatSaber.Heck";

        internal const string DURATION = "_duration";
        internal const string EASING = "_easing";
        internal const string NAME = "_name";
        internal const string POINTDEFINITIONS = "_pointDefinitions";
        internal const string POINTS = "_points";
        internal const string TIME = "_time";
        internal const string TRACK = "_track";

        internal const string ANIMATETRACK = "AnimateTrack";
        internal const string ASSIGNPATHANIMATION = "AssignPathAnimation";

        internal static readonly Harmony _harmonyInstance = new Harmony(HARMONYID);

        internal static HeckLogger Logger { get; private set; }

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            Logger = new HeckLogger(pluginLogger);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            CustomJSONData.CustomEventCallbackController.customEventCallbackControllerInit += Animation.AnimationController.CustomEventCallbackInit;
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmonyInstance.UnpatchAll(HARMONYID);
            CustomJSONData.CustomEventCallbackController.customEventCallbackControllerInit -= Animation.AnimationController.CustomEventCallbackInit;
        }
    }
}
