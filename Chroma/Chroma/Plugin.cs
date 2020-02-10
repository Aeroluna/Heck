using IPA;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace Chroma
{
    public class Plugin : IBeatSaberPlugin
    {
        private ChromaPlugin chroma;

        public void OnApplicationStart()
        {
            chroma = ChromaPlugin.Instantiate(this);
        }

        public void Init(object thisIsNull, IPALogger pluginLogger)
        {
            ChromaLogger.logger = pluginLogger;
        }

        public void OnApplicationQuit()
        {
        }

        public void OnUpdate()
        {
            chroma.OnUpdate();
        }

        public void OnFixedUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
        }
    }
}