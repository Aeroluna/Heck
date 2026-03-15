using System.Collections.Generic;
using SiraUtil.Affinity;
using Zenject;

namespace Heck.HarmonyPatches;

/*
 * Delays creating the NoteCutSoundEffect until 1 second before the note time.
 * Many noodle maps will greatly increase the amount of notes on screen at any time,
 * causing all the audio voices to be taken up and many notes can have their cut sound skipped.
 */
internal class NoteCutSoundDelayer : ITickable, IAffinity
{
    private readonly List<NoteController> _queuedCutSounds = [];

    private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
    private readonly BeatmapCallbacksController _beatmapCallbacksController;
    private readonly BeatmapObjectManager _beatmapObjectManager;

    internal NoteCutSoundDelayer(
        NoteCutSoundEffectManager noteCutSoundEffectManager,
        BeatmapCallbacksController beatmapCallbacksController,
        BeatmapObjectManager beatmapObjectManager)
    {
        _noteCutSoundEffectManager = noteCutSoundEffectManager;
        _beatmapCallbacksController = beatmapCallbacksController;
        _beatmapObjectManager = beatmapObjectManager;
    }

    public void Tick()
    {
        if (_queuedCutSounds.Count <= 0)
        {
            return;
        }

        for (int i = _queuedCutSounds.Count - 1; i >= 0; i--)
        {
            if (_noteCutSoundEffectManager._noteCutSoundEffectPoolContainer.activeItems.Count >=
                NoteCutSoundEffectManager.kMaxNumberOfEffects)
            {
                return;
            }

            NoteController noteController = _queuedCutSounds[i];
            switch (noteController.noteData.time - _beatmapCallbacksController.songTime)
            {
                case < 0:
                    // the note was already cut, just skip this one
                    _queuedCutSounds.Remove(noteController);
                    break;
                case < 1:
                    _queuedCutSounds.Remove(noteController);
                    _noteCutSoundEffectManager.HandleNoteWasSpawned(noteController);
                    break;
            }
        }
    }

    private void QueuedCreate(NoteController noteController)
    {
        _queuedCutSounds.Insert(0, noteController);
    }

    /*
     * NoteCutSoundEffectManager.HandleNoteWasSpawned needs to be called in the order that notes are cut,
     * which thanks to noodle, is not always the order they were spawned in. Because of this, we rework
     * HandleNoteWasSpawned to be called right before it's time to be cut instead of when the note spawns.
     */
    [AffinityPostfix]
    [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.Start))]
    private void ReplaceSubscribe(NoteCutSoundEffectManager __instance)
    {
        _beatmapObjectManager.noteWasSpawnedEvent -= __instance.HandleNoteWasSpawned;
        _beatmapObjectManager.noteWasSpawnedEvent += QueuedCreate;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.OnDestroy))]
    private void RemoveSubscribe(NoteCutSoundEffectManager __instance)
    {
        _beatmapObjectManager.noteWasSpawnedEvent -= QueuedCreate;
    }

    /*[AffinityTranspiler]
    [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.HandleNoteWasSpawned))]
    private IEnumerable<CodeInstruction> RemoveLimit(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- if (activeItems.Count > 64)
             * ++ if (activeItems.Count > MAX_SOUNDS_AT_ONCE)
             #1#
            .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S))
            .Set(OpCodes.Ldc_I4, MAX_SOUNDS_AT_ONCE)
            .InstructionEnumeration();
    }*/
}
