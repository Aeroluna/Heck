using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HMUI;
using JetBrains.Annotations;
using ModestTree;
using Zenject;

namespace Heck.PlayView;

public sealed class PlayViewManager : IDisposable
{
    private readonly bool[] _doPresent;
    private readonly GameServerLobbyFlowCoordinator _gameServerLobbyFlowCoordinator;
    private readonly LobbyGameStateController _lobbyGameStateController;
    private readonly MenuTransitionsHelper _menuTransitionsHelper;
    private readonly PartyFreePlayFlowCoordinator _partyFreePlayFlowCoordinator;
    private readonly SoloFreePlayFlowCoordinator _soloFreePlayFlowCoordinator;
    private readonly PlayViewControllerData[] _viewControllers;

    private int _currentIndex;
    private StartStandardLevelParameters? _currentParameters;

    private FlowCoordinator _flowCoordinator;
    private bool _modifiedParameters;

    [UsedImplicitly]
    private PlayViewManager(
        [Inject(Optional = true, Source = InjectSources.Local)]
        IEnumerable<IPlayViewController> viewControllers,
        SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator,
        PartyFreePlayFlowCoordinator partyFreePlayFlowCoordinator,
        GameServerLobbyFlowCoordinator gameServerLobbyFlowCoordinator,
        ILobbyGameStateController lobbyGameStateController,
        MenuTransitionsHelper menuTransitionsHelper)
    {
        _viewControllers =
            viewControllers.Select(n => new PlayViewControllerData(n)).OrderBy(n => n.Priority).ToArray();
        _doPresent = new bool[_viewControllers.Length];
        foreach (PlayViewControllerData playViewControllerData in _viewControllers)
        {
            IPlayViewController viewController = playViewControllerData.ViewController;
            viewController.Finished += Activate;
            if (viewController is IParameterModifier parameterModifier)
            {
                parameterModifier.ParametersModified += HandleParametersModified;
            }
        }

        _flowCoordinator = soloFreePlayFlowCoordinator;
        _soloFreePlayFlowCoordinator = soloFreePlayFlowCoordinator;
        _partyFreePlayFlowCoordinator = partyFreePlayFlowCoordinator;
        _gameServerLobbyFlowCoordinator = gameServerLobbyFlowCoordinator;
        _lobbyGameStateController = (LobbyGameStateController)lobbyGameStateController;
        _menuTransitionsHelper = menuTransitionsHelper;
    }

    internal PlayViewControllerData? ActiveView { get; private set; }

    public void Dispose()
    {
        foreach (PlayViewControllerData playViewControllerData in _viewControllers)
        {
            IPlayViewController viewController = playViewControllerData.ViewController;
            viewController.Finished -= Activate;
            if (viewController is IParameterModifier parameterModifier)
            {
                parameterModifier.ParametersModified -= HandleParametersModified;
            }
        }
    }

    // SEND IT BABY!!!!
    [PublicAPI]
    public void ForceStart(StartStandardLevelParameters startParameters)
    {
        Init(startParameters, true);
        StartStandard();
    }

    internal bool EarlyDismiss()
    {
        if (ActiveView == null)
        {
            return true;
        }

        ActiveView.InvokeOnEarlyDismiss();
        _flowCoordinator.DismissViewController((ViewController)ActiveView.ViewController);
        ActiveView = null;
        return false;
    }

    internal void FinishAll()
    {
        foreach (PlayViewControllerData playViewControllerData in _viewControllers)
        {
            playViewControllerData.InvokeOnFinish();
        }
    }

    internal void Init(StartStandardLevelParameters startParameters, bool forced = false)
    {
        _currentIndex = 0;
        _currentParameters = startParameters;
        _modifiedParameters = false;
        for (int i = 0; i < _viewControllers.Length; i++)
        {
            _doPresent[i] = _viewControllers[i].ViewController.Init(_currentParameters);
        }

        if (forced)
        {
            return;
        }

        if (startParameters is StartMultiplayerLevelParameters)
        {
            _flowCoordinator = _gameServerLobbyFlowCoordinator;
        }
        else
        {
            _flowCoordinator = _soloFreePlayFlowCoordinator.isActivated
                ? _soloFreePlayFlowCoordinator
                : _partyFreePlayFlowCoordinator;
        }

        Activate();
    }

    internal bool StartMultiplayer()
    {
        Dismiss();

        foreach (PlayViewControllerData playViewControllerData in _viewControllers)
        {
            playViewControllerData.InvokeOnPlay();
        }

        if (!_modifiedParameters ||
            _currentParameters is not StartMultiplayerLevelParameters multiplayerStartParameters)
        {
            return true;
        }

        _lobbyGameStateController.countdownStarted = false;
        _lobbyGameStateController.StopListeningToGameStart();

        _menuTransitionsHelper.StartMultiplayerLevel(
            multiplayerStartParameters.GameMode,
#if !PRE_V1_37_1
            multiplayerStartParameters.BeatmapKey,
            multiplayerStartParameters.BeatmapLevel,
            multiplayerStartParameters.BeatmapLevelData,
#else
            multiplayerStartParameters.PreviewBeatmapLevel,
            multiplayerStartParameters.BeatmapDifficulty,
            multiplayerStartParameters.BeatmapCharacteristic,
            multiplayerStartParameters.DifficultyBeatmap,
#endif
            multiplayerStartParameters.OverrideColorScheme,
            multiplayerStartParameters.GameplayModifiers,
            multiplayerStartParameters.PlayerSpecificSettings,
            multiplayerStartParameters.PracticeSettings,
            multiplayerStartParameters.BackButtonText,
            multiplayerStartParameters.UseTestNoteCutSoundEffects,
            multiplayerStartParameters.BeforeSceneSwitchCallback,
            multiplayerStartParameters.MultiplayerLevelFinishedCallback,
            multiplayerStartParameters.DidDisconnectCallback);
        return false;
    }

