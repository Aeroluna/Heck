using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Animation;
using Heck.ReLoad;
using Zenject;

namespace Heck.HarmonyPatches
{
    [HeckPatch(PatchType.Features)]
    internal static class PatchedPlayerInstaller
    {
        private static readonly MethodInfo _getContainer = AccessTools.PropertyGetter(typeof(MonoInstallerBase), "Container");
        private static readonly MethodInfo _bindHeckMultiPlayer = AccessTools.Method(typeof(PatchedPlayerInstaller), nameof(BindHeckMultiPlayer));

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
        private static void GameplayCoreBinder(MonoInstallerBase __instance, GameplayCoreSceneSetupData ____sceneSetupData)
        {
            BindHeckSinglePlayer(____sceneSetupData, __instance.Container);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller), nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
        private static IEnumerable<CodeInstruction> MultiplayerConnectedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * PlayerSpecificSettingsNetSerializable playerSpecificSettingsForUserId = this._playersSpecificSettingsAtGameStartModel.GetPlayerSpecificSettingsForUserId(this._connectedPlayer.userId);
                 * ++ BindHeckMultiPlayer(playerSpecificSettingsForUserId, base.Container);
                 */
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
            DiContainer container)
        {
            IReadonlyBeatmapData untransformedBeatmapData;
            if (sceneSetupData is HeckinGameplayCoreSceneSetupData hecked)
            {
                untransformedBeatmapData = hecked.UntransformedBeatmapData;
            }
            else
            {
                Plugin.Log.LogError("Failed to get untransformedBeatmapData, falling back.");
                untransformedBeatmapData = sceneSetupData.transformedBeatmapData;
            }

            bool leftHanded = sceneSetupData.playerSpecificSettings.leftHanded;
            DeserializerManager.DeserializeBeatmapData(
                (CustomBeatmapData)sceneSetupData.transformedBeatmapData,
                untransformedBeatmapData,
                leftHanded,
                out Dictionary<string, Track> beatmapTracks,
                out HashSet<(object? Id, DeserializedData DeserializedData)> deserializedDatas);
            container.Bind<Dictionary<string, Track>>().FromInstance(beatmapTracks).AsSingle();
            deserializedDatas.Do(n =>
                container.BindInstance(n.DeserializedData)
                    .WithId(n.Id));
            container.BindInstance(deserializedDatas);

            container.Bind<ObjectInitializerManager>().AsSingle();

            container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(leftHanded);

            if (HeckController.DebugMode && sceneSetupData.practiceSettings != null)
            {
                container.BindInterfacesAndSelfTo<ReLoader>().AsSingle().NonLazy();
            }
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
