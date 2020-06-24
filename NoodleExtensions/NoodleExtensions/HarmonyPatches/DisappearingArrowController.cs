namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(DisappearingArrowController))]
    [NoodlePatch("SetArrowTransparency")]
    internal class DisappearingArrowControllerSetArrowTransparency
    {
        // This makes _dissolveArrow work and I cannot figure out why
        private static void Postfix(CutoutEffect ____arrowCutoutEffect, float arrowTransparency)
        {
            ____arrowCutoutEffect.SetCutout(1f - arrowTransparency);
        }
    }
}