    private void Activate()
    {
        if (_currentParameters == null)
        {
            throw new InvalidOperationException();
        }

        while (_currentIndex < _viewControllers.Length)
        {
            int index = _currentIndex++;
            PlayViewControllerData viewControllerData = _viewControllers[index];
            IPlayViewController viewController = viewControllerData.ViewController;
            if (!_doPresent[index])
            {
                continue;
            }

            bool newView = ActiveView == null;
            ActiveView = viewControllerData;

            if (newView)
            {
                _flowCoordinator.PresentViewController((ViewController)viewController);
            }
            else
            {
                _flowCoordinator.ReplaceTopViewController((ViewController)viewController);
            }

            _flowCoordinator.SetTitle(viewControllerData.Title);

            viewControllerData.InvokeOnShow();

            return;
        }

        Dismiss();

        if (_currentParameters is not StartMultiplayerLevelParameters)
        {
            StartStandard();
        }
    }

    private void Dismiss()
    {
        if (ActiveView == null)
        {
            return;
        }

        _flowCoordinator.DismissViewController(
            (ViewController)ActiveView.ViewController,
            ViewController.AnimationDirection.Horizontal,
            null,
            true);
        ActiveView = null;
    }

    private void HandleParametersModified(StartStandardLevelParameters startParameters)
    {
        _modifiedParameters = true;
        _currentParameters = startParameters;
    }

    private void StartStandard()
    {
        if (_currentParameters == null)
        {
            return;
        }

        foreach (PlayViewControllerData playViewControllerData in _viewControllers)
        {
            playViewControllerData.InvokeOnPlay();
        }

        _menuTransitionsHelper.StartStandardLevel(
            _currentParameters.GameMode,
#if !PRE_V1_37_1
            _currentParameters.BeatmapKey,
            _currentParameters.BeatmapLevel,
#else
            _currentParameters.DifficultyBeatmap,
            _currentParameters.PreviewBeatmapLevel,
#endif
            _currentParameters.OverrideEnvironmentSettings,
            _currentParameters.OverrideColorScheme,
#if !V1_29_1
            _currentParameters.BeatmapOverrideColorScheme,
#endif
            _currentParameters.GameplayModifiers,
            _currentParameters.PlayerSpecificSettings,
            _currentParameters.PracticeSettings,
#if !PRE_V1_37_1
            _currentParameters.EnvironmentsListModel,
#endif
            _currentParameters.BackButtonText,
            _currentParameters.UseTestNoteCutSoundEffects,
            _currentParameters.StartPaused,
            _currentParameters.BeforeSceneSwitchCallback,
            null,
            _currentParameters.LevelFinishedCallback,
#if !V1_29_1
            _currentParameters.LevelRestartedCallback,
            _currentParameters.RecordingToolData);
#else
            _currentParameters.LevelRestartedCallback);
#endif
    }

    internal class PlayViewControllerData
    {
        private readonly MethodInfo? _onEarlyDismiss;

        private readonly MethodInfo? _onFinish;
        private readonly MethodInfo? _onPlay;

        private readonly MethodInfo? _onShow;

        internal PlayViewControllerData(IPlayViewController viewController)
        {
            ViewController = viewController;
            Type type = viewController.GetType();
            PlayViewControllerSettingsAttribute? settingsAttribute =
                type.TryGetAttribute<PlayViewControllerSettingsAttribute>();
            Priority = settingsAttribute?.Priority ?? 0;
            Title = settingsAttribute?.Title ?? string.Empty;
            _onPlay = type.GetMethod("OnPlay", AccessTools.all);
            _onFinish = type.GetMethod("OnFinish", AccessTools.all);
            _onEarlyDismiss = type.GetMethod("OnEarlyDismiss", AccessTools.all);
            _onShow = type.GetMethod("OnShow", AccessTools.all);
        }

        internal int Priority { get; }

        internal string Title { get; }

        internal IPlayViewController ViewController { get; }

        internal void InvokeOnEarlyDismiss()
        {
            _onEarlyDismiss?.Invoke(ViewController, []);
        }

        internal void InvokeOnFinish()
        {
            _onFinish?.Invoke(ViewController, []);
        }

        internal void InvokeOnPlay()
        {
            _onPlay?.Invoke(ViewController, []);
        }

        internal void InvokeOnShow()
        {
            _onShow?.Invoke(ViewController, []);
        }
    }
}
