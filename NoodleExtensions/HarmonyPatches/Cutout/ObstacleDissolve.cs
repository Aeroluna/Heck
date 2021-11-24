using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.Cutout
{
    [HeckPatch(typeof(ObstacleDissolve))]
    [HeckPatch("Awake")]
    internal static class ObstacleDissolveAwake
    {
        [UsedImplicitly]
        private static void Postfix(ObstacleController ____obstacleController, CutoutAnimateEffect ____cutoutAnimateEffect)
        {
            CutoutManager.ObstacleCutoutEffects.Add(____obstacleController, new CutoutAnimateEffectWrapper(____cutoutAnimateEffect));
        }
    }

    [HeckPatch(typeof(ObstacleDissolve))]
    [HeckPatch("OnDestroy")]
    internal static class ObstacleDissolveOnDestroy
    {
        [UsedImplicitly]
        private static void Postfix(ObstacleController ____obstacleController)
        {
            CutoutManager.ObstacleCutoutEffects.Remove(____obstacleController);
        }
    }
}
