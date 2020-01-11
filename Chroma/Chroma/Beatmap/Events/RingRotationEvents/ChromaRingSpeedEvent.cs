using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Beatmap.Events {

    [Obsolete("Now handled with Custom JSON Data", true)]
    public class ChromaRingSpeedEvent : ChromaColourEvent {

        public static float ringSpeedMult = 1f;

        public ChromaRingSpeedEvent(BeatmapEventData data) : base(data, false, true, new Color[] { }) { }

        public override bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType) {
            /*PlayerController playerController = GameObject.FindObjectOfType<PlayerController>();
            if (playerController != null) {
                playerController.transform.root.rotation = Quaternion.Euler(A.r * 360, A.g * 360, A.b * 360);
                return true;
            }
            return false;*/
            ringSpeedMult = A.r + (A.g * 15f) + (A.b * 100f); //A.r > 0 ? A.r * 15f : 1f;
            return true;
        }

    }

}
