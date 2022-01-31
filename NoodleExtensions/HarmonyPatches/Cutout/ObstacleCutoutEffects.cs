using NoodleExtensions.Managers;
using SiraUtil.Affinity;

namespace NoodleExtensions.HarmonyPatches.Cutout
{
    internal class ObstacleCutoutEffects : IAffinity
    {
        private readonly CutoutManager _cutoutManager;

        private ObstacleCutoutEffects(CutoutManager cutoutManager)
        {
            _cutoutManager = cutoutManager;
        }

        // cant patch Awake with Affinity
        [AffinityPostfix]
        [AffinityPatch(typeof(ObstacleDissolve), nameof(ObstacleDissolve.HandleObstacleDidInitEvent))]
        private void CreateCutoutWrapper(ObstacleController ____obstacleController, CutoutAnimateEffect ____cutoutAnimateEffect)
        {
            if (!_cutoutManager.ObstacleCutoutEffects.ContainsKey(____obstacleController))
            {
                _cutoutManager.ObstacleCutoutEffects.Add(____obstacleController, new CutoutAnimateEffectWrapper(____cutoutAnimateEffect));
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(ObstacleDissolve), nameof(ObstacleDissolve.OnDestroy))]
        private void DestroyCutoutWrapper(ObstacleController ____obstacleController)
        {
            _cutoutManager.ObstacleCutoutEffects.Remove(____obstacleController);
        }
    }
}
