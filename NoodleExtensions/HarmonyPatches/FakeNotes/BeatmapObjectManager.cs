namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("HandleNoteControllerNoteWasCut")]
    internal static class BeatmapObjectManagerHandleNoteWasCut
    {
        private static readonly MethodInfo _despawnMethod = AccessTools.Method(typeof(BeatmapObjectManager), "Despawn", new Type[] { typeof(NoteController) });

        [HarmonyPriority(Priority.High)]
        private static bool Prefix(BeatmapObjectManager __instance, NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (!FakeNoteHelper.GetFakeNote(noteController))
            {
                NoteCutCoreEffectsSpawnerStart.NoteCutCoreEffectsSpawner!.HandleNoteWasCut(noteController, noteCutInfo);
                _despawnMethod.Invoke(__instance, new object[] { noteController });

                return false;
            }

            return true;
        }
    }

    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("HandleNoteControllerNoteWasMissed")]
    internal static class BeatmapObjectManagerHandleNoteWasMissed
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
