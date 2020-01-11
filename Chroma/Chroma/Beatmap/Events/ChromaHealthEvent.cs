using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.Events {

    public class ChromaHealthEvent : ChromaColourEvent {

        public float HealthChangeAmount {
            get { return (A.r - 0.5f) * 2; }
        }

        public ChromaHealthEvent(BeatmapEventData data) : base(data, false, true, new Color[] { }) { }

        public override bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType) {
            GameEnergyCounter counter = GameObject.FindObjectOfType<GameEnergyCounter>();
            if (counter != null) {
                ChromaLogger.Log("Changing health by " + HealthChangeAmount);
                counter.InvokeMethod("AddEnergy", HealthChangeAmount);
                return true;
            }
            return false;
        }

    }

}
