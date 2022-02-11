using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Zenject;
using Object = UnityEngine.Object;

namespace Heck.HarmonyPatches
{
    [HeckPatch(PatchType.Features)]
    internal static class ObjectInitializer
    {
        private static readonly MethodInfo _fromComponentInNewPrefab =
            AccessTools.Method(typeof(FactoryFromBinderBase), nameof(FactoryFromBinderBase.FromComponentInNewPrefab));

        private static readonly MethodInfo _fromInitializer = AccessTools.Method(typeof(ObjectInitializer), nameof(FromInitializer));
        private static readonly MethodInfo _getContainer = AccessTools.PropertyGetter(typeof(MonoInstallerBase), "Container");

        [HarmonyPriority(Priority.Low)]
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BeatmapObjectsInstaller), nameof(BeatmapObjectsInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> BeatmapObjectsTranspiler(IEnumerable<CodeInstruction> instructions) =>
            FromInitializerTranspiler(instructions);

        [HarmonyPriority(Priority.Low)]
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FakeMirrorObjectsInstaller), nameof(FakeMirrorObjectsInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> FakeMirrorObjectsTranspiler(IEnumerable<CodeInstruction> instructions) =>
            FromInitializerTranspiler(instructions);

        [HarmonyPriority(Priority.Low)]
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller), nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> MultiplayerConnectedPlayerTranspiler(IEnumerable<CodeInstruction> instructions) =>
            FromInitializerTranspiler(instructions);

        private static IEnumerable<CodeInstruction> FromInitializerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _fromComponentInNewPrefab))
                .Repeat(matcher => matcher
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Callvirt, _getContainer),
                        new CodeInstruction(OpCodes.Call, _fromInitializer))
                    .RemoveInstructions(2))
                .InstructionEnumeration();
        }

        private static void FromInitializer(FactoryFromBinderBase binderBase, Object prefab, DiContainer container)
        {
            switch (binderBase)
            {
                case FactoryFromBinder<GameNoteController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateGameNoteController());
                    break;
                case FactoryFromBinder<MirroredCubeNoteController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateMirroredCubeNoteController());
                    break;
                case FactoryFromBinder<MultiplayerConnectedPlayerGameNoteController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateMultiplayerConnectedPlayerGameNoteController());
                    break;
                case FactoryFromBinder<BombNoteController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateBombNoteController());
                    break;
                case FactoryFromBinder<MirroredBombNoteController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateMirroredBombNoteController());
                    break;
                case FactoryFromBinder<MultiplayerConnectedPlayerBombNoteController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateMultiplayerConnectedPlayerBombNoteController());
                    break;
                case FactoryFromBinder<ObstacleController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateObstacleController());
                    break;
                case FactoryFromBinder<MirroredObstacleController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateMirroredObstacleController());
                    break;
                case FactoryFromBinder<MultiplayerConnectedPlayerObstacleController> binder:
                    binder.FromResolveGetter<ObjectInitializerManager>(n => n.CreateMultiplayerConnectedPlayerObstacleController());
                    break;
                default:
                    // fallback
                    binderBase.FromComponentInNewPrefab(prefab);
                    return;
            }

            // Bind prefab for intializer manager
            container.Bind(prefab.GetType()).FromInstance(prefab).AsSingle();
        }
    }
}
