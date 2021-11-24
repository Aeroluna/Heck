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
using CustomJSONData.CustomBeatmap;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Logger = IPA.Logging.Logger;

namespace Heck.SettingsSetter
{
    internal class SettingsSetterViewController : BSMLResourceViewController
    {
        private static readonly Action<FlowCoordinator, ViewController, AnimationDirection, Action?, bool> _dismissViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, AnimationDirection, Action?, bool>>.GetDelegate("DismissViewController");
        private static readonly Action<FlowCoordinator, ViewController, Action?, AnimationDirection, bool> _presentViewController = MethodAccessor<FlowCoordinator, Action<FlowCoordinator, ViewController, Action?, AnimationDirection, bool>>.GetDelegate("PresentViewController");

        private static SettingsSetterViewController? _instance;

        private static MainSystemInit? _mainSystemInit;

        private static MainSettingsModelSO? _mainSettings;

        private static SinglePlayerLevelSelectionFlowCoordinator? _activeFlowCoordinator;

        private static ColorSchemesSettings? _overrideColorScheme;

        private static string? _contentBSML;

        [UIValue("contents")]
        private readonly List<object> _contents = new();

        [UsedImplicitly]
        [UIObject("contentObject")]
        private readonly GameObject? _contentObject;

        private MenuTransitionsHelper? _menuTransitionsHelper;

        private StartStandardLevelParameters _defaultParameters;

        private StartStandardLevelParameters _modifiedParameters;

        private StartStandardLevelParameters _cachedParameters;

        private SettableMainSettings? _cachedMainSettings;

        private SettableMainSettings? _modifiedMainSettings;

        private List<Tuple<ISettableSetting, object>>? _settableSettingsToSet;

        public override string ResourceName => "Heck.SettingsSetter.SettingsSetter.bsml";

        internal static SettingsSetterViewController Instance => _instance != null ? _instance
            : throw new InvalidOperationException($"[{nameof(_instance)}] was not created.");

        internal static SinglePlayerLevelSelectionFlowCoordinator ActiveFlowCoordinator
        {
            get => _activeFlowCoordinator != null ? _activeFlowCoordinator : throw new InvalidOperationException($"[{nameof(_activeFlowCoordinator)}] was null.");
            set => _activeFlowCoordinator = value;
        }

        // Color scheme settings isnt passed through like the rest of the kids
        internal static ColorSchemesSettings CurrentOverrideColorScheme
        {
            get => _overrideColorScheme ?? throw new InvalidOperationException($"[{nameof(_overrideColorScheme)}] was null.");
            set => _overrideColorScheme = value;
        }

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

        private static MainSettingsModelSO MainSettings => _mainSettings != null ? _mainSettings
            : throw new InvalidOperationException($"[{nameof(_mainSettings)}] was null.");

        private MenuTransitionsHelper MenuTransitionHelper => _menuTransitionsHelper != null ? _menuTransitionsHelper
            : throw new InvalidOperationException($"[{nameof(_menuTransitionsHelper)}] was not created.");

        internal static void Instantiate()
        {
            _instance = BeatSaberUI.CreateViewController<SettingsSetterViewController>();
            _mainSettings = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>().First();
            _mainSystemInit = Resources.FindObjectsOfTypeAll<MainSystemInit>().First();
        }

        internal void ForceStart()
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
            MainSettings.mirrorGraphicsSettings.value = _cachedMainSettings.MirrorGraphicsSettings;
            MainSettings.mainEffectGraphicsSettings.value = _cachedMainSettings.MainEffectGraphicsSettings;
            MainSettings.smokeGraphicsSettings.value = _cachedMainSettings.SmokeGraphicsSettings;
            MainSettings.burnMarkTrailsEnabled.value = _cachedMainSettings.BurnMarkTrailsEnabled;
            MainSettings.screenDisplacementEffectsEnabled.value = _cachedMainSettings.ScreenDisplacementEffectsEnabled;
            MainSettings.maxShockwaveParticles.value = _cachedMainSettings.MaxShockwaveParticles;
            _cachedMainSettings = null;
            _mainSystemInit!.Init();
        }

