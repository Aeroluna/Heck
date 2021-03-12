namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteCutCoreEffectsSpawner))]
    [NoodlePatch("Start")]
    internal static class NoteCutCoreEffectsSpawnerStart
    {
        internal static NoteCutCoreEffectsSpawner NoteCutCoreEffectsSpawner { get; private set; }

        private static void Postfix(NoteCutCoreEffectsSpawner __instance)
        {
            NoteCutCoreEffectsSpawner = __instance;
        }
    }
}
