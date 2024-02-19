using System.Collections.Generic;
using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace Heck.HarmonyPatches
{
    // Weird cut sound shenanigans to prevent unity from crashing.
    internal class NoteCutSoundLimiter : ITickable, IAffinity
    {
        private const int MAX_SOUNDS_PER_FRAME = 30;

        private readonly List<NoteController> _hitsoundQueue = new();

        private readonly SiraLog _log;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private int _lastFrame = -1;
        private int _cutCount = -1;

        internal NoteCutSoundLimiter(
            SiraLog log,
            NoteCutSoundEffectManager noteCutSoundEffectManager,
            AudioTimeSyncController audioTimeSyncController)
        {
            _log = log;
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
            _audioTimeSyncController = audioTimeSyncController;
        }

        public void Tick()
        {
            if (_hitsoundQueue.Count <= 0)
            {
                return;
            }

            List<NoteController> noteControllers = new(_hitsoundQueue);
            _hitsoundQueue.Clear();
            noteControllers.ForEach(_noteCutSoundEffectManager.HandleNoteWasSpawned);
            _log.Warn($"[{noteControllers.Count}] cut sounds moved to next frame!");
        }

        [AffinityPriority(Priority.Low)]
        [AffinityPrefix]
        [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.HandleNoteWasSpawned))]
        private void ProcessHitSound(NoteController noteController, ref bool __runOriginal)
        {
            // skip notes already passed, useful for reLoader
            if (noteController.noteData.time < _audioTimeSyncController.songTime)
            {
                __runOriginal = false;
                return;
            }

            if (!__runOriginal)
            {
                return;
            }

            if (Time.frameCount == _lastFrame)
            {
                _cutCount++;
            }
            else
            {
                _lastFrame = Time.frameCount;
                _cutCount = 1;
            }

            // We do not allow more than 30 NoteCutSoundEffects to be created in a single frame to prevent unity from dying
            if (_cutCount < MAX_SOUNDS_PER_FRAME)
            {
                return;
            }

            // too many, queue for next frame
            _hitsoundQueue.Add(noteController);
            __runOriginal = false;
        }
    }
}
