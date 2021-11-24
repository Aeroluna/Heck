using System.Collections.Generic;
using Heck;
using JetBrains.Annotations;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(NoteCutSoundEffectManager))]
    [HeckPatch("Start")]
    internal static class NoteCutSoundEffectManagerStart
    {
        [UsedImplicitly]
        private static void Postfix(NoteCutSoundEffectManager __instance)
        {
            NoodleCutSoundEffectManager noodleManager = __instance.gameObject.AddComponent<NoodleCutSoundEffectManager>();
            noodleManager.Init(__instance);
            NoteCutSoundEffectManagerHandleNoteWasSpawned.NoodleManager = noodleManager;
        }
    }

    [HeckPatch(typeof(NoteCutSoundEffectManager))]
    [HeckPatch("HandleNoteWasSpawned")]
    internal static class NoteCutSoundEffectManagerHandleNoteWasSpawned
    {
        internal static NoodleCutSoundEffectManager? NoodleManager { get; set; }

        // Do not create a NoteCutSoundEffect for fake notes
        [UsedImplicitly]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController) && NoodleManager!.ProcessHitSound(noteController);
        }
    }

    // Weird cut sound shenanigans to prevent unity from crashing.
    internal class NoodleCutSoundEffectManager : MonoBehaviour
    {
        private readonly List<NoteController> _hitsoundQueue = new();

        private NoteCutSoundEffectManager _noteCutSoundEffectManager = null!;
        private int _lastFrame = -1;
        private int _cutCount = -1;

        internal void Init(NoteCutSoundEffectManager noteCutSoundEffectManager)
        {
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
        }

        internal bool ProcessHitSound(NoteController noteController)
        {
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
                return true;
            }

            _hitsoundQueue.Add(noteController);
            return false;
        }

        private void Update()
        {
            if (_hitsoundQueue.Count <= 0 || Time.frameCount == _lastFrame)
            {
                return;
            }

            List<NoteController> noteControllers = new(_hitsoundQueue);
            _hitsoundQueue.Clear();
            noteControllers.ForEach(_noteCutSoundEffectManager.HandleNoteWasSpawned);
            Log.Logger.Log($"{noteControllers.Count} cut sounds moved to next frame!");
        }
    }
}
