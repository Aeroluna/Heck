using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomJSONData;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Logger = IPA.Logging.Logger;

namespace Heck.SettingsSetter
{
    internal class SettingsSetterViewController : BSMLResourceViewController
    {
        private static readonly Action<FlowCoordinator, ViewController, AnimationDirection, Action?, bool> _dismissViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, AnimationDirection, Action?, bool>>.GetDelegate("DismissViewController");
        private static readonly Action<FlowCoordinator, ViewController, Action?, AnimationDirection, bool> _presentViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, Action?, AnimationDirection, bool>>.GetDelegate("PresentViewController");

        private static string? _contentBSML;

        [UIValue("contents")]
        private readonly List<object> _contents = new();

        [UsedImplicitly]
        [UIObject("contentObject")]
        private readonly GameObject? _contentObject;

        private MainSystemInit _mainSystemInit = null!;
        private MainSettingsModelSO _mainSettings = null!;
        private ColorSchemesSettings _colorSchemesSettings = null!;

        private SinglePlayerLevelSelectionFlowCoordinator? _activeFlowCoordinator;

        private MenuTransitionsHelper? _menuTransitionsHelper;

        private StartStandardLevelParameters _defaultParameters;

        private StartStandardLevelParameters _modifiedParameters;

        private StartStandardLevelParameters _cachedParameters;

        private SettableMainSettings? _cachedMainSettings;

        private SettableMainSettings? _modifiedMainSettings;

        private List<Tuple<ISettableSetting, object>>? _settableSettingsToSet;

        public override string ResourceName => "Heck.SettingsSetter.SettingsSetter.bsml";

        internal bool DoPresent { get; private set; }

        private static string ContentBSML
        {
            get
            {
                if (_contentBSML != null)
                {
                    return _contentBSML;
                }

                using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("Heck.SettingsSetter.SettableSettingsContent.bsml")
                                                ?? throw new InvalidOperationException("Failed to retrieve SettableSettingsContent.bsml."));
                _contentBSML = reader.ReadToEnd();

                return _contentBSML;
            }
        }

        private MenuTransitionsHelper MenuTransitionHelper => _menuTransitionsHelper != null ? _menuTransitionsHelper
            : throw new InvalidOperationException($"[{nameof(_menuTransitionsHelper)}] was not created.");

        internal void ForceStartLevel()
        {
            StartWithParameters(_cachedParameters, true);
        }

        internal void RestoreCached()
        {
            if (_settableSettingsToSet != null)
            {
                foreach (Tuple<ISettableSetting, object> tuple in _settableSettingsToSet)
                {
                    ISettableSetting settableSetting = tuple.Item1;
                    Heck.Log.Logger.Log($"Restored settable setting [{settableSetting.FieldName}] in [{settableSetting.GroupName}].", Logger.Level.Trace);
                    settableSetting.SetTemporary(null);
                }

                _settableSettingsToSet = null;
            }

            if (_cachedMainSettings == null)
            {
                return;
            }

            Heck.Log.Logger.Log("Main settings restored.", Logger.Level.Trace);
            _mainSettings.mirrorGraphicsSettings.value = _cachedMainSettings.MirrorGraphicsSettings;
            _mainSettings.mainEffectGraphicsSettings.value = _cachedMainSettings.MainEffectGraphicsSettings;
            _mainSettings.smokeGraphicsSettings.value = _cachedMainSettings.SmokeGraphicsSettings;
            _mainSettings.burnMarkTrailsEnabled.value = _cachedMainSettings.BurnMarkTrailsEnabled;
            _mainSettings.screenDisplacementEffectsEnabled.value = _cachedMainSettings.ScreenDisplacementEffectsEnabled;
            _mainSettings.maxShockwaveParticles.value = _cachedMainSettings.MaxShockwaveParticles;
            _cachedMainSettings = null;
            _mainSystemInit.Init();
        }

