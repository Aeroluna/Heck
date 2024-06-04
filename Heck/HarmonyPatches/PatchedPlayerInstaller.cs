using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Animation;
using Heck.ReLoad;
using SiraUtil.Affinity;
using Zenject;

namespace Heck.HarmonyPatches
{
    internal class PatchedPlayerInstaller : IAffinity
    {
        private static readonly MethodInfo _getContainer = AccessTools.PropertyGetter(typeof(MonoInstallerBase), "Container");
        private static readonly MethodInfo _bindHeckMultiPlayer = AccessTools.Method(typeof(PatchedPlayerInstaller), nameof(BindHeckMultiPlayer));

        private readonly DeserializerManager _deserializerManager;

        private PatchedPlayerInstaller(DeserializerManager deserializerManager)
        {
            _deserializerManager = deserializerManager;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
        private void GameplayCoreBinder(MonoInstallerBase __instance, GameplayCoreSceneSetupData ____sceneSetupData)
        {
            BindHeckSinglePlayer(____sceneSetupData, __instance.Container);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(MultiplayerConnectedPlayerInstaller), nameof(MultiplayerConnectedPlayerInstaller.InstallBindings))]
        private IEnumerable<CodeInstruction> MultiplayerConnectedTranspiler(IEnumerable<CodeInstruction> instructions)
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

        private static void BindHeckMultiPlayer(
            PlayerSpecificSettingsNetSerializable playerSpecificSettings,
            DiContainer container)
        {
            container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(playerSpecificSettings.leftHanded);
        }

        private void BindHeckSinglePlayer(
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
                Plugin.Log.Debug("Failed to get untransformedBeatmapData, falling back");
                untransformedBeatmapData = sceneSetupData.transformedBeatmapData;
            }

            bool leftHanded = sceneSetupData.playerSpecificSettings.leftHanded;
            _deserializerManager.DeserializeBeatmapData(
                sceneSetupData.difficultyBeatmap,
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

            container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(leftHanded);

            if (HeckController.DebugMode && sceneSetupData.practiceSettings != null)
            {
                container.BindInterfacesAndSelfTo<ReLoader>().AsSingle().NonLazy();
            }
        }
    }
}
