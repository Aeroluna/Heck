namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(NoteCutSoundEffectManager))]
    [HeckPatch("Start")]
    internal static class NoteCutSoundEffectManagerStart
    {
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
        private static bool Prefix(NoteController noteController)
        {
            if (FakeNoteHelper.GetFakeNote(noteController))
            {
                return NoodleManager!.ProcessHitSound(noteController);
            }

            return false;
        }
    }

    // Weird cut sound shenanigans to prevent unity from crashing.
    internal class NoodleCutSoundEffectManager : MonoBehaviour
    {
        private readonly List<NoteController> _hitsoundQueue = new List<NoteController>();

        private NoteCutSoundEffectManager? _noteCutSoundEffectManager;
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
            else
            {
                _hitsoundQueue.Add(noteController);
                return false;
            }
        }

        private void Update()
        {
            if (_hitsoundQueue.Count > 0 && Time.frameCount != _lastFrame)
            {
                List<NoteController> noteControllers = new List<NoteController>(_hitsoundQueue);
                _hitsoundQueue.Clear();
                noteControllers.ForEach(_noteCutSoundEffectManager!.HandleNoteWasSpawned);
                Plugin.Logger.Log($"{noteControllers.Count} cut sounds moved to next frame!");
            }
        }
    }
}
