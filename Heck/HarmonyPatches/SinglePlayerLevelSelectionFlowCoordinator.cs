using System;
using HarmonyLib;
using Heck.SettingsSetter;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;

namespace Heck.HarmonyPatches
{
    [HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator))]
    [HarmonyPatch("StartLevel")]
    internal static class SinglePlayerLevelSelectionFlowCoordinatorStartLevel
    {
        [UsedImplicitly]
        private static void Prefix(SinglePlayerLevelSelectionFlowCoordinator __instance, GameplaySetupViewController ____gameplaySetupViewController)
        {
            SettingsSetterViewController.CurrentOverrideColorScheme = ____gameplaySetupViewController.colorSchemesSettings;
            SettingsSetterViewController.ActiveFlowCoordinator = __instance;
        }
    }

    [HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator))]
    [HarmonyPatch("HandleBasicLevelCompletionResults")]
    internal static class SinglePlayerLevelSelectionFlowCoordinatorHandleBasicLevelCompletionResults
    {
        [UsedImplicitly]
        private static bool Prefix(LevelCompletionResults levelCompletionResults)
        {
            if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.Restart ||
                !SettingsSetterViewController.Instance.DoPresent)
            {
                return true;
            }

            SettingsSetterViewController.Instance.ForceStart();
            return false;
        }
    }

    [HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator))]
    [HarmonyPatch("LevelSelectionFlowCoordinatorTopViewControllerWillChange")]
    internal static class SinglePlayerLevelSelectionFlowCoordinatorLevelSelectionFlowCoordinatorTopViewControllerWillChange
    {
        private static readonly Action<FlowCoordinator, ViewController?, ViewController.AnimationType> _setLeftScreenViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController?, ViewController.AnimationType>>.GetDelegate("SetLeftScreenViewController");
        private static readonly Action<FlowCoordinator, ViewController?, ViewController.AnimationType> _setRightScreenViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController?, ViewController.AnimationType>>.GetDelegate("SetRightScreenViewController");
        private static readonly Action<FlowCoordinator, ViewController?, ViewController.AnimationType> _setBottomScreenViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController?, ViewController.AnimationType>>.GetDelegate("SetBottomScreenViewController");
        private static readonly Action<FlowCoordinator, string?, ViewController.AnimationType> _setTitle = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, string?, ViewController.AnimationType>>.GetDelegate("SetTitle");
        private static readonly PropertyAccessor<FlowCoordinator, bool>.Setter _showBackButtonSetter = PropertyAccessor<FlowCoordinator, bool>.GetSetter("showBackButton");

        [UsedImplicitly]
        private static bool Prefix(SinglePlayerLevelSelectionFlowCoordinator __instance, ViewController newViewController, ViewController.AnimationType animationType)
        {
            if (newViewController != SettingsSetterViewController.Instance)
            {
                return true;
            }

            _setLeftScreenViewController(__instance, null, animationType);
            _setRightScreenViewController(__instance, null, animationType);
            _setBottomScreenViewController(__instance, null, animationType);
            _setTitle(__instance, "information", animationType);
            FlowCoordinator flowCoordinator = __instance;
            _showBackButtonSetter(ref flowCoordinator, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator))]
    [HarmonyPatch("BackButtonWasPressed")]
    internal static class SinglePlayerLevelSelectionFlowCoordinatorBackButtonWasPressed
    {
        private static readonly Action<FlowCoordinator, ViewController, ViewController.AnimationDirection, Action?, bool> _dismissViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, ViewController.AnimationDirection, Action?, bool>>.GetDelegate("DismissViewController");

        [UsedImplicitly]
        private static bool Prefix(SinglePlayerLevelSelectionFlowCoordinator __instance)
        {
            if (__instance.topViewController != SettingsSetterViewController.Instance)
            {
                return true;
            }

            _dismissViewController(__instance, SettingsSetterViewController.Instance, ViewController.AnimationDirection.Horizontal, null, false);
            return false;
        }
    }
}
