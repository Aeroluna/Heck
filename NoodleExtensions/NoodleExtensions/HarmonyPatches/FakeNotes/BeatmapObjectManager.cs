namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using UnityEngine;

    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BeatmapObjectManagerHandleNoteWasCut
    {
        private static readonly MethodInfo _despawnMethod = typeof(BeatmapObjectManager).GetMethod("Despawn", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(NoteController) }, null);
        private static NoteCutCoreEffectsSpawner _noteCutCoreEffectsSpawner;

        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(BeatmapObjectManager __instance, NoteController noteController, NoteCutInfo noteCutInfo)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (!FakeNoteHelper.GetFakeNote(noteController))
            {
                if (_noteCutCoreEffectsSpawner == null)
                {
                    _noteCutCoreEffectsSpawner = Resources.FindObjectsOfTypeAll<NoteCutCoreEffectsSpawner>().First();
                }

                _noteCutCoreEffectsSpawner.HandleNoteWasCut(noteController, noteCutInfo);
                _despawnMethod.Invoke(__instance, new object[] { noteController });

                return false;
            }

            return true;
        }
    }

    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("HandleNoteWasMissed")]
    internal static class BeatmapObjectManagerHandleNoteWasMissed
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
