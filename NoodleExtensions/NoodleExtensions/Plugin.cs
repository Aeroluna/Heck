using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine.SceneManagement;
using Harmony;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    public class Plugin : IBeatSaberPlugin
    {
        public const bool DebugMode = true;

        public void Init(object thisIsNull, IPALogger pluginLogger)
        {
            Logger.logger = pluginLogger;
        }

        public void OnApplicationStart()
        {
            var harmony = HarmonyInstance.Create("com.noodle.BeatSaber.NoodleExtensions");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }

        #endregion
    }
}
