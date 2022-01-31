using System;
using IPA.Utilities;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    internal class FakeNotePatches : IAffinity
    {
        private static readonly Action<BeatmapObjectManager, NoteController> _despawnMethod = MethodAccessor<BeatmapObjectManager, Action<BeatmapObjectManager, NoteController>>.GetDelegate("Despawn");

        private readonly FakePatchesManager _fakePatchesManager;
        private readonly NoteCutCoreEffectsSpawner _noteCutCoreEffectsSpawner;

        private FakeNotePatches(FakePatchesManager fakePatchesManager, NoteCutCoreEffectsSpawner noteCutCoreEffectsSpawner)
        {
            _fakePatchesManager = fakePatchesManager;
            _noteCutCoreEffectsSpawner = noteCutCoreEffectsSpawner;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BadNoteCutEffectSpawner), nameof(BadNoteCutEffectSpawner.HandleNoteWasCut))]
        private bool BadNoteSkip(NoteController noteController)
        {
            return _fakePatchesManager.GetFakeNote(noteController);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.HandleNoteControllerNoteWasCut))]
        private bool Prefix(BeatmapObjectManager __instance, NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (_fakePatchesManager.GetFakeNote(noteController))
            {
                return true;
            }

            _noteCutCoreEffectsSpawner.HandleNoteWasCut(noteController, noteCutInfo);
            _despawnMethod(__instance, noteController);

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.HandleNoteControllerNoteWasMissed))]
        private bool MissedNoteSkip(NoteController noteController)
        {
            return _fakePatchesManager.GetFakeNote(noteController);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BombCutSoundEffectManager), nameof(BombCutSoundEffectManager.HandleNoteWasCut))]
        private bool BombCutSoundSkip(NoteController noteController)
        {
            // Do not create a BombCutSoundEffect for fake notes
            return _fakePatchesManager.GetFakeNote(noteController);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(NoteCutScoreSpawner), nameof(NoteCutScoreSpawner.HandleNoteWasCut))]
        private bool NoteScoreSkip(NoteController noteController)
        {
            return _fakePatchesManager.GetFakeNote(noteController);
        }

        // Do not create a NoteCutSoundEffect for fake notes
        [AffinityPrefix]
        [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.HandleNoteWasSpawned))]
        private bool NoteCutSoundSkip(NoteController noteController)
        {
            return _fakePatchesManager.GetFakeNote(noteController);
        }
    }
}
