using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController _instance;

        internal static CustomEventCallbackController _customEventCallbackController;
        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            _customEventCallbackController = customEventCallbackController;
            _customEventCallbackController.AddCustomEventCallback(Dissolve.Callback, 0, false);
            _customEventCallbackController.AddCustomEventCallback(DissolveArrow.Callback, 0, false);
            _customEventCallbackController.AddCustomEventCallback(TrackMovement.Callback, 0, false);

            if (_instance != null) Destroy(_instance);
            _instance = _customEventCallbackController.gameObject.AddComponent<AnimationController>();
        }

        private static Dictionary<string, Track> _tracks { get => ((CustomBeatmapData)_customEventCallbackController._beatmapData).customData.tracks; }

        private static BeatmapObjectManager _beatmapObjectManager;
        private static BeatmapObjectManager beatmapObjectManager
        {
            get
            {
                if (_beatmapObjectManager == null) _beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().First();
                return _beatmapObjectManager;
            }
        }
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteAPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteAPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteBPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteBPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _bombNotePoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_bombNotePool");

        internal static Track GetTrack(CustomEventData customEventData)
        {
            string trackName = Trees.at(customEventData.data, "_track");
            if (_tracks.TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                Logger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                return null;
            }
        }

        internal static bool CompareTrack(NoteController noteController, Track track)
        {
            return Trees.at(((CustomNoteData)noteController.noteData).customData, "track") == track;
        }

        internal static IEnumerable<NoteController> GetActiveBasicNotes(Track track)
        {
            BeatmapObjectManager objectManager = beatmapObjectManager;
            IEnumerable<NoteController> activeBasicNotes = _noteAPoolAccessor(ref objectManager).activeItems
                .Union(_noteBPoolAccessor(ref objectManager).activeItems);
            return activeBasicNotes.Where(n => CompareTrack(n, track));
        }
    }
}
