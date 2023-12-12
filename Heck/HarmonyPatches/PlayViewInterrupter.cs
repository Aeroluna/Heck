using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck.PlayView;
using HMUI;
using JetBrains.Annotations;
using SiraUtil.Affinity;

namespace Heck.HarmonyPatches
{
    [HeckPatch]
    internal class PlayViewInterrupter : IAffinity
    {
        private static readonly FieldInfo _menuTransitionHelperField = AccessTools.Field(typeof(SinglePlayerLevelSelectionFlowCoordinator), "_menuTransitionsHelper");

        private static readonly ConstructorInfo _standardLevelParametersCtor = AccessTools.FirstConstructor(typeof(StartStandardLevelParameters), _ => true);
        private static readonly ConstructorInfo _multiplayerLevelParametersCtor = AccessTools.FirstConstructor(typeof(StartMultiplayerLevelParameters), _ => true);

        private static readonly MethodInfo _startStandardLevel = AccessTools.Method(
            typeof(MenuTransitionsHelper),
            nameof(MenuTransitionsHelper.StartStandardLevel),
            new[]
            {
                typeof(string), typeof(IDifficultyBeatmap), typeof(IPreviewBeatmapLevel),
                typeof(OverrideEnvironmentSettings), typeof(ColorScheme),
#if LATEST
                typeof(ColorScheme),
#endif
                typeof(GameplayModifiers),
                typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(string), typeof(bool), typeof(bool),
                typeof(Action), typeof(Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>),
#if LATEST
                typeof(Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>),
                typeof(RecordingToolManager.SetupData?)
#else
                typeof(Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>)
#endif
            });

        private static readonly MethodInfo _startMultiplayerLevel = AccessTools.Method(
            typeof(MenuTransitionsHelper),
            nameof(MenuTransitionsHelper.StartMultiplayerLevel),
            new[]
            {
                typeof(string), typeof(IPreviewBeatmapLevel), typeof(BeatmapDifficulty), typeof(BeatmapCharacteristicSO),
                typeof(IDifficultyBeatmap), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings),
                typeof(PracticeSettings), typeof(string), typeof(bool), typeof(Action),
                typeof(Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData>), typeof(Action<DisconnectedReason>)
            });

        private readonly PlayViewManager _playViewManager;
        private readonly LobbyGameStateController _lobbyGameStateController;
        private readonly LobbyGameStateModel _lobbyGameStateModel;

        private bool _playViewManagerHasRun;

        private PlayViewInterrupter(
            PlayViewManager playViewManager,
            ILobbyGameStateController lobbyGameStateController,
            LobbyGameStateModel lobbyGameStateModel)
        {
            _playViewManager = playViewManager;
            _lobbyGameStateController = (lobbyGameStateController as LobbyGameStateController)!;
            _lobbyGameStateModel = lobbyGameStateModel;
        }

        // Get all the parameters used to make a StartStandardLevelParameters
#pragma warning disable CS8321
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), nameof(SinglePlayerLevelSelectionFlowCoordinator.StartLevel))]
        private static StartStandardLevelParameters GetParameters(
            SinglePlayerLevelSelectionFlowCoordinator instance, Action beforeSceneSwitchCallback, bool practice)
        {
            throw new NotImplementedException("Reverse patch has not been executed.");

            [UsedImplicitly]
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _menuTransitionHelperField))
                    .Advance(-1)
                    .SetOpcodeAndAdvance(OpCodes.Nop)
                    .RemoveInstructions(1)
                    .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _startStandardLevel))
                    .SetInstruction(new CodeInstruction(OpCodes.Newobj, _standardLevelParametersCtor))
                    .InstructionEnumeration();
            }
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(LobbyGameStateController), nameof(LobbyGameStateController.StartMultiplayerLevel))]
        private static StartMultiplayerLevelParameters GetMultiplayerParameters(
            LobbyGameStateController instance, ILevelGameplaySetupData gameplaySetupData, IDifficultyBeatmap difficultyBeatmap, Action beforeSceneSwitchCallback)
        {
            throw new NotImplementedException("Reverse patch has not been executed.");

            [UsedImplicitly]
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .Start()
                    .RemoveInstructions(7)
                    .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _startMultiplayerLevel))
                    .SetInstruction(new CodeInstruction(OpCodes.Newobj, _multiplayerLevelParametersCtor))
                    .InstructionEnumeration();
            }
        }
