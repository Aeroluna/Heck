using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Zenject;

namespace Heck.HarmonyPatches
{
    [HeckPatch(PatchType.Features)]
    internal static class HeckPlayerInstaller
    {
        private static readonly FieldInfo _sceneSetupData = AccessTools.Field(typeof(GameplayCoreInstaller), "_sceneSetupData");
        private static readonly MethodInfo _transformedBeatmapData = AccessTools.PropertyGetter(typeof(GameplayCoreSceneSetupData), nameof(GameplayCoreSceneSetupData.transformedBeatmapData));
        private static readonly MethodInfo _getContainer = AccessTools.PropertyGetter(typeof(MonoInstallerBase), "Container");
        private static readonly MethodInfo _bindHeckSinglePlayer = AccessTools.Method(typeof(HeckPlayerInstaller), nameof(BindHeckSinglePlayer));
        private static readonly MethodInfo _bindHeckMultiPlayer = AccessTools.Method(typeof(HeckPlayerInstaller), nameof(BindHeckMultiPlayer));

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> GameplayCoreTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Loads all the fields necessary to call BindHeckPlayer
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _transformedBeatmapData))
                .ThrowLastError()
                .Advance(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _sceneSetupData),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getContainer),
                    new CodeInstruction(OpCodes.Call, _bindHeckSinglePlayer))
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller), nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> MultiplayerConnectedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getContainer),
                    new CodeInstruction(OpCodes.Call, _bindHeckMultiPlayer))
                .InstructionEnumeration();
        }

        private static void BindHeckSinglePlayer(
            GameplayCoreSceneSetupData sceneSetupData,
            PlayerSpecificSettings playerSpecificSettings,
            DiContainer container)
        {
            IReadonlyBeatmapData untransformedBeatmapData;
            if (sceneSetupData is HeckinGameplayCoreSceneSetupData hecked)
            {
                untransformedBeatmapData = hecked.UntransformedBeatmapData;
            }
            else
            {
                Log.Logger.Log("Failed to get untransformedBeatmapData, falling back.");
                untransformedBeatmapData = sceneSetupData.transformedBeatmapData;
            }

            DeserializerManager.DeserializeBeatmapDataAndBind(
                container,
                (CustomBeatmapData)sceneSetupData.transformedBeatmapData,
                untransformedBeatmapData);

            container.Bind<ObjectInitializerManager>().AsSingle();

            container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(playerSpecificSettings.leftHanded);
        }

        private static void BindHeckMultiPlayer(
            PlayerSpecificSettingsNetSerializable playerSpecificSettings,
            DiContainer container)
        {
            container.Bind<ObjectInitializerManager>().AsSingle();

            container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(playerSpecificSettings.leftHanded);
        }
    }
}
