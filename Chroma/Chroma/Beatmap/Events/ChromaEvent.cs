using Chroma.Settings;
using Chroma.VFX.ChromaToggle.VFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap.Events {

    public abstract class ChromaEvent {

        //Simple Events
        public const int CHROMA_EVENT_RING_ROTATE_LEFT = 1910000000;
        public const int CHROMA_EVENT_RING_ROTATE_RIGHT = 1910000001;
        public const int CHROMA_EVENT_LASER_RESET_STATE_ON = 1910000002;
        public const int CHROMA_EVENT_LASER_RESET_STATE_OFF = 1910000003;
        public const int CHROMA_EVENT_LASER_SPIN_DEFAULT = 1910000004;
        public const int CHROMA_EVENT_LASER_SPIN_INBOARD = 1910000005;
        public const int CHROMA_EVENT_LASER_SPIN_OUTBOARD = 1910000006;
        public const int CHROMA_EVENT_MAIN_LIGHTNING = 1910000007;
        public const int CHROMA_EVENT_AMBIENT_LIGHTNING = 1910000008;
        public const int CHROMA_EVENT_RING_ROTATE_RESET = 1910000009;

        //Data Events
        public const int CHROMA_EVENT_SCALE = 1950000001;
        public const int CHROMA_EVENT_HEALTH = 1950000002;
        public const int CHROMA_EVENT_ROTATE = 1950000003;
        public const int CHROMA_EVENT_AMBIENT_LIGHT = 1950000004;
        public const int CHROMA_EVENT_BARRIER_COLOUR = 1950000005;
        public const int CHROMA_EVENT_RING_SPEED_MULT = 1950000006;
        public const int CHROMA_EVENT_RING_PROPAGATION_MULT = 1950000007;
        public const int CHROMA_EVENT_RING_STEP_MULT = 1950000008;
        //public const int CHROMA_EVENT_CHANCE = 1950000009;

        private static Dictionary<BeatmapEventData, ChromaEvent> chromaEvents = new Dictionary<BeatmapEventData, ChromaEvent>();

        public static void ClearChromaEvents() {
            ChromaRingPropagationEvent.ringPropagationMult = 1f;
            ChromaRingSpeedEvent.ringSpeedMult = 1f;
            ChromaRingStepEvent.ringStepMult = 1f;

            ResetFlags();

            chromaEvents.Clear();

            ChromaBarrierColourEvent.Clear();
            ChromaNoteScaleEvent.Clear();
        }

        public static ChromaEvent SetChromaEvent(BeatmapEventData lightEvent, ChromaEvent chromaEvent) {
            if (chromaEvents.ContainsKey(lightEvent)) {
                chromaEvents.Remove(lightEvent);
            }
            chromaEvents.Add(lightEvent, chromaEvent);
            chromaEvent.OnEventSet(lightEvent);
            return chromaEvent;
        }

        public static void RemoveChromaEvent(BeatmapEventData beatmapEvent) {
            if (chromaEvents.ContainsKey(beatmapEvent)) chromaEvents.Remove(beatmapEvent);
        }

        public static ChromaEvent GetChromaEvent(BeatmapEventData beatmapEvent) {
            if (chromaEvents.ContainsKey(beatmapEvent)) {
                if (chromaEvents.TryGetValue(beatmapEvent, out ChromaEvent chromaEvent)) return chromaEvent;
            }
            return null;
        }

        protected BeatmapEventData data;
        public BeatmapEventData Data {
            get {
                return data;
            }
        }

        protected bool requiresColourEventsEnabled;
        protected bool requiresSpecialEventsEnabled;

        public bool RequiresColourEventsEnabled { get { return requiresColourEventsEnabled; } }
        public bool RequiresSpecialEventsEnabled { get { return requiresSpecialEventsEnabled; } }

        public ChromaEvent(BeatmapEventData data, bool requiresColourEventsEnabled, bool requiresSpecialEventsEnabled) {
            this.data = data;
            this.requiresColourEventsEnabled = requiresColourEventsEnabled;
            this.requiresSpecialEventsEnabled = requiresSpecialEventsEnabled;
        }

        public abstract bool Activate(ref LightSwitchEventEffect light, ref BeatmapEventData data, ref BeatmapEventType eventType);

        public virtual void OnEventSet(BeatmapEventData lightmapEvent) {

        }

        public static bool SimpleEventActivate(TrackLaneRingsRotationEffectSpawner tre, ref BeatmapEventData beatmapEventData, ref BeatmapEventType eventType) {
            int id = beatmapEventData.value;
            switch (id) {
                case CHROMA_EVENT_RING_ROTATE_RESET:
                    float ringPropWas = ChromaRingPropagationEvent.ringPropagationMult;
                    float ringSpeedWas = ChromaRingSpeedEvent.ringSpeedMult;
                    float ringStepWas = ChromaRingStepEvent.ringStepMult;
                    ChromaRingPropagationEvent.ringPropagationMult = 116f;
                    ChromaRingSpeedEvent.ringSpeedMult = 116f;
                    ChromaRingStepEvent.ringStepMult = 0;
                    tre.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger(
                        new BeatmapEventData(beatmapEventData.time, BeatmapEventType.Event8, CHROMA_EVENT_RING_ROTATE_RIGHT));
                    ChromaRingPropagationEvent.ringPropagationMult = ringPropWas;
                    ChromaRingSpeedEvent.ringSpeedMult = ringSpeedWas;
                    ChromaRingStepEvent.ringStepMult = ringStepWas;
                    ChromaLogger.Log("Ring reset called");
                    return true;
            }
            return false;
        }

        public static bool SimpleEventActivate(LightSwitchEventEffect lse, ref BeatmapEventData beatmapEventData, ref BeatmapEventType eventType) {
            int id = beatmapEventData.value;
            switch (id) {
                case CHROMA_EVENT_LASER_RESET_STATE_ON:
                    disablePositionReset = false;
                    return true;
                case CHROMA_EVENT_LASER_RESET_STATE_OFF:
                    disablePositionReset = true;
                    return true;
                case CHROMA_EVENT_LASER_SPIN_DEFAULT:
                    laserSpinDirection = 0;
                    return true;
                case CHROMA_EVENT_LASER_SPIN_INBOARD:
                    laserSpinDirection = 1;
                    return true;
                case CHROMA_EVENT_LASER_SPIN_OUTBOARD:
                    laserSpinDirection = -1;
                    return true;
                case CHROMA_EVENT_MAIN_LIGHTNING:
                    if (!ChromaConfig.CustomSpecialEventsEnabled) return true;
                    VFXLightning.InstanceOrDefault.TriggerLightning(false);
                    return true;
                case CHROMA_EVENT_AMBIENT_LIGHTNING:
                    if (!ChromaConfig.CustomSpecialEventsEnabled) return true;
                    VFXLightning.InstanceOrDefault.TriggerLightning(true);
                    return true;
            }
            return false;
        }

        /*
         * SIMPLE EVENT FLAGS
         */

        public static bool disablePositionReset = false;
        public static int laserSpinDirection = 0;

        private static void ResetFlags() {
            disablePositionReset = false;
            laserSpinDirection = 0;
        }

    }

}
