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
        [AffinityPatch(typeof(BadNoteCutEffectSpawner), nameof(BadNoteCutEffectSpawner.HandleNoteWasCut))]
        [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.HandleNoteWasSpawned))] // Do not create a NoteCutSoundEffect for fake notes
        [AffinityPatch(typeof(BombCutSoundEffectManager), nameof(BombCutSoundEffectManager.HandleNoteWasCut))]
        [AffinityPatch(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.HandleNoteControllerNoteWasMissed))]
        private bool NoteSkip(NoteController noteController)
        {
            return _fakePatchesManager.GetFakeNote(noteController);
        }
    }
}
