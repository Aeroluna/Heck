namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(DisappearingArrowController))]
    [NoodlePatch("SetArrowTransparency")]
    internal static class DisappearingArrowControllerSetArrowTransparency
    {
        // This makes _dissolveArrow work and I cannot figure out why
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(CutoutEffect ____arrowCutoutEffect, float arrowTransparency)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            ____arrowCutoutEffect.SetCutout(1f - arrowTransparency);
        }
    }
}
