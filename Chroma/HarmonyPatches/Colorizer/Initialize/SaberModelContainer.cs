using Chroma.Colorizer.Monobehaviours;
using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.Colorizer.Initialize
{
    [HarmonyPatch(typeof(SaberModelContainer))]
    [HarmonyPatch("Start")]
    internal static class SaberModelContainerStart
    {
        [UsedImplicitly]
        private static void Postfix(SaberModelContainer __instance, global::Saber ____saber)
        {
            __instance.gameObject.AddComponent<ChromaSaberController>().Init(____saber);
        }
    }
}
