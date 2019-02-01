using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Chroma.Utils {

    public class AudioUtil : MonoBehaviour {

        private static AudioUtil _instance;

        /// <summary>
        /// Returns the singleton AudioUtil MonoBehaviour.
        /// Will create a new one if it does not exist.
        /// </summary>
        public static AudioUtil Instance {
            get {
                if (_instance == null) {
                    ChromaLogger.Log("Initializing AudioBehaviour...", ChromaLogger.Level.DEBUG);
                    GameObject ob = new GameObject("ChromaAudioBehaviour");
                    _instance = ob.AddComponent<AudioUtil>();
                    DontDestroyOnLoad(ob);
                    _instance.Init();
                }
                return _instance;
            }
        }


        private List<AudioSource> oneShotPool = new List<AudioSource>();

        private AudioSource AvailableOneShot {
            get {
                for (int i = 0; i < oneShotPool.Count; i++) {
                    if (oneShotPool[i].isPlaying) continue;
                    return oneShotPool[i];
                }
                AudioSource newOneShot = gameObject.AddComponent<AudioSource>();
                MakeSourceNonDimensional(newOneShot, false);
                return newOneShot;
            }
        }

        AudioSource ambianceSource;

        private Dictionary<string, AudioClip> memorizedClips = new Dictionary<string, AudioClip>();

        void Init() {
            ambianceSource = gameObject.AddComponent<AudioSource>();
            MakeSourceNonDimensional(ambianceSource, true);

            AudioSource mainOneShot = AvailableOneShot;

            StartCoroutine(GenerateAudioClip(mainOneShot, "ConfigReload.wav"));
            StartCoroutine(GenerateAudioClip(ambianceSource, "RainLoop.wav"));

            //foreach (AudioSource a in GetComponents<AudioSource>()) MakeSourceNonDimensional(a);
        }

        float masterVolume = 1f;

        public void SetVolume(float masterVolume) {
            //oneShotSource.volume = masterVolume;
            //ambianceSource.volume = masterVolume;
            //altOneShotSource.volume = masterVolume;
            this.masterVolume = masterVolume;
        }

        IEnumerator GenerateAudioClip(AudioSource audioSource, string filenameWithExtension, bool play = false) {
            if (memorizedClips.ContainsKey(filenameWithExtension)) {
                audioSource.clip = memorizedClips[filenameWithExtension];
            } else {
                string filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/Audio/" + filenameWithExtension;
                ChromaLogger.Log("Searching for audio file " + filePath, ChromaLogger.Level.DEBUG, false);
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.WAV)) {
                    yield return www.SendWebRequest();
                    if (www.isHttpError || www.isNetworkError) {
                        ChromaLogger.Log(www.error, ChromaLogger.Level.ERROR, false);
                        yield break;
                    } else {
                        ChromaLogger.Log("Found sound " + filenameWithExtension, ChromaLogger.Level.ERROR, false);
                        AudioClip downloadedClip = DownloadHandlerAudioClip.GetContent(www);
                        if (downloadedClip == null) {
                            ChromaLogger.Log("Failed to find sound " + filePath, ChromaLogger.Level.WARNING, false);
                            yield break;
                        }
                        audioSource.clip = downloadedClip;
                        memorizedClips[filenameWithExtension] = downloadedClip;
                    }
                }
            }
            if (play) audioSource.Play();
            yield break;
        }

        /// <summary>
        /// Plays a sound in the Audio folder once
        /// </summary>
        /// <param name="filenameWithExtension">The file name with extension, found in the Audio folder</param>
        /// <param name="volume">Optional volume multiplier</param>
        /// <param name="pitch">Optional pitch multiplier</param>
        /// <returns>The Unity AudioSource used to play the sound</returns>
        public AudioSource PlayOneShotSound(string filenameWithExtension, float volume = 1f, float pitch = 1f) {
            AudioSource oneShotSource = AvailableOneShot;
            PlayOneShotSound(filenameWithExtension, oneShotSource);
            return oneShotSource;
        }

        private float lastTime = 0;
        /// <summary>
        /// Plays "Error.wav", with a delay in case of error spam.
        /// </summary>
        public void PlayErrorSound() {
            if (Time.unscaledTime < lastTime + 1f) return;
            lastTime = Time.unscaledTime;
            PlayOneShotSound("Error.wav");
        }

        /// <summary>
        /// Plays a sound in the Audio folder once, with a specific AudioSource
        /// </summary>
        /// <param name="filenameWithExtension">The file name with extension, found in the Audio folder</param>
        /// <param name="oneShotSource">The AudioSource to play the file through</param>
        /// <param name="volume">Optional volume multiplier</param>
        /// <param name="pitch">Optional pitch multiplier</param>
        public void PlayOneShotSound(string filenameWithExtension, AudioSource oneShotSource, float volume = 1f, float pitch = 1f) {
            oneShotSource.volume = masterVolume * volume;
            oneShotSource.pitch = pitch;
            StartCoroutine(GenerateAudioClip(oneShotSource, filenameWithExtension, true));
        }

        /// <summary>
        /// Plays "ConfigReload.wav" once.
        /// </summary>
        public void PlayReloadSound() {
            PlayOneShotSound("ConfigReload.wav");
        }

        /// <summary>
        /// Plays the given file through the ambient sound AudioSource, which loops
        /// </summary>
        /// <param name="filenameWithExtension">The file name with extension, found in the Audio folder</param>
        public void StartAmbianceSound(string filenameWithExtension) {
            StartCoroutine(GenerateAudioClip(ambianceSource, filenameWithExtension, true));
        }

        /// <summary>
        /// Stops the ambient AudioSource from playing.
        /// </summary>
        public void StopAmbianceSound() {
            ambianceSource.Stop();
        }

        /// <summary>
        /// Makes sounds created by the given AudioSource a global, non-directional source.
        /// </summary>
        /// <param name="source">The audio source to flatten</param>
        /// <param name="loop">Whether the audio source should loop or not.</param>
        public static void MakeSourceNonDimensional(AudioSource source, bool loop) {
            source.loop = loop;
            source.bypassEffects = true;
            source.bypassListenerEffects = true;
            source.bypassReverbZones = true;
            source.spatialBlend = 0;
            source.spatialize = false;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
        }

        /// <summary>
        /// Makes sounds created by the given AudioSource a global, non-directional source.
        /// </summary>
        /// <param name="source">The audio source to flatten</param>
        public static void MakeSourceNonDimensional(AudioSource source) {
            MakeSourceNonDimensional(source, source.loop);
        }

    }

}
