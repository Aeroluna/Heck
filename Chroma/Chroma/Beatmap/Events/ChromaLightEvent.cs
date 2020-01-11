using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.Events {

    public class ChromaLightEvent : ChromaColourEvent {

        public const int CHROMA_LIGHT_OFFSET = 1900000000;

        public ChromaLightEvent(BeatmapEventData data, Color a, Color b) : base(data, true, false, a, b) {

        }

        public override bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType) {
            ColourManager.RecolourLight(ref light, A == Color.clear ? ColourManager.LightA : A, B == Color.clear ? ColourManager.LightB : B);
            return true;
        }

    }

}
