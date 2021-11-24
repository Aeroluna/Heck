using System.Reflection;
using HarmonyLib;
using Heck;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("HandleNoteControllerNoteWasCut")]
    internal static class BeatmapObjectManagerHandleNoteWasCut
    {
        private static readonly MethodInfo _despawnMethod = AccessTools.Method(typeof(BeatmapObjectManager), "Despawn", new[] { typeof(NoteController) });

        [HarmonyPriority(Priority.High)]
        private static bool Prefix(BeatmapObjectManager __instance, NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (FakeNoteHelper.GetFakeNote(noteController))
            {
                return true;
            }

            NoteCutCoreEffectsSpawnerStart.NoteCutCoreEffectsSpawner!.HandleNoteWasCut(noteController, noteCutInfo);
            _despawnMethod.Invoke(__instance, new object[] { noteController });

            return false;
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
