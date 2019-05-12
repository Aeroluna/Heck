using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.VFX {

    public class VFXLightning : MonoBehaviour {

        private static VFXLightning _instance;

        public static VFXLightning Instance {
            get { return _instance; }
        }

        public static VFXLightning InstanceOrDefault {
            get {
                if (_instance == null) {
                    GameObject instanceGameObject = new GameObject("VFXLightning");
                    _instance = instanceGameObject.AddComponent<VFXLightning>();
                    DontDestroyOnLoad(_instance.gameObject);
                    _instance.Init();
                }
                return _instance;
            }
        }

        private bool _ambientLightning = false;

        public bool AmbientLightning {
            get { return _ambientLightning; }
            set {
                _ambientLightning = value;
                if (ambientLightningRoutine != null) StopCoroutine(ambientLightningRoutine);
                if (value) ambientLightningRoutine = StartCoroutine(AmbientLightningLoop());
            }
        }

        private float[] _ambientLightningTimeRange = new float[] { 7f, 21f };

        public float AmbientLightningMinTime {
            get { return _ambientLightningTimeRange[0]; }
            set { _ambientLightningTimeRange[0] = value; }
        }

        public float AmbientLightningMaxTime {
            get { return _ambientLightningTimeRange[1]; }
            set { _ambientLightningTimeRange[1] = value; }
        }

        public Color LightningColour { get; set; } = new Color(0.3f, 0.3f, 0.3f);
        public Color AmbientLightningColour { get; set; } = new Color(0.08f, 0.08f, 0.08f);

        public float[] LightningVolumeRange = new float[] { 1f, 1f };
        public float[] AmbientLightningVolumeRange = new float[] { 0.25f, 0.45f };

        public float[] LightningPitchRange = new float[] { 0.85f, 1.05f };
        public float[] AmbientLightningPitchRange = new float[] { 0.85f, 1.05f };

        public string ThunderSound = "Thunder.wav";
        public string AmbientThunderSound = "Thunder.wav";

        BSLight[] lightningLights = null;

        private void Init() {

            try {

                if (lightningLights == null || lightningLights.Length < 1 || lightningLights[0] == null) {
                    LightSwitchEventEffect[] origLights = ColourManager.GetAllLightSwitches();
                    if (lightningLights != null) {
                        for (int j = 0; j < lightningLights.Length; j++) Destroy(lightningLights[j].gameObject);
                    }
                    List<BSLight> ll = new List<BSLight>();
                    for (int j = 0; j < origLights.Length; j++) {
                        BSLight[] origLl = origLights[j].GetField<BSLight[]>("_lights");
                        for (int k = 0; k < origLl.Length; k++) {
                            GameObject g = GameObject.Instantiate(origLl[k].gameObject);
                            BSLight nl = g.GetComponent<BSLight>();
                            nl.color = Color.clear;
                            nl.name = "CT_LightningLight_" + k;
                            foreach (Renderer r in nl.GetComponentsInChildren<Renderer>()) r.enabled = false;
                            ll.Add(nl);
                        }
                    }
                    lightningLights = ll.ToArray();

                    for (int i = 0; i < lightningLights.Length; i++) {
                        lightningLights[i].transform.SetParent(transform, true);
                    }

                    ChromaLogger.Log("VFXLightning set up with " + lightningLights.Length + " lights cloned.");
                }

            } catch (Exception e) {
                ChromaLogger.Log("Error initializing VFXLightning");
                ChromaLogger.Log(e);
            }


        }

        public void TriggerLightning(bool isAmbient) {
            Init();
            StartCoroutine(Lightning(isAmbient));
        }

        Coroutine ambientLightningRoutine = null;
        IEnumerator AmbientLightningLoop() {
            while (gameObject.activeSelf) {
                yield return new WaitForSeconds(UnityEngine.Random.Range(AmbientLightningMinTime, AmbientLightningMaxTime));
                TriggerLightning(true);
            }
        }

        IEnumerator Lightning(bool isAmbient) {
            AudioUtil.Instance.PlayOneShotSound("Thunder.wav");
            StartCoroutine(LightningFlashRoutine(LightningColour, isAmbient));

            //RandomSaberSwap();
            yield return new WaitForSeconds(0.105f);

            LightningFlashMidEvent?.Invoke(isAmbient);

            /*NoteJump[] movers = Resources.FindObjectsOfTypeAll<NoteJump>();
            foreach (NoteJump mover in movers) {
                Vector3 endPos = mover.GetField<Vector3>("_endPos");
                endPos.x = -endPos.x;
                mover.SetField("_endPos", endPos);
            }*/
        }

        //SimpleColorSO whiteCo;
        IEnumerator LightningFlashRoutine(Color flashColor, bool isAmbient) {
            LightningFlashEvent?.Invoke(isAmbient);

            System.Random rand = new System.Random();

            float t = 1.5f;
            float j;
            bool on = true;
            while (t > 0) {
                on = !on;

                if (on || rand.NextDouble() < 0.5) {
                    SetLightningFlashLights(flashColor);
                } else {
                    SetLightningFlashLights(Color.clear);
                }

                j = UnityEngine.Random.Range(0.05f, 0.1f) * (on ? 1f : 1.25f);
                t -= j;
                yield return new WaitForSeconds(j);
            }

            SetLightningFlashLights(Color.clear);
            LightningFlashEndedEvent?.Invoke(isAmbient);
            //NightmareLighting(invert);
        }

        private void SetLightningFlashLights(Color color) {
            foreach (BSLight l in lightningLights) {
                l.color = color;
            }
        }

        public delegate void LightningFlash(bool isAmbient);
        public event LightningFlash LightningFlashEvent;

        public delegate void LightningFlashMid(bool isAmbient);
        public event LightningFlashMid LightningFlashMidEvent;

        public delegate void LightningFlashEnded(bool isAmbient);
        public event LightningFlashEnded LightningFlashEndedEvent;

    }

}
