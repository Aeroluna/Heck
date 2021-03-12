namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(DisappearingArrowControllerBase<GameNoteController>))]
    [NoodlePatch("SetArrowTransparency")]
    internal static class DisappearingArrowControllerSetArrowTransparency
    {
        // This makes _dissolveArrow work and I cannot figure out why
        private static void Postfix(CutoutEffect ____arrowCutoutEffect, float arrowTransparency)
        {
            ____arrowCutoutEffect.SetCutout(1f - arrowTransparency);
        }
    }
}
