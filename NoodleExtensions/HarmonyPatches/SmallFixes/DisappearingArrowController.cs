using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(typeof(DisappearingArrowControllerBase<GameNoteController>))]
    [HeckPatch("SetArrowTransparency")]
    internal static class DisappearingArrowControllerSetArrowTransparency
    {
        // This makes _dissolveArrow work and I cannot figure out why
        [UsedImplicitly]
        private static void Postfix(CutoutEffect ____arrowCutoutEffect, float arrowTransparency)
        {
            ____arrowCutoutEffect.SetCutout(1f - arrowTransparency);
        }
    }
}