        internal void Init(SinglePlayerLevelSelectionFlowCoordinator flowCoordinator, StartStandardLevelParameters startParameters)
        {
            // When in doubt, wrap everything in one big try catch statement!
            try
            {
                Dictionary<string, object?>? settings = startParameters.DifficultyBeatmap.GetBeatmapCustomData().Get<Dictionary<string, object?>>("_settings");
                if (settings != null)
                {
                    _contents.Clear();
                    _modifiedParameters = startParameters;

                    Dictionary<string, object?>? jsonPlayerOptions = settings.Get<Dictionary<string, object?>>("_playerOptions");
                    if (jsonPlayerOptions != null)
                    {
                        PlayerSpecificSettings playerSettings = startParameters.PlayerSpecificSettings;
                        List<Dictionary<string, object>> settablePlayerSettings = SettingSetterSettableSettingsManager.SettingsTable["_playerOptions"];

                        PlayerSpecificSettings modifiedPlayerSettings = playerSettings.CopyWith();

                        foreach (Dictionary<string, object> settablePlayerSetting in settablePlayerSettings)
                        {
                            string settingName = (string)settablePlayerSetting["_name"];
                            string fieldName = (string)settablePlayerSetting["_fieldName"];

                            object? json = jsonPlayerOptions.Get<object>(fieldName);
                            if (json == null)
                            {
                                continue;
                            }

                            FieldInfo field = typeof(PlayerSpecificSettings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                                              ?? throw new InvalidOperationException($"Unable to find field with name {fieldName}.");
                            object activeValue = field.GetValue(playerSettings);
                            json = json switch
                            {
                                string jsonString => Enum.Parse(typeof(EnvironmentEffectsFilterPreset), jsonString),
                                IConvertible => Convert.ChangeType(json, activeValue.GetType()),
                                _ => json
                            };

                            if (json.Equals(activeValue))
                            {
                                continue;
                            }

                            _contents.Add(new ListObject($"[Player Options] {settingName}", $"{activeValue} -> {json}"));
                            field.SetValue(modifiedPlayerSettings, json);
                        }

                        _modifiedParameters.PlayerSpecificSettings = modifiedPlayerSettings;
                    }

                    Dictionary<string, object?>? jsonModifiers = settings.Get<Dictionary<string, object?>>("_modifiers");
                    if (jsonModifiers != null)
                    {
                        GameplayModifiers gameplayModifiers = startParameters.GameplayModifiers;
                        List<Dictionary<string, object>> settableGameplayModifiers = SettingSetterSettableSettingsManager.SettingsTable["_modifiers"];

                        GameplayModifiers modifiedGameplayModifiers = gameplayModifiers.CopyWith();

                        foreach (Dictionary<string, object> settableGameplayModifier in settableGameplayModifiers)
                        {
                            string settingName = (string)settableGameplayModifier["_name"];
                            string fieldName = (string)settableGameplayModifier["_fieldName"];

                            object? json = jsonModifiers.Get<object>(fieldName);
                            if (json == null)
                            {
                                continue;
                            }

                            FieldInfo field = typeof(GameplayModifiers).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                                              ?? throw new InvalidOperationException($"Unable to find field with name {fieldName}.");
                            object activeValue = field.GetValue(gameplayModifiers);
                            json = json switch
                            {
                                string jsonString => fieldName switch
                                {
                                    "_energyType" => Enum.Parse(typeof(GameplayModifiers.EnergyType), jsonString),
                                    "_enabledObstacleType" => Enum.Parse(
                                        typeof(GameplayModifiers.EnabledObstacleType), jsonString),
                                    "_songSpeed" => Enum.Parse(typeof(GameplayModifiers.SongSpeed), jsonString),
                                    _ => json
                                },
                                IConvertible => Convert.ChangeType(json, activeValue.GetType()),
                                _ => json
                            };

                            if (json.Equals(activeValue))
                            {
                                continue;
                            }

                            _contents.Add(new ListObject($"[Modifiers] {settingName}", $"{activeValue} -> {json}"));
                            field.SetValue(modifiedGameplayModifiers, json);
                        }

                        _modifiedParameters.GameplayModifiers = modifiedGameplayModifiers;
                    }

                    Dictionary<string, object?>? jsonEnvironments = settings.Get<Dictionary<string, object?>>("_environments");
                    if (jsonEnvironments != null)
                    {
                        OverrideEnvironmentSettings environmentOverrideSettings = startParameters.OverrideEnvironmentSettings;

                        Dictionary<string, object> settableEnvironmentSetting = SettingSetterSettableSettingsManager.SettingsTable["_environments"].First();
                        string settingName = (string)settableEnvironmentSetting["_name"];
                        string fieldName = (string)settableEnvironmentSetting["_fieldName"];
                        bool activeValue = environmentOverrideSettings.overrideEnvironments;
                        bool? json = jsonEnvironments.Get<bool>(fieldName);

                        if (json != activeValue)
                        {
                            _contents.Add(new ListObject($"[Environments] {settingName}", $"{activeValue} -> {json}"));

                            // copy fields from original overrideenvironmentsettings to our new copy
                            OverrideEnvironmentSettings modifiedOverrideEnvironmentSettings = new();
                            modifiedOverrideEnvironmentSettings.SetField("_data", environmentOverrideSettings.GetField<Dictionary<EnvironmentTypeSO, EnvironmentInfoSO>, OverrideEnvironmentSettings>("_data"));

                            modifiedOverrideEnvironmentSettings.overrideEnvironments = json.Value;

                            _modifiedParameters.OverrideEnvironmentSettings = modifiedOverrideEnvironmentSettings;
                        }
                    }

                    Dictionary<string, object?>? jsonColors = settings.Get<Dictionary<string, object?>>("_colors");
                    if (jsonColors != null)
                    {
                        Dictionary<string, object> settableColorSetting = SettingSetterSettableSettingsManager.SettingsTable["_colors"].First();
                        string settingName = (string)settableColorSetting["_name"];
                        string fieldName = (string)settableColorSetting["_fieldName"];
                        bool activeValue = _colorSchemesSettings.overrideDefaultColors;
                        bool? json = jsonColors.Get<bool>(fieldName);

                        if (json != activeValue)
                        {
                            _contents.Add(new ListObject($"[Colors] {settingName}", $"{activeValue} -> {json}"));

                            _modifiedParameters.OverrideColorScheme = json.Value ? _colorSchemesSettings.GetOverrideColorScheme() : null;
                        }
                    }

                    _modifiedMainSettings = null;
                    _cachedMainSettings = null;
                    Dictionary<string, object?>? jsonGraphics = settings.Get<Dictionary<string, object?>>("_graphics");
                    if (jsonGraphics != null)
                    {
                        List<Dictionary<string, object>> settableGraphicsSettings = SettingSetterSettableSettingsManager.SettingsTable["_graphics"];

                        _cachedMainSettings = new SettableMainSettings(
                            _mainSettings.mirrorGraphicsSettings,
                            _mainSettings.mainEffectGraphicsSettings,
                            _mainSettings.smokeGraphicsSettings,
                            _mainSettings.burnMarkTrailsEnabled,
                            _mainSettings.screenDisplacementEffectsEnabled,
                            _mainSettings.maxShockwaveParticles);
                        _modifiedMainSettings = _cachedMainSettings with { };

                        foreach (Dictionary<string, object> settableGraphicSetting in settableGraphicsSettings)
                        {
                            string settingName = (string)settableGraphicSetting["_name"];
                            string fieldName = (string)settableGraphicSetting["_fieldName"];

                            object? json = jsonGraphics.Get<object>(fieldName);
                            if (json == null)
                            {
                                continue;
                            }

                            // substring is to remove underscore
                            object valueSO = typeof(MainSettingsModelSO).GetField(fieldName.Substring(1), BindingFlags.Instance | BindingFlags.Public)?.GetValue(_mainSettings)
                                             ?? throw new InvalidOperationException($"Unable to find valueSO with name [{fieldName.Substring(1)}].");
                            object activeValue = valueSO switch
                            {
                                BoolSO boolSO => boolSO.value,
                                IntSO intSO => intSO.value,
                                _ => throw new InvalidOperationException($"How the hell did you reach this? [{valueSO.GetType()}]")
                            };
                            if (json is IConvertible)
                            {
                                json = Convert.ChangeType(json, activeValue.GetType());
                            }

                            if (json.Equals(activeValue))
                            {
                                continue;
                            }

                            _contents.Add(new ListObject($"[Graphics] {settingName}", $"{activeValue} -> {json}"));
                            FieldInfo field = typeof(SettableMainSettings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                                              ?? throw new InvalidOperationException($"Unable to find field with name {fieldName}.");
                            field.SetValue(_modifiedMainSettings, json);
                        }
                    }

                    _settableSettingsToSet = null;
                    foreach ((string s, Dictionary<string, ISettableSetting> value) in SettingSetterSettableSettingsManager.SettableSettings)
                    {
                        Dictionary<string, object?>? jsonGroup = settings.Get<Dictionary<string, object?>>(s);
                        if (jsonGroup == null)
                        {
                            continue;
                        }

                        _settableSettingsToSet = new List<Tuple<ISettableSetting, object>>();

                        foreach ((string key, ISettableSetting settableSetting) in value)
                        {
                            object? json = jsonGroup.Get<object>(key);
                            object activeValue = settableSetting.TrueValue;
                            if (json == null || json.Equals(activeValue))
                            {
                                continue;
                            }

                            _contents.Add(new ListObject($"[{settableSetting.GroupName}] {settableSetting.FieldName}", $"{activeValue} -> {json}"));
                            _settableSettingsToSet.Add(new Tuple<ISettableSetting, object>(settableSetting, json));
                        }
                    }

                    if (_contents.Any())
                    {
                        if (_contentObject != null)
                        {
                            Destroy(_contentObject);
                        }

                        DoPresent = true;
                        _activeFlowCoordinator = flowCoordinator;
                        _defaultParameters = startParameters;
                        _presentViewController(flowCoordinator, this, null, AnimationDirection.Horizontal, false);
                        BSMLParser.instance.Parse(ContentBSML, gameObject, this);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Heck.Log.Logger.Log("Could not setup settable settings!", Logger.Level.Error);
                Heck.Log.Logger.Log(e, Logger.Level.Error);
            }

            DoPresent = false;
        }

        internal void StartWithParameters(StartStandardLevelParameters startParameters, bool force = false)
        {
            if (!force)
            {
                _cachedParameters = startParameters;

                if (_modifiedMainSettings != null)
                {
                    Heck.Log.Logger.Log("Main settings modified.", Logger.Level.Trace);
                    _mainSettings.mirrorGraphicsSettings.value = _modifiedMainSettings.MirrorGraphicsSettings;
                    _mainSettings.mainEffectGraphicsSettings.value = _modifiedMainSettings.MainEffectGraphicsSettings;
                    _mainSettings.smokeGraphicsSettings.value = _modifiedMainSettings.SmokeGraphicsSettings;
                    _mainSettings.burnMarkTrailsEnabled.value = _modifiedMainSettings.BurnMarkTrailsEnabled;
                    _mainSettings.screenDisplacementEffectsEnabled.value = _modifiedMainSettings.ScreenDisplacementEffectsEnabled;
                    _mainSettings.maxShockwaveParticles.value = _modifiedMainSettings.MaxShockwaveParticles;
                    _modifiedMainSettings = null;
                    _mainSystemInit.Init();
                }

                if (_settableSettingsToSet != null)
                {
                    foreach ((ISettableSetting settableSetting, object item2) in _settableSettingsToSet)
                    {
                        Heck.Log.Logger.Log($"Set settable setting [{settableSetting.FieldName}] in [{settableSetting.GroupName}] to [{item2}].", Logger.Level.Trace);
                        settableSetting.SetTemporary(item2);
                    }
                }
            }

            MenuTransitionHelper.StartStandardLevel(
                startParameters.GameMode,
                startParameters.DifficultyBeatmap,
                startParameters.PreviewBeatmapLevel,
                startParameters.OverrideEnvironmentSettings,
                startParameters.OverrideColorScheme,
                startParameters.GameplayModifiers,
                startParameters.PlayerSpecificSettings,
                startParameters.PracticeSettings,
                startParameters.BackButtonText,
                startParameters.UseTestNoteCutSoundEffects,
                startParameters.StartPaused,
                startParameters.BeforeSceneSwitchCallback,
                null,
                startParameters.LevelFinishedCallback);
        }

        [UsedImplicitly]
        [Inject]
        private void Construct(GameplaySetupViewController gameplaySetupViewController, MenuTransitionsHelper menuTransitionsHelper)
        {
            _colorSchemesSettings = gameplaySetupViewController.colorSchemesSettings;
            _menuTransitionsHelper = menuTransitionsHelper;
            _mainSettings = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>().First();
            _mainSystemInit = Resources.FindObjectsOfTypeAll<MainSystemInit>().First();
        }

        [UsedImplicitly]
        [UIAction("decline-click")]
        private void OnDeclineClick()
        {
            _cachedMainSettings = null;
            _modifiedMainSettings = null;
            _settableSettingsToSet = null;
            Dismiss();
            StartWithParameters(_defaultParameters);
        }

        [UsedImplicitly]
        [UIAction("accept-click")]
        private void OnAcceptClick()
        {
            Dismiss();
            StartWithParameters(_modifiedParameters);
        }

        private void Dismiss()
        {
            if (_activeFlowCoordinator == null)
            {
                throw new InvalidOperationException($"[{nameof(_activeFlowCoordinator)}] was null.");
            }

            _dismissViewController(_activeFlowCoordinator, this, AnimationDirection.Horizontal, null, true);
        }

        private readonly struct ListObject
        {
            internal ListObject(string leftText, string rightText)
            {
                LeftText = leftText;
                RightText = rightText;
            }

            [UsedImplicitly]
            [UIValue("lefttext")]
            private string LeftText { get; }

            [UsedImplicitly]
            [UIValue("righttext")]
            private string RightText { get; }
        }

        [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Fields are set through reflection")]
        private record SettableMainSettings
        {
            [UsedImplicitly]
            private int _mirrorGraphicsSettings;
            [UsedImplicitly]
            private int _mainEffectGraphicsSettings;
            [UsedImplicitly]
            private bool _smokeGraphicsSettings;
            [UsedImplicitly]
            private bool _burnMarkTrailsEnabled;
            [UsedImplicitly]
            private bool _screenDisplacementEffectsEnabled;
            [UsedImplicitly]
            private int _maxShockwaveParticles;

            internal SettableMainSettings(
                int mirrorGraphicsSettings,
                int mainEffectGraphicsSettings,
                bool smokeGraphicsSettings,
                bool burnMarkTrailsEnabled,
                bool screenDisplacementEffectsEnabled,
                int maxShockwaveParticles)
            {
                _mirrorGraphicsSettings = mirrorGraphicsSettings;
                _mainEffectGraphicsSettings = mainEffectGraphicsSettings;
                _smokeGraphicsSettings = smokeGraphicsSettings;
                _burnMarkTrailsEnabled = burnMarkTrailsEnabled;
                _screenDisplacementEffectsEnabled = screenDisplacementEffectsEnabled;
                _maxShockwaveParticles = maxShockwaveParticles;
            }

            internal int MirrorGraphicsSettings => _mirrorGraphicsSettings;

            internal int MainEffectGraphicsSettings => _mainEffectGraphicsSettings;

            internal bool SmokeGraphicsSettings => _smokeGraphicsSettings;

            internal bool BurnMarkTrailsEnabled => _burnMarkTrailsEnabled;

            internal bool ScreenDisplacementEffectsEnabled => _screenDisplacementEffectsEnabled;

            internal int MaxShockwaveParticles => _maxShockwaveParticles;
        }
    }
}
