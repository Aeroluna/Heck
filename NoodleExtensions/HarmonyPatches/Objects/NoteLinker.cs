using System.Collections.Generic;
using System.Linq;
using Heck;
using IPA.Utilities;
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
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
        }

        private delegate void SendNoteWasCutEventDelegate(NoteController noteController, in NoteCutInfo noteCutInfo);

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteController), "Init")]
        private void AddLink(NoteController __instance, NoteData noteData)
        {
            if (!_deserializedData.Resolve(noteData, out NoodleBaseNoteData? baseNoteData) || baseNoteData is not NoodleNoteData noodleData)
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
                linkedNotes = new HashSet<NoteController>
                {
                    __instance
                };
                _linkedNotes.Add(link, linkedNotes);
            }
            else
            {
                linkedNotes.Add(__instance);
            }

            _linkedLinkedNotes.Add(__instance, linkedNotes);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteController), "SendNoteWasCutEvent")]
        private void TriggerLink(NoteController __instance, NoteCutInfo noteCutInfo)
        {
            if (!_linkedLinkedNotes.TryGetValue(__instance, out HashSet<NoteController> noteControllers))
            {
                return;
            }

            _linkedLinkedNotes.Remove(__instance);
            noteControllers.Remove(__instance);

            NoteController[] linked = noteControllers.ToArray();
            foreach (NoteController noteController in noteControllers)
            {
                _linkedLinkedNotes.Remove(noteController);
                noteControllers.Remove(noteController);
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

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectManager), "Despawn", AffinityMethodType.Normal, null, typeof(NoteController))]
        private void RemoveLink(NoteController noteController)
        {
            if (!_linkedLinkedNotes.TryGetValue(noteController, out HashSet<NoteController> linkedNotes))
            {
                return;
            }

            linkedNotes.Remove(noteController);
            _linkedLinkedNotes.Remove(noteController);
        }
    }
}
