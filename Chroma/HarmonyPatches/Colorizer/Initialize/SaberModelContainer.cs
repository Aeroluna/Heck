namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(SaberModelContainer))]
    [HarmonyPatch("Start")]
    internal static class SaberModelContainerStart
    {
        private static void Postfix(SaberModelContainer __instance, Saber ____saber)
        {
            __instance.gameObject.AddComponent<ChromaSaberController>().Init(____saber);
        }
    }
}
