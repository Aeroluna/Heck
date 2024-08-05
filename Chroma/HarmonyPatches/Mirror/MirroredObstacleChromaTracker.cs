using Chroma.Colorizer;
using SiraUtil.Affinity;

namespace Chroma.HarmonyPatches.Mirror;

internal class MirroredObstacleChromaTracker : IAffinity
{
    private readonly ObstacleColorizerManager _manager;

    private MirroredObstacleChromaTracker(ObstacleColorizerManager manager)
    {
        _manager = manager;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(MirroredObstacleController), nameof(MirroredObstacleController.UpdatePositionAndRotation))]
    private void Postfix(MirroredObstacleController __instance, ObstacleController ____followedObstacle)
    {
        _manager.Colorize(__instance, _manager.GetColorizer(____followedObstacle).Color);
    }
}
