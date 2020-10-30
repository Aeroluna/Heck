namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteCutCoreEffectsSpawner))]
    [NoodlePatch("Start")]
    internal static class NoteCutCoreEffectsSpawnerStart
    {
        internal static NoteCutCoreEffectsSpawner NoteCutCoreEffectsSpawner { get; private set; }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(NoteCutCoreEffectsSpawner __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            NoteCutCoreEffectsSpawner = __instance;
        }
    }
}
