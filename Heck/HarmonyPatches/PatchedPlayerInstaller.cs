using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Animation;
using Heck.HarmonyPatches.UntransformedData;
using Heck.ReLoad;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using Zenject;
using DeserializerManager = Heck.Deserialize.DeserializerManager;

namespace Heck.HarmonyPatches
{
    internal class PatchedPlayerInstaller : IAffinity
    {
        private static readonly MethodInfo _getContainer = AccessTools.PropertyGetter(typeof(MonoInstallerBase), "Container");
        private static readonly MethodInfo _bindHeckMultiPlayer = AccessTools.Method(typeof(PatchedPlayerInstaller), nameof(BindHeckMultiPlayer));

        private readonly SiraLog _log;
        private readonly DeserializerManager _deserializerManager;

        private PatchedPlayerInstaller(SiraLog log, DeserializerManager deserializerManager)
        {
            _log = log;
            _deserializerManager = deserializerManager;
        }

        private static void BindHeckMultiPlayer(
            PlayerSpecificSettingsNetSerializable playerSpecificSettings,
            DiContainer container)
        {
            container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(playerSpecificSettings.leftHanded);
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

        private void BindHeckSinglePlayer(
            GameplayCoreSceneSetupData sceneSetupData,
            DiContainer container)
        {
            bool leftHanded = sceneSetupData.playerSpecificSettings.leftHanded;
            container.Bind<bool>().WithId(HeckController.LEFT_HANDED_ID).FromInstance(leftHanded);

            Dictionary<string, Track> beatmapTracks;
            HashSet<(object? Id, Deserialize.DeserializedData DeserializedData)> deserializedDatas;
            if (sceneSetupData.transformedBeatmapData is CustomBeatmapData customBeatmapData)
            {
                IReadonlyBeatmapData untransformedBeatmapData;
                if (sceneSetupData is HeckGameplayCoreSceneSetupData hecked)
                {
                    untransformedBeatmapData = hecked.UntransformedBeatmapData;
                }
                else
                {
                    _log.Debug("Failed to get untransformedBeatmapData, falling back");
                    untransformedBeatmapData = customBeatmapData;
                }

#if LATEST
                _deserializerManager.DeserializeBeatmapData(
                    sceneSetupData.beatmapLevel,
#else
                _deserializerManager.DeserializeBeatmapData(
                    sceneSetupData.difficultyBeatmap,
#endif
                    customBeatmapData,
                    untransformedBeatmapData,
                    leftHanded,
                    out beatmapTracks,
                    out deserializedDatas);

                if (HeckController.DebugMode && sceneSetupData.practiceSettings != null)
                {
                    container.BindInterfacesAndSelfTo<ReLoader>().AsSingle().NonLazy();
                }
            }
            else
            {
                deserializedDatas = _deserializerManager.EmptyDeserialize();
                beatmapTracks = new Dictionary<string, Track>();
            }

            container.Bind<Dictionary<string, Track>>().FromInstance(beatmapTracks).AsSingle();
            deserializedDatas.Do(n =>
                container.BindInstance(n.DeserializedData)
                    .WithId(n.Id));
            container.BindInstance(deserializedDatas);
        }
    }
}
