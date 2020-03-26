using HarmonyLib;
using IPA;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {
        internal const string CAPABILITY = "Noodle Extensions";

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

        [OnStart]
        public void OnApplicationStart()
        {
            SongCore.Collections.RegisterCapability("Noodle Extensions");
            var harmony = new Harmony("com.noodle.BeatSaber.NoodleExtensions");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            HarmonyPatches.BeatmapDataLoaderProcessBasicNotesInTimeRow.PatchBeatmapDataLoader(harmony);
        }
    }
}