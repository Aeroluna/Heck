using HarmonyLib;
using IPA;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {
        internal const string CAPABILITY = "Noodle Extensions";
        internal const string HARMONYID_CORE = "com.noodle.BeatSaber.NoodleExtensionsCore";
        internal const string HARMONYID = "com.noodle.BeatSaber.NoodleExtensions";

        // All objects
        internal const string POSITION = "_position";

        internal const string ROTATION = "_rotation"; // Rotation events included

        // Wall exclusives
        internal const string LOCALROTATION = "_localRotation";

        internal const string SCALE = "_scale";

        // Note exclusives
        internal const string CUTDIRECTION = "_cutDirection";

        internal const string FLIP = "_flip";

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            Logger.logger = pluginLogger;
        }

        internal static Harmony coreharmony = new Harmony(HARMONYID_CORE);
        internal static Harmony harmony = new Harmony(HARMONYID);

        [OnStart]
        public void OnApplicationStart()
        {
            SongCore.Collections.RegisterCapability("Noodle Extensions");
            coreharmony.PatchAll(Assembly.GetExecutingAssembly());
            HarmonyPatches.BeatmapDataLoaderProcessBasicNotesInTimeRow.PatchBeatmapDataLoader(coreharmony);
            NoodleController.InitNoodlePatches();
        }
    }
}