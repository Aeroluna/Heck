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
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.Animation
{
    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController _instance;

        internal static CustomEventCallbackController _customEventCallbackController;
        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            _customEventCallbackController = customEventCallbackController;
            _customEventCallbackController.AddCustomEventCallback(Dissolve.Callback);
            _customEventCallbackController.AddCustomEventCallback(DissolveArrow.Callback);
            _customEventCallbackController.AddCustomEventCallback(AnimateTrack.Callback);
            _customEventCallbackController.AddCustomEventCallback(AssignAnimation.Callback);

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
        private static readonly FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.Accessor _obstaclePoolAccessor = FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.GetAccessor("_obstaclePool");

        internal static Track GetTrack(CustomEventData customEventData)
        {
            string trackName = Trees.at(customEventData.data, TRACK);
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

        private static bool CompareTrack(BeatmapObjectData beatmapObjectData, Track track)
        {
            return Trees.at(((dynamic)beatmapObjectData).customData, "track") == track;
        }

        internal static IEnumerable<NoteController> GetActiveNotes(Track track = null)
        {
            BeatmapObjectManager objectManager = beatmapObjectManager;
            IEnumerable<NoteController> activeNotes = _noteAPoolAccessor(ref objectManager).activeItems
                .Union(_noteBPoolAccessor(ref objectManager).activeItems)
                .Union(_bombNotePoolAccessor(ref objectManager).activeItems);
            if (track != null) return activeNotes.Where(n => CompareTrack(n.noteData, track));
            return activeNotes;
        }

        internal static IEnumerable<ObstacleController> GetActiveObstacles(Track track = null)
        {
            BeatmapObjectManager objectManager = beatmapObjectManager;
            IEnumerable<ObstacleController> activeObstacles = _obstaclePoolAccessor(ref objectManager).activeItems;
            if (track != null) return activeObstacles.Where(n => CompareTrack(n.obstacleData, track));
            return activeObstacles;
        }

        internal static PointData DynamicToPointData(dynamic dyn)
        {
            IEnumerable<IEnumerable<float>> points = ((IEnumerable<object>)dyn)
                        ?.Cast<IEnumerable<object>>()
                        .Select(n => n.Select(Convert.ToSingle));
            if (points == null) return null;

            PointData pointData = new PointData();
            foreach (IEnumerable<float> rawPoint in points)
            {
                pointData.Add(new Vector4(rawPoint.ElementAt(0), rawPoint.ElementAt(1), rawPoint.ElementAt(2), rawPoint.ElementAt(3)));
            }
            return pointData;
        }
    }
}
