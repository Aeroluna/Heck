using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.Events {

    public class ChromaAmbientLightEvent : ChromaColourEvent {

        public ChromaAmbientLightEvent(BeatmapEventData data) : base(data, true, false, new Color[] { }) { }

        public override bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType) {
            ColourManager.RecolourAmbientLights(A);
            return true;
        }

    }

}
