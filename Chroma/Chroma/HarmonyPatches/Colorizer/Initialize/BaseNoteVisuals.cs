namespace Chroma.HarmonyPatches
{
    using System;
    using System.Linq;
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(BaseNoteVisuals))]
    [HarmonyPatch("Awake")]
    internal static class BaseNoteVisualsAwake
    {
        // I cant think of a better way to check if a NoteControllerBase is a bomb
        private static readonly Type[] _bombTypes = new Type[]
        {
            typeof(BombNoteController),
            typeof(MultiplayerConnectedPlayerBombNoteController),
            typeof(MirroredBombNoteController),
        };

        internal static bool IsBombType(NoteControllerBase noteController)
        {
            return _bombTypes.Contains(noteController.GetType());
        }

        [HarmonyPriority(Priority.High)]
        private static void Prefix(NoteControllerBase ____noteController)
        {
            if (IsBombType(____noteController))
            {
                new BombColorizer(____noteController);
            }
            else
            {
                new NoteColorizer(____noteController);
            }
        }
    }

    [HarmonyPatch(typeof(BaseNoteVisuals))]
    [HarmonyPatch("OnDestroy")]
    internal static class BaseNoteVisualsOnDestroy
    {
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(NoteControllerBase ____noteController)
        {
            if (BaseNoteVisualsAwake.IsBombType(____noteController))
            {
                BombColorizer.Colorizers.Remove(____noteController);
            }
            else
            {
                NoteColorizer.Colorizers.Remove(____noteController);
            }
        }
    }
}
