using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.Events {

    public class ChromaChanceEvent : ChromaColourEvent {

        public float Chance {
            get { return A.r * A.g * A.b; }
        }

        public ChromaChanceEvent(BeatmapEventData data) : base(data, false, false, new Color[] { }) { }

        public override bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType) {
            return false;
        }

    }

}
