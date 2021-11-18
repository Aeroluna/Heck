namespace NoodleExtensions.HarmonyPatches
{
    using Heck;

    [HeckPatch(typeof(DisappearingArrowControllerBase<GameNoteController>))]
    [HeckPatch("SetArrowTransparency")]
    internal static class DisappearingArrowControllerSetArrowTransparency
    {
        // This makes _dissolveArrow work and I cannot figure out why
        private static void Postfix(CutoutEffect ____arrowCutoutEffect, float arrowTransparency)
        {
            ____arrowCutoutEffect.SetCutout(1f - arrowTransparency);
        }
    }
}
