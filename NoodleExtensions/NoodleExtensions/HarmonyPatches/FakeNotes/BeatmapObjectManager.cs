namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Reflection;
    using HarmonyLib;

    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BeatmapObjectManagerHandleNoteWasCut
    {
        private static readonly MethodInfo _despawnMethod = typeof(BeatmapObjectManager).GetMethod("Despawn", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(NoteController) }, null);

        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(BeatmapObjectManager __instance, NoteController noteController, NoteCutInfo noteCutInfo)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (!FakeNoteHelper.GetFakeNote(noteController))
            {
                NoteCutCoreEffectsSpawnerStart.NoteCutCoreEffectsSpawner.HandleNoteWasCut(noteController, noteCutInfo);
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
