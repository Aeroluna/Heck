namespace NoodleExtensions.HarmonyPatches
{
    using UnityEngine;

    [NoodlePatch(typeof(NoteCutSoundEffectManager))]
    [NoodlePatch("HandleNoteWasSpawned")]
    internal static class NoteCutSoundEffectManagerHandleNoteWasSpawned
    {
        private static int _lastFrame = -1;
        private static int _cutCount = -1;

        // Do not create a NoteCutSoundEffect for fake notes
        private static bool Prefix(NoteController noteController)
        {
            if (FakeNoteHelper.GetFakeNote(noteController))
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

                // We do not allow more than 34 NoteCutSoundEffects to be created in a single frame to prevent unity from dying
                if (_cutCount < 34)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