#pragma warning restore CS8321

        [AffinityPrefix]
        [AffinityPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), nameof(SinglePlayerLevelSelectionFlowCoordinator.StartLevel))]
        private bool StartLevelPrefix(SinglePlayerLevelSelectionFlowCoordinator __instance, Action beforeSceneSwitchCallback, bool practice)
        {
            StartStandardLevelParameters parameters = GetParameters(__instance, beforeSceneSwitchCallback, practice);
            _playViewManager.Init(parameters);
            return false;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MultiplayerLevelLoader), nameof(MultiplayerLevelLoader.Tick))]
        private void WaitingForCountdownPostfix(MultiplayerLevelLoader.MultiplayerBeatmapLoaderState ____loaderState, ILevelGameplaySetupData ____gameplaySetupData, IDifficultyBeatmap ____difficultyBeatmap)
        {
            if (____loaderState != MultiplayerLevelLoader.MultiplayerBeatmapLoaderState.WaitingForCountdown ||
                _playViewManagerHasRun)
            {
                return;
            }

            if (_lobbyGameStateModel.gameState == MultiplayerGameState.Game)
            {
                _playViewManagerHasRun = false;
                return;
            }

            StartMultiplayerLevelParameters parameters = GetMultiplayerParameters(_lobbyGameStateController, ____gameplaySetupData, ____difficultyBeatmap, null!);
            _playViewManager.Init(parameters);
            _playViewManagerHasRun = true;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LobbyGameStateController), nameof(LobbyGameStateController.StartMultiplayerLevel))]
        private bool StartMultiplayer()
        {
            _playViewManagerHasRun = false;
            return _playViewManager.StartMultiplayer();
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(LobbyGameStateController), nameof(LobbyGameStateController.HandleMenuRpcManagerCancelledLevelStart))]
        private void CancelMultiplayerLevelStart()
        {
            _playViewManagerHasRun = false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), "LevelSelectionFlowCoordinatorTopViewControllerWillChange")]
        private bool LevelSelectionFlowCoordinatorTopViewControllerWillChangePrefix(
            SinglePlayerLevelSelectionFlowCoordinator __instance,
            ViewController newViewController,
            ViewController.AnimationType animationType)
        {
            PlayViewManager.PlayViewControllerData? controllerData = _playViewManager.ActiveView;
            if (newViewController != ((ViewController?)controllerData?.ViewController))
            {
                return true;
            }

            __instance.SetLeftScreenViewController(null, animationType);
            __instance.SetRightScreenViewController(null, animationType);
            __instance.SetBottomScreenViewController(null, animationType);
            __instance.SetTitle(null, animationType);
            FlowCoordinator flowCoordinator = __instance;
            flowCoordinator.showBackButton = true;
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), "BackButtonWasPressed")]
        private bool BackButtonWasPressedPrefix(SinglePlayerLevelSelectionFlowCoordinator __instance)
        {
            return _playViewManager.EarlyDismiss();
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MenuTransitionsHelper), "HandleMainGameSceneDidFinish")]
        private void HandleMainGameSceneDidFinishPrefix(LevelCompletionResults levelCompletionResults)
        {
            if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.Restart)
            {
                _playViewManager.FinishAll();
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MenuTransitionsHelper), "HandleMultiplayerLevelDidFinish")]
        [AffinityPatch(typeof(MenuTransitionsHelper), "HandleMultiplayerLevelDidDisconnect")]
        private void HandleMultiplayerLevelDidFinishPrefix()
        {
            _playViewManager.FinishAll();
        }
    }
}
