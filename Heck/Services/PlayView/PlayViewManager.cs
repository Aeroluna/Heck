using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using ModestTree;
using Zenject;

namespace Heck.PlayView
{
    internal sealed class PlayViewManager : IDisposable
    {
        private static readonly Action<FlowCoordinator, ViewController, ViewController.AnimationDirection, Action?, bool> _dismissViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, ViewController.AnimationDirection, Action?, bool>>.GetDelegate("DismissViewController");
        private static readonly Action<FlowCoordinator, ViewController, Action?, ViewController.AnimationDirection, bool> _presentViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, Action?, ViewController.AnimationDirection, bool>>.GetDelegate("PresentViewController");
        private static readonly Action<FlowCoordinator, ViewController, Action?, ViewController.AnimationType, ViewController.AnimationDirection> _replaceTopViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, Action?, ViewController.AnimationType, ViewController.AnimationDirection>>.GetDelegate("ReplaceTopViewController");

        private readonly PlayViewControllerData[] _viewControllers;
        private readonly SoloFreePlayFlowCoordinator _soloFreePlayFlowCoordinator;
        private readonly PartyFreePlayFlowCoordinator _partyFreePlayFlowCoordinator;
        private readonly GameServerLobbyFlowCoordinator _gameServerLobbyFlowCoordinator;
        private readonly LobbyGameStateController _lobbyGameStateController;
        private readonly MenuTransitionsHelper _menuTransitionsHelper;

        private readonly bool[] _doPresent;

        private FlowCoordinator _flowCoordinator;

        private int _currentIndex;
        private StartStandardLevelParameters? _currentParameters;
        private bool _modifiedParameters;

        [UsedImplicitly]
        private PlayViewManager(
            [Inject(Optional = true, Source = InjectSources.Local)] IEnumerable<IPlayViewController> viewControllers,
            SoloFreePlayFlowCoordinator soloFreePlayFlowCoordinator,
            PartyFreePlayFlowCoordinator partyFreePlayFlowCoordinator,
            GameServerLobbyFlowCoordinator gameServerLobbyFlowCoordinator,
            ILobbyGameStateController lobbyGameStateController,
            MenuTransitionsHelper menuTransitionsHelper)
        {
            _viewControllers = viewControllers.Select(n => new PlayViewControllerData(n)).OrderBy(n => n.Priority).ToArray();
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

        internal void Init(StartStandardLevelParameters startParameters)
        {
            _currentIndex = 0;
            _currentParameters = startParameters;
            _modifiedParameters = false;
            if (startParameters is StartMultiplayerLevelParameters)
            {
                _flowCoordinator = _gameServerLobbyFlowCoordinator;
            }
            else
            {
                _flowCoordinator = _soloFreePlayFlowCoordinator.isActivated ? _soloFreePlayFlowCoordinator : _partyFreePlayFlowCoordinator;
            }

            for (int i = 0; i < _viewControllers.Length; i++)
            {
                _doPresent[i] = _viewControllers[i].ViewController.Init(_currentParameters);
            }

            Activate();
        }

        internal void FinishAll()
        {
            foreach (PlayViewControllerData playViewControllerData in _viewControllers)
            {
                playViewControllerData.InvokeOnFinish();
            }
        }

        internal bool EarlyDismiss()
        {
            if (ActiveView == null)
            {
                return true;
            }

            ActiveView.InvokeOnEarlyDismiss();
            _dismissViewController(_flowCoordinator, (ViewController)ActiveView.ViewController, ViewController.AnimationDirection.Horizontal, null, false);
            ActiveView = null;
            return false;
        }

        internal bool StartMultiplayer()
        {
            Dismiss();

            foreach (PlayViewControllerData playViewControllerData in _viewControllers)
            {
                playViewControllerData.InvokeOnPlay();
            }

            if (!_modifiedParameters || _currentParameters is not StartMultiplayerLevelParameters multiplayerStartParameters)
            {
                return true;
            }

            _lobbyGameStateController.SetProperty("countdownStarted", false);
            _lobbyGameStateController.StopListeningToGameStart();

            _menuTransitionsHelper.StartMultiplayerLevel(
                multiplayerStartParameters.GameMode,
                multiplayerStartParameters.PreviewBeatmapLevel,
                multiplayerStartParameters.BeatmapDifficulty,
                multiplayerStartParameters.BeatmapCharacteristic,
                multiplayerStartParameters.DifficultyBeatmap,
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

        private void Dismiss()
        {
            if (ActiveView == null)
            {
                return;
            }

            _dismissViewController(_flowCoordinator, (ViewController)ActiveView.ViewController, ViewController.AnimationDirection.Horizontal, null, true);
            ActiveView = null;
        }

        private void HandleParametersModified(StartStandardLevelParameters startParameters)
        {
            _modifiedParameters = true;
            _currentParameters = startParameters;
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
                    _presentViewController(
                        _flowCoordinator,
                        (ViewController)viewController,
                        null,
                        ViewController.AnimationDirection.Horizontal,
                        false);
                }
                else
                {
                    _replaceTopViewController(
                        _flowCoordinator,
                        (ViewController)viewController,
                        null,
                        ViewController.AnimationType.In,
                        ViewController.AnimationDirection.Horizontal);
                }

                viewControllerData.InvokeOnShow();

                return;
            }

            Dismiss();

            if (_currentParameters is not StartMultiplayerLevelParameters)
            {
                StartStandard();
            }
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
                _currentParameters.DifficultyBeatmap,
                _currentParameters.PreviewBeatmapLevel,
                _currentParameters.OverrideEnvironmentSettings,
                _currentParameters.OverrideColorScheme,
                _currentParameters.GameplayModifiers,
                _currentParameters.PlayerSpecificSettings,
                _currentParameters.PracticeSettings,
                _currentParameters.BackButtonText,
                _currentParameters.UseTestNoteCutSoundEffects,
                _currentParameters.StartPaused,
                _currentParameters.BeforeSceneSwitchCallback,
                null,
                _currentParameters.LevelFinishedCallback,
                _currentParameters.LevelRestartedCallback);
        }

        internal class PlayViewControllerData
        {
            private readonly MethodInfo? _onPlay;

            private readonly MethodInfo? _onFinish;

            private readonly MethodInfo? _onEarlyDismiss;

            private readonly MethodInfo? _onShow;

            internal PlayViewControllerData(IPlayViewController viewController)
            {
                ViewController = viewController;
                Type type = viewController.GetType();
                PlayViewControllerSettings? settingsAttribute = type.TryGetAttribute<PlayViewControllerSettings>();
                Priority = settingsAttribute?.Priority ?? 0;
                Title = settingsAttribute?.Title ?? string.Empty;
                _onPlay = type.GetMethod("OnPlay", AccessTools.all);
                _onFinish = type.GetMethod("OnFinish", AccessTools.all);
                _onEarlyDismiss = type.GetMethod("OnEarlyDismiss", AccessTools.all);
                _onShow = type.GetMethod("OnShow", AccessTools.all);
            }

            internal IPlayViewController ViewController { get; }

            internal int Priority { get; }

            internal string Title { get; }

            internal void InvokeOnPlay()
            {
                _onPlay?.Invoke(ViewController, Array.Empty<object>());
            }

            internal void InvokeOnFinish()
            {
                _onFinish?.Invoke(ViewController, Array.Empty<object>());
            }

            internal void InvokeOnEarlyDismiss()
            {
                _onEarlyDismiss?.Invoke(ViewController, Array.Empty<object>());
            }

            internal void InvokeOnShow()
            {
                _onShow?.Invoke(ViewController, Array.Empty<object>());
            }
        }
    }
}
