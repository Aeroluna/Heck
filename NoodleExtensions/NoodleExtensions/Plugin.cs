using Harmony;
using IPA;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    public class Plugin : IBeatSaberPlugin
    {
        public const bool DebugMode = true;

        internal static bool MappingExtensionsActive = false;
        internal static bool NoodleExtensionsActive = false;

        public void Init(object thisIsNull, IPALogger pluginLogger)
        {
            Logger.logger = pluginLogger;
        }

        public void OnApplicationStart()
        {
            SongCore.Collections.RegisterCapability("Mapping Extensions");
            SongCore.Collections.RegisterCapability("Noodle Extensions");
            var harmony = HarmonyInstance.Create("com.noodle.BeatSaber.NoodleExtensions");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "GameCore")
            {
                MappingExtensionsActive = CheckRequirements("Mapping Extensions");
                NoodleExtensionsActive = CheckRequirements("Noodle Extensions");
            }
        }

        private bool CheckRequirements(string capability)
        {
            var diff = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;
            var songData = SongCore.Collections.RetrieveDifficultyData(diff);
            return songData?.additionalDifficultyData._requirements.Contains(capability) ?? false;
        }

        #region Unused

        public void OnApplicationQuit()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void OnUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        #endregion Unused
    }
}