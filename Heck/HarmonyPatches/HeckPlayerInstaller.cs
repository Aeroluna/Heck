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
        private static readonly MethodInfo _bindHeckPlayer = AccessTools.Method(typeof(HeckPlayerInstaller), nameof(BindHeckPlayer));

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
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getContainer),
                    new CodeInstruction(OpCodes.Call, _bindHeckPlayer))
                .InstructionEnumeration();
        }

        // TODO: fix multiplayer
        /*[HarmonyTranspiler]
        [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller), nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> MultiplayerConnectedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Same as the GameplayCoreInstaller except different locals & loads true for isMultiplayer
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createTransformedBeatmapData))
                .Advance(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, 12),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, _getBeatmapData),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getContainer),
                    new CodeInstruction(OpCodes.Call, _bindHeckPlayer))
                .InstructionEnumeration();
        }*/

        private static void BindHeckPlayer(
            GameplayCoreSceneSetupData sceneSetupData,
            PlayerSpecificSettings playerSpecificSettings,
            bool isMultiplayer,
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

            // TODO: swap strings out for const variables
            container.Bind<bool>().WithId("isMultiplayer").FromInstance(isMultiplayer);
            container.Bind<bool>().WithId("leftHanded").FromInstance(playerSpecificSettings.leftHanded);
        }
    }
}
