using System.Collections.Generic;
using System.Linq;
using Heck.Deserialize;
using SiraUtil.Affinity;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class NoteLinker : IAffinity
    {
        private readonly DeserializedData _deserializedData;
        private readonly Dictionary<string, HashSet<NoteController>> _linkedNotes = new();
        private readonly Dictionary<NoteController, HashSet<NoteController>> _linkedLinkedNotes = new();

        private NoteLinker(
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData,
            BeatmapObjectManager beatmapObjectManager)
        {
            _deserializedData = deserializedData;
            beatmapObjectManager.noteWasSpawnedEvent += AddLink;
            beatmapObjectManager.noteWasDespawnedEvent += RemoveLink;
        }

        private void AddLink(NoteController noteController)
        {
            if (!_deserializedData.Resolve(noteController.noteData, out NoodleBaseNoteData? baseNoteData) || baseNoteData is not NoodleNoteData noodleData)
            {
                return;
            }

            string? link = noodleData.Link;
            if (link == null)
            {
                return;
            }

            if (!_linkedNotes.TryGetValue(link, out HashSet<NoteController>? linkedNotes))
            {
                linkedNotes = new HashSet<NoteController>();
                _linkedNotes.Add(link, linkedNotes);
            }

            linkedNotes.Add(noteController);
            _linkedLinkedNotes.Add(noteController, linkedNotes);
        }

        private void RemoveLink(NoteController noteController)
        {
            if (!_linkedLinkedNotes.TryGetValue(noteController, out HashSet<NoteController> linkedNotes))
            {
                return;
            }

            linkedNotes.Remove(noteController);
            _linkedLinkedNotes.Remove(noteController);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(NoteController), "SendNoteWasCutEvent")]
        private void TriggerLink(NoteController __instance, NoteCutInfo noteCutInfo)
        {
            if (!_linkedLinkedNotes.TryGetValue(__instance, out HashSet<NoteController> linkedNotes))
            {
                return;
            }

            linkedNotes.Remove(__instance);
            _linkedLinkedNotes.Remove(__instance);

            NoteController[] linked = linkedNotes.ToArray();
            linkedNotes.Clear();
            foreach (NoteController noteController in linked)
            {
                _linkedLinkedNotes.Remove(noteController);
            }

            foreach (NoteController noteController in linked)
            {
#pragma warning disable SA1117
                NoteCutInfo newInfo = new(noteController.noteData, noteCutInfo.speedOK, noteCutInfo.directionOK, noteCutInfo.saberTypeOK,
                    noteCutInfo.wasCutTooSoon, noteCutInfo.saberSpeed, noteCutInfo.saberDir, noteCutInfo.saberType,
                    noteCutInfo.timeDeviation, noteCutInfo.cutDirDeviation, noteCutInfo.cutPoint, noteCutInfo.cutNormal, noteCutInfo.cutDistanceToCenter,
                    noteCutInfo.cutAngle, noteCutInfo.worldRotation, noteCutInfo.inverseWorldRotation, noteCutInfo.noteRotation, noteCutInfo.notePosition,
                    noteCutInfo.saberMovementData);
#pragma warning restore SA1117
                noteController.SendNoteWasCutEvent(newInfo);
            }
        }
    }
}
