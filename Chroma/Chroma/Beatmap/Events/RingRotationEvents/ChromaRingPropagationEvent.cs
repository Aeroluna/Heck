using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.Events {

    [Obsolete("Now handled with Custom JSON Data", true)]
    public class ChromaRingPropagationEvent : ChromaColourEvent {

        public static float ringPropagationMult = 1f;

        public ChromaRingPropagationEvent(BeatmapEventData data) : base(data, false, true, new Color[] { }) { }

        public override bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType) {
            /*PlayerController playerController = GameObject.FindObjectOfType<PlayerController>();
            if (playerController != null) {
                playerController.transform.root.rotation = Quaternion.Euler(A.r * 360, A.g * 360, A.b * 360);
                return true;
            }
            return false;*/
            ringPropagationMult = A.r + (A.g * 15f) + (A.b * 100f);
            //ringPropagationMult = A.r * 7.5f;
            return true;
        }

    }

}
