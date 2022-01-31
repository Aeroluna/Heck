using System.Collections.Generic;
using HarmonyLib;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    // Weird cut sound shenanigans to prevent unity from crashing.
    internal class NoteCutSoundLimiter : ITickable, IAffinity
    {
        private readonly List<NoteController> _hitsoundQueue = new();

        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private int _lastFrame = -1;
        private int _cutCount = -1;

        internal NoteCutSoundLimiter(NoteCutSoundEffectManager noteCutSoundEffectManager)
        {
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
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
            Log.Logger.Log($"{noteControllers.Count} cut sounds moved to next frame!");
        }

        // Do not create a NoteCutSoundEffect for fake notes
        [AffinityPriority(Priority.Low)]
        [AffinityPrefix]
        [AffinityPatch(typeof(NoteCutSoundEffectManager), "HandleNoteWasSpawned")]
        private void ProcessHitSound(NoteController noteController, ref bool __runOriginal)
        {
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
            if (_cutCount < 30)
            {
                return;
            }

            // too many, queue for next frame
            _hitsoundQueue.Add(noteController);
            __runOriginal = false;
        }
    }
}
