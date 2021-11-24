using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(NoteCutCoreEffectsSpawner))]
    [HeckPatch("Start")]
    internal static class NoteCutCoreEffectsSpawnerStart
    {
        internal static NoteCutCoreEffectsSpawner? NoteCutCoreEffectsSpawner { get; private set; }

        [UsedImplicitly]
        private static void Postfix(NoteCutCoreEffectsSpawner __instance)
        {
            NoteCutCoreEffectsSpawner = __instance;
        }
    }
}
