namespace NoodleExtensions.HarmonyPatches
{
    using Heck;

    [HeckPatch(typeof(ObstacleDissolve))]
    [HeckPatch("Awake")]
    internal static class ObstacleDissolveAwake
    {
        private static void Postfix(ObstacleController ____obstacleController, CutoutAnimateEffect ____cutoutAnimateEffect)
        {
            CutoutManager.ObstacleCutoutEffects.Add(____obstacleController, new CutoutAnimateEffectWrapper(____cutoutAnimateEffect));
        }
    }

    [HeckPatch(typeof(ObstacleDissolve))]
    [HeckPatch("OnDestroy")]
    internal static class ObstacleDissolveOnDestroy
    {
        private static void Postfix(ObstacleController ____obstacleController)
        {
            CutoutManager.ObstacleCutoutEffects.Remove(____obstacleController);
        }
    }
}