        internal void Init(StartStandardLevelParameters startParameters, MenuTransitionsHelper menuTransitionsHelper)
        {
            // When in doubt, wrap everything in one big try catch statement!
            try
            {
                if (startParameters.DifficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
                {
                    Dictionary<string, object?>? settings = customBeatmapData.beatmapCustomData.Get<Dictionary<string, object?>>("_settings");
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

                                _contents.Add(new ListObject($"[Player Options] {settingName}", $"{activeValue} > {json}"));
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

                                _contents.Add(new ListObject($"[Modifiers] {settingName}", $"{activeValue} > {json}"));
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
                                _contents.Add(new ListObject($"[Environments] {settingName}", $"{activeValue} => {json}"));

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
                            ColorSchemesSettings colorSchemesSettings = CurrentOverrideColorScheme;
                            Dictionary<string, object> settableColorSetting = SettingSetterSettableSettingsManager.SettingsTable["_colors"].First();
                            string settingName = (string)settableColorSetting["_name"];
                            string fieldName = (string)settableColorSetting["_fieldName"];
                            bool activeValue = colorSchemesSettings.overrideDefaultColors;
                            bool? json = jsonColors.Get<bool>(fieldName);

                            if (json != activeValue)
                            {
                                _contents.Add(new ListObject($"[Colors] {settingName}", $"{activeValue} > {json}"));

                                _modifiedParameters.OverrideColorScheme = json.Value ? colorSchemesSettings.GetOverrideColorScheme() : null;
                            }
                        }

                        _modifiedMainSettings = null;
                        _cachedMainSettings = null;
                        Dictionary<string, object?>? jsonGraphics = settings.Get<Dictionary<string, object?>>("_graphics");
                        if (jsonGraphics != null)
                        {
                            MainSettingsModelSO mainSettingsModel = MainSettings;
                            List<Dictionary<string, object>> settableGraphicsSettings = SettingSetterSettableSettingsManager.SettingsTable["_graphics"];

                            _cachedMainSettings = new SettableMainSettings(
                                mainSettingsModel.mirrorGraphicsSettings,
                                mainSettingsModel.mainEffectGraphicsSettings,
                                mainSettingsModel.smokeGraphicsSettings,
                                mainSettingsModel.burnMarkTrailsEnabled,
                                mainSettingsModel.screenDisplacementEffectsEnabled,
                                mainSettingsModel.maxShockwaveParticles);
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
                                object valueSO = typeof(MainSettingsModelSO).GetField(fieldName.Substring(1), BindingFlags.Instance | BindingFlags.Public)?.GetValue(mainSettingsModel)
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

                                _contents.Add(new ListObject($"[Graphics] {settingName}", $"{activeValue} > {json}"));
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

                                _contents.Add(new ListObject($"[{settableSetting.GroupName}] {settableSetting.FieldName}", $"{activeValue} > {json}"));
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
                            _defaultParameters = startParameters;
                            _menuTransitionsHelper = menuTransitionsHelper;
                            _presentViewController(ActiveFlowCoordinator, this, null, AnimationDirection.Horizontal, false);
                            BSMLParser.instance.Parse(ContentBSML, gameObject, this);
                            return;
                        }
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

        [UsedImplicitly]
        [UIAction("decline-click")]
        private void OnDeclineClick()
        {
            _cachedMainSettings = null;
            _modifiedMainSettings = null;
            _settableSettingsToSet = null;
            StartWithParameters(_defaultParameters);
            Dismiss();
        }

        [UsedImplicitly]
        [UIAction("accept-click")]
        private void OnAcceptClick()
        {
            StartWithParameters(_modifiedParameters);
            Dismiss();
        }

        private void Dismiss()
        {
            _dismissViewController(ActiveFlowCoordinator, this, AnimationDirection.Horizontal, null, true);
        }

        private void StartWithParameters(StartStandardLevelParameters startParameters, bool force = false)
        {
            if (!force)
            {
                _cachedParameters = startParameters;

                if (_modifiedMainSettings != null)
                {
                    Heck.Log.Logger.Log("Main settings modified.", Logger.Level.Trace);
                    MainSettings.mirrorGraphicsSettings.value = _modifiedMainSettings.MirrorGraphicsSettings;
                    MainSettings.mainEffectGraphicsSettings.value = _modifiedMainSettings.MainEffectGraphicsSettings;
                    MainSettings.smokeGraphicsSettings.value = _modifiedMainSettings.SmokeGraphicsSettings;
                    MainSettings.burnMarkTrailsEnabled.value = _modifiedMainSettings.BurnMarkTrailsEnabled;
                    MainSettings.screenDisplacementEffectsEnabled.value = _modifiedMainSettings.ScreenDisplacementEffectsEnabled;
                    MainSettings.maxShockwaveParticles.value = _modifiedMainSettings.MaxShockwaveParticles;
                    _modifiedMainSettings = null;
                    _mainSystemInit!.Init();
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
                startParameters.BeforeSceneSwitchCallback,
                null,
                startParameters.LevelFinishedCallback);
        }

        internal struct StartStandardLevelParameters
        {
            internal StartStandardLevelParameters(
                string gameMode,
                IDifficultyBeatmap difficultyBeatmap,
                IPreviewBeatmapLevel previewBeatmapLevel,
                OverrideEnvironmentSettings overrideEnvironmentSettings,
                ColorScheme overrideColorScheme,
                GameplayModifiers gameplayModifiers,
                PlayerSpecificSettings playerSpecificSettings,
                PracticeSettings practiceSettings,
                string backButtonText,
                bool useTestNoteCutSoundEffects,
                Action beforeSceneSwitchCallback,
                Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> levelFinishedCallback)
            {
                GameMode = gameMode;
                DifficultyBeatmap = difficultyBeatmap;
                PreviewBeatmapLevel = previewBeatmapLevel;
                OverrideEnvironmentSettings = overrideEnvironmentSettings;
                OverrideColorScheme = overrideColorScheme;
                GameplayModifiers = gameplayModifiers;
                PlayerSpecificSettings = playerSpecificSettings;
                PracticeSettings = practiceSettings;
                BackButtonText = backButtonText;
                UseTestNoteCutSoundEffects = useTestNoteCutSoundEffects;
                BeforeSceneSwitchCallback = beforeSceneSwitchCallback;
                LevelFinishedCallback = levelFinishedCallback;
            }

            internal string GameMode { get; }

            internal IDifficultyBeatmap DifficultyBeatmap { get; }

            internal IPreviewBeatmapLevel PreviewBeatmapLevel { get; }

            internal OverrideEnvironmentSettings OverrideEnvironmentSettings { get; set; }

            internal ColorScheme? OverrideColorScheme { get; set; }

            internal GameplayModifiers GameplayModifiers { get; set; }

            internal PlayerSpecificSettings PlayerSpecificSettings { get; set; }

            internal PracticeSettings PracticeSettings { get; }

            internal string BackButtonText { get; }

            internal bool UseTestNoteCutSoundEffects { get; }

            internal Action? BeforeSceneSwitchCallback { get; }

            internal Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelFinishedCallback { get; }
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
