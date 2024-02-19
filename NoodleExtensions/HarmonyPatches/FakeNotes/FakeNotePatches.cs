using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    internal class FakeNotePatches : IAffinity
    {
        private readonly FakePatchesManager _fakePatchesManager;
        private readonly NoteCutCoreEffectsSpawner? _noteCutCoreEffectsSpawner;

        private FakeNotePatches(
            SiraLog log,
            FakePatchesManager fakePatchesManager,
            [InjectOptional] GameObjectContext? context,
            [InjectOptional] NoteCutCoreEffectsSpawner? noteCutCoreEffectsSpawner)
        {
            _fakePatchesManager = fakePatchesManager;
            _noteCutCoreEffectsSpawner = context != null ? context.GetComponentInChildren<NoteCutCoreEffectsSpawner>() : noteCutCoreEffectsSpawner;

            if (_noteCutCoreEffectsSpawner == null)
            {
                log.Error($"Could not get [{nameof(NoteCutCoreEffectsSpawner)}]");
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.HandleNoteControllerNoteWasCut))]
        private bool Prefix(BeatmapObjectManager __instance, NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (_fakePatchesManager.GetFakeNote(noteController))
            {
                return true;
            }

            if (_noteCutCoreEffectsSpawner != null)
            {
                _noteCutCoreEffectsSpawner.HandleNoteWasCut(noteController, noteCutInfo);
            }

            __instance.Despawn(noteController);

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
