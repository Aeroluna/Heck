using Chroma.Settings;
using IPA;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chroma {

    public class Plugin : IBeatSaberPlugin {

        public string Name => "Chroma";
        
        public string Version => ChromaPlugin.Version.ToString();

        ChromaPlugin chroma;

        public void OnApplicationStart() {
            chroma = ChromaPlugin.Instantiate(this);
        }

        public void OnApplicationQuit() {
        }

        public void OnUpdate() {
            chroma.OnUpdate();
        }

        public void OnFixedUpdate() {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) {
        }

        public void OnSceneUnloaded(Scene scene) {
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene) {
        }

        /*public void OnApplicationStart() {
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
        }*/

    }
}
