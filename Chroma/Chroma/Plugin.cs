using Chroma.Settings;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chroma {

    public class Plugin : IPlugin {

        public string Name => "Chroma";
        public string Version => "1.0.1";

        ChromaPlugin chroma;

        public void OnApplicationStart() {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            chroma = ChromaPlugin.Instantiate(this);
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1) {
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) {
        }

        public void OnApplicationQuit() {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        public void OnLevelWasLoaded(int level) {

        }

        public void OnLevelWasInitialized(int level) {
        }

        public void OnUpdate() {
            chroma.OnUpdate();
        }

        public void OnFixedUpdate() {
        }

    }
}
