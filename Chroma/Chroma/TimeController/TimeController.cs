using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using Chroma.Utils;

namespace Chroma.TimeController {

    public class TimeController : MonoBehaviour {

        public static TimeController Instantiate(ChromaBehaviour chroma) {
            GameObject go = new GameObject("ChromaTimeController");
            go.transform.SetParent(chroma.transform);
            TimeController timeCon = go.AddComponent<TimeController>();
            timeCon.chroma = chroma;

            return timeCon;
        }

        private ChromaBehaviour chroma;

        private AudioTimeSyncController AudioTimeSync { get; set; }
        private AudioSource _songAudio;

        private float baseTimeScale = 1f;

        private float modifiedTime = 1f; //Practice Plugin, etc.
        private float controlledTime = 1f;

        //private float powerupTimeScale = 1f;

        /*public float PowerupTimeMult {
            get { return powerupTimeScale; }
            set {
                powerupTimeScale = value;
                UpdateTimeScale(modifiedTime);
            }
        }*/
        /*public float ManipulatedTime {
            get {
                float t = 1;
                foreach (TimeManipulator manipulator in manipulators) {
                    manipulator.Manipulate(ref t);
                }
                return t;
            }
        }*/

        #region manipulators

        private List<TimeManipulator> manipulators = new List<TimeManipulator>();

        private float _manipulatedTime = 1f;
        public float ManipulatedTime {
            get {
                return _manipulatedTime;
            }
            private set {
                _manipulatedTime = value;
                UpdateTimeScale(modifiedTime);
            }
        }

        public void AddManipulator(TimeManipulator manipulator) {
            manipulators.Add(manipulator);
        }

        public void RemoveManipulator(TimeManipulator manipulator) {
            manipulators.Remove(manipulator);
            UpdateManipulatedTime();
        }

        public void UpdateManipulatedTime() {
            float t = 1;
            foreach (TimeManipulator manipulator in manipulators) {
                manipulator.Manipulate(ref t);
            }
            ManipulatedTime = t;
        }

        #endregion

        public float TimeMult {
            get {
                return baseTimeScale * ManipulatedTime;
            }
        }

        public float TimeScale {
            get {
                return controlledTime;
            }
        }

        public float OverrideTimeScale { get; set; } = -1f;

        void Start() {

            AudioTimeSync = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
            AudioTimeSync.forcedAudioSync = true;

            _songAudio = AudioTimeSync.GetField<AudioSource>("_audioSource");

            UpdateTimeScale(_songAudio.pitch);

            //if (mode.specialOptions.timeFluxOptions.Enabled) StartCoroutine(TimeFluxRoutine(mode.specialOptions.timeFluxOptions.fluxSpeed, mode.specialOptions.timeFluxOptions.minTime, mode.specialOptions.timeFluxOptions.maxTime));
        }

        void LateUpdate() {

            Time.timeScale = OverrideTimeScale >= 0f ? OverrideTimeScale : chroma.IsPaused ? 1f : TimeMult;

            if (!AudioTimeSync.forcedAudioSync) {
                UpdateTimeScale(1f);
            } else if (_songAudio.pitch != controlledTime) {
                UpdateTimeScale(_songAudio.pitch);
            }

        }

        void UpdateTimeScale(float modifiedTime) {
            this.modifiedTime = modifiedTime;
            controlledTime = modifiedTime * TimeMult;
            _songAudio.pitch = controlledTime;
            AudioTimeSync.forcedAudioSync = !Mathf.Approximately(_songAudio.pitch, 1f);
        }

        /*IEnumerator TimeFluxRoutine(float fluxSpeed, float minTime, float maxTime) {

            float startTime = Time.unscaledTime;
            TimeManipulator sinManipulator = new TimeManipulator(this, GetFluxScale(0, mode.specialOptions.timeFluxOptions.minTime, mode.specialOptions.timeFluxOptions.maxTime), TimeManipulator.Operation.MULTIPLY);

            while (true) {
                yield return new WaitForEndOfFrame();
                if (chroma.IsPaused) continue;
                sinManipulator.Value = GetFluxScale((Time.unscaledTime - startTime) * fluxSpeed, minTime, maxTime);
            }

        }

        private float GetFluxScale(float time, float minTime, float maxTime) {
            float t = Mathf.PerlinNoise((time / 8f), 0);
            //t = t * ((0.5f * (maxTime - minTime)) * Mathf.Sin(time) + (0.5f * (maxTime - minTime)));
            //return t + 1.5f;
            return minTime + (maxTime - minTime) * ((Mathf.Sin(time + t) / 2) + 0.5f);
        }*/

    }

}
