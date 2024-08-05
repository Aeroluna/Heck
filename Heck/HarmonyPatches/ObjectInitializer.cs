using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck.ObjectInitialize;
using UnityEngine;
using Zenject;

namespace Heck.HarmonyPatches;

[HeckPatch(PatchType.Features)]
internal static class ObjectInitializer
{
    private static readonly MethodInfo _fromComponentInNewPrefab =
        AccessTools.Method(typeof(FactoryFromBinderBase), nameof(FactoryFromBinderBase.FromComponentInNewPrefab));

    private static readonly MethodInfo _fromInitializer = AccessTools.Method(
        typeof(ObjectInitializer),
        nameof(FromInitializer));

    [HarmonyPriority(Priority.Low)]
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BeatmapObjectsInstaller), nameof(BeatmapObjectsInstaller.InstallBindings))]
    private static IEnumerable<CodeInstruction> BeatmapObjectsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return FromInitializerTranspiler(instructions);
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(FakeMirrorObjectsInstaller), nameof(FakeMirrorObjectsInstaller.InstallBindings))]
    private static IEnumerable<CodeInstruction> FakeMirrorObjectsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return FromInitializerTranspiler(instructions);
    }

    private static void FromInitializer(FactoryFromBinderBase binderBase, Object prefab)
    {
        switch (binderBase)
        {
            case FactoryFromBinder<GameNoteController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateGameNoteController<GameNoteController>(prefab));
                break;
            case FactoryFromBinder<MirroredGameNoteController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateGameNoteController<MirroredGameNoteController>(prefab));
                break;
            case FactoryFromBinder<MultiplayerConnectedPlayerGameNoteController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateGameNoteController<MultiplayerConnectedPlayerGameNoteController>(prefab));
                break;
            case FactoryFromBinder<BombNoteController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateBombNoteController<BombNoteController>(prefab));
                break;
            case FactoryFromBinder<MirroredBombNoteController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateBombNoteController<MirroredBombNoteController>(prefab));
                break;
            case FactoryFromBinder<MultiplayerConnectedPlayerBombNoteController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateBombNoteController<MultiplayerConnectedPlayerBombNoteController>(prefab));
                break;
            case FactoryFromBinder<ObstacleController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateObstacleController<ObstacleController>(prefab));
                break;
            case FactoryFromBinder<MirroredObstacleController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateObstacleController<MirroredObstacleController>(prefab));
                break;
            case FactoryFromBinder<MultiplayerConnectedPlayerObstacleController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateObstacleController<MultiplayerConnectedPlayerObstacleController>(prefab));
                break;
            case FactoryFromBinder<BurstSliderGameNoteController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateGameNoteController<BurstSliderGameNoteController>(prefab));
                break;
            case FactoryFromBinder<SliderController> binder:
                binder.FromResolveGetter<ObjectInitializerManager>(
                    n => n.CreateSliderController<SliderController>(prefab));
                break;
            default:
                // fallback
                binderBase.FromComponentInNewPrefab(prefab);
                break;
        }
    }

    private static IEnumerable<CodeInstruction> FromInitializerTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- base.Container.BindMemoryPool<BombNoteController, BombNoteController.Pool>().WithInitialSize(35).FromComponentInNewPrefab(this._bombNotePrefab);
             * ++ FromInitializer(base.Container.BindMemoryPool<BombNoteController, BombNoteController.Pool>().WithInitialSize(35), this._bombNotePrefab);
             * repeating. im not gonna list every single one changed
             */
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _fromComponentInNewPrefab))
            .Repeat(
                matcher => matcher
                    .SetAndAdvance(OpCodes.Call, _fromInitializer)
                    .RemoveInstruction())
            .InstructionEnumeration();
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyTranspiler]
    [HarmonyPatch(
        typeof(MultiplayerConnectedPlayerInstaller),
        nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
    private static IEnumerable<CodeInstruction> MultiplayerConnectedPlayerTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return FromInitializerTranspiler(instructions);
    }
}
