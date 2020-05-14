using HarmonyLib;
using IPA;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string CAPABILITY = "Noodle Extensions";
        internal const string HARMONYID_CORE = "com.noodle.BeatSaber.NoodleExtensionsCore";
        internal const string HARMONYID = "com.noodle.BeatSaber.NoodleExtensions";

        #region All Objects
        internal const string POSITION = "_position";
        internal const string ROTATION = "_rotation"; // Rotation events included

        internal const string NOTEJUMPSPEED = "_noteJumpMovementSpeed";
        internal const string SPAWNOFFSET = "_noteJumpStartBeatOffset";

        internal const string DESPAWNTIME = "_despawnTime";
        internal const string DESPAWNDURATION = "_despawnDuration";
        internal const string VARIABLEROTATION = "_variableRotation";
        internal const string VARIABLELOCALROTATION = "_variableLocalRotation";
        #endregion

        #region Wall Exclusive
        internal const string LOCALROTATION = "_localRotation";
        internal const string SCALE = "_scale";
        #endregion

        #region Note Exclusive
        internal const string CUTDIRECTION = "_cutDirection";
        internal const string FLIP = "_flip";
        #endregion

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            Logger.logger = pluginLogger;
            NoodleController.InitNoodlePatches();
        }

        internal static readonly Harmony coreharmony = new Harmony(HARMONYID_CORE);
        internal static readonly Harmony harmony = new Harmony(HARMONYID);

        [OnEnable]
        public void OnEnable()
        {
            SongCore.Collections.RegisterCapability(CAPABILITY);
            coreharmony.PatchAll(Assembly.GetExecutingAssembly());
            HarmonyPatches.BeatmapDataLoaderProcessNotesInTimeRow.PatchBeatmapDataLoader(coreharmony);
        }

        [OnDisable]
        public void OnDisable()
        {
            SongCore.Collections.DeregisterizeCapability(CAPABILITY);
            coreharmony.UnpatchAll(HARMONYID_CORE);
            coreharmony.UnpatchAll(HARMONYID);
        }
    }
}