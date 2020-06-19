using CustomJSONData;
using System.Linq;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController instance { get; private set; }

        internal CustomEventCallbackController customEventCallbackController { get; private set; }

        internal BeatmapObjectSpawnController beatmapObjectSpawnController
        {
            get
            {
                if (_beatmapObjectSpawnController == null) _beatmapObjectSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
                return _beatmapObjectSpawnController;
            }
        }

        private BeatmapObjectSpawnController _beatmapObjectSpawnController;

        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            if (instance != null) Destroy(instance);
            instance = customEventCallbackController.gameObject.AddComponent<AnimationController>();

            instance.customEventCallbackController = customEventCallbackController;
            instance.customEventCallbackController.AddCustomEventCallback(AnimateTrack.Callback);
            instance.customEventCallbackController.AddCustomEventCallback(AssignPathAnimation.Callback);
        }
    }
}
