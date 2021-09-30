namespace Heck.SettingsSetter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using BeatSaberMarkupLanguage.Attributes;
    using BeatSaberMarkupLanguage.ViewControllers;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HMUI;
    using IPA.Utilities;
    using UnityEngine;

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
        private readonly List<object> _contents = new List<object>();

        [UIObject("contentObject")]
#pragma warning disable CS0649
        private readonly GameObject? _contentObject;
#pragma warning restore CS0649

        private MenuTransitionsHelper? _menuTransitionsHelper;

        private StartStandardLevelParameters _defaultParameters;

        private StartStandardLevelParameters _modifiedParameters;

        private StartStandardLevelParameters _cachedParameters;

        private SettableMainSettings? _cachedMainSettings;

        private SettableMainSettings? _modifiedMainSettings;

        private List<Tuple<ISettableSetting, object>>? _settableSettingsToSet;

        public override string ResourceName => "Heck.SettingsSetter.SettingsSetter.bsml";

        internal static SettingsSetterViewController Instance => _instance ?? throw new InvalidOperationException($"[{nameof(_instance)}] was not created.");

        internal static SinglePlayerLevelSelectionFlowCoordinator ActiveFlowCoordinator
        {
            get => _activeFlowCoordinator ?? throw new InvalidOperationException($"[{nameof(_activeFlowCoordinator)}] was null.");
            set => _activeFlowCoordinator = value;
        }

        // Color scheme settings isnt passed through like the rest of the kids
        internal static ColorSchemesSettings OverrideColorScheme
        {
            get => _overrideColorScheme ?? throw new InvalidOperationException($"[{nameof(_overrideColorScheme)}] was null.");
            set => _overrideColorScheme = value;
        }

        internal bool DoPresent { get; private set; }

        private static string ContentBSML
        {
            get
            {
                if (_contentBSML == null)
                {
                    using StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Heck.SettingsSetter.SettableSettingsContent.bsml"));
                    _contentBSML = reader.ReadToEnd();
                }

                return _contentBSML;
            }
        }

        private static MainSettingsModelSO MainSettings => _mainSettings ?? throw new InvalidOperationException($"[{nameof(_mainSettings)}] was null.");

        private MenuTransitionsHelper MenuTransitionHelper => _menuTransitionsHelper ?? throw new InvalidOperationException($"[{nameof(_menuTransitionsHelper)}] was not created.");

        internal static void Instantiate()
        {
            _instance = BeatSaberMarkupLanguage.BeatSaberUI.CreateViewController<SettingsSetterViewController>();
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
                    Plugin.Logger.Log($"Restored settable setting [{settableSetting.FieldName}] in [{settableSetting.GroupName}].", IPA.Logging.Logger.Level.Trace);
                    settableSetting.SetTemporary(null);
                }

                _settableSettingsToSet = null;
            }

            if (_cachedMainSettings != null)
            {
                Plugin.Logger.Log($"Main settings restored.", IPA.Logging.Logger.Level.Trace);
                MainSettings.mirrorGraphicsSettings.value = _cachedMainSettings.MirrorGraphicsSettings;
                MainSettings.mainEffectGraphicsSettings.value = _cachedMainSettings.MainEffectGraphicsSettings;
                MainSettings.smokeGraphicsSettings.value = _cachedMainSettings.SmokeGraphicsSettings;
                MainSettings.burnMarkTrailsEnabled.value = _cachedMainSettings.BurnMarkTrailsEnabled;
                MainSettings.screenDisplacementEffectsEnabled.value = _cachedMainSettings.ScreenDisplacementEffectsEnabled;
                MainSettings.maxShockwaveParticles.value = _cachedMainSettings.MaxShockwaveParticles;
                _cachedMainSettings = null;
                _mainSystemInit!.Init();
            }
        }

        internal void Init(StartStandardLevelParameters startParameters, MenuTransitionsHelper menuTransitionsHelper)
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
                            string name = (string)settablePlayerSetting["_name"];
                            string fieldName = (string)settablePlayerSetting["_fieldName"];

                            object? json = jsonPlayerOptions.Get<object>(fieldName);
                            if (json != null)
                            {
                                FieldInfo field = typeof(PlayerSpecificSettings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                                object activeValue = field.GetValue(playerSettings);
                                if (json is string jsonString)
                                {
                                    json = Enum.Parse(typeof(EnvironmentEffectsFilterPreset), jsonString);
                                }
                                else if (json is IConvertible)
                                {
                                    json = Convert.ChangeType(json, activeValue.GetType());
                                }

                                if (!json.Equals(activeValue))
                                {
                                    _contents.Add(new ListObject($"[Player Options] {name}", $"{activeValue} > {json}"));
                                    field.SetValue(modifiedPlayerSettings, json);
                                }
                            }
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
                            string name = (string)settableGameplayModifier["_name"];
                            string fieldName = (string)settableGameplayModifier["_fieldName"];

                            object? json = jsonModifiers.Get<object>(fieldName);
                            if (json != null)
                            {
                                FieldInfo field = typeof(GameplayModifiers).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                                object activeValue = field.GetValue(gameplayModifiers);
                                if (json is string jsonString)
                                {
                                    switch (fieldName)
                                    {
                                        case "_energyType":
                                            json = Enum.Parse(typeof(GameplayModifiers.EnergyType), jsonString);
                                            break;

                                        case "_enabledObstacleType":
                                            json = Enum.Parse(typeof(GameplayModifiers.EnabledObstacleType), jsonString);
                                            break;

                                        case "_songSpeed":
                                            json = Enum.Parse(typeof(GameplayModifiers.SongSpeed), jsonString);
                                            break;
                                    }
                                }
                                else if (json is IConvertible)
                                {
                                    json = Convert.ChangeType(json, activeValue.GetType());
                                }

                                if (!json.Equals(activeValue))
                                {
                                    _contents.Add(new ListObject($"[Modifiers] {name}", $"{activeValue} > {json}"));
                                    field.SetValue(modifiedGameplayModifiers, json);
                                }
                            }
                        }

                        _modifiedParameters.GameplayModifiers = modifiedGameplayModifiers;
                    }

                    Dictionary<string, object?>? jsonEnvironments = settings.Get<Dictionary<string, object?>>("_environments");
                    if (jsonEnvironments != null)
                    {
                        OverrideEnvironmentSettings? environmentOverrideSettings = startParameters.OverrideEnvironmentSettings;

                        if (environmentOverrideSettings != null)
                        {
                            Dictionary<string, object> settableEnvironmentSetting = SettingSetterSettableSettingsManager.SettingsTable["_environments"].First();
                            string name = (string)settableEnvironmentSetting["_name"];
                            string fieldName = (string)settableEnvironmentSetting["_fieldName"];
                            bool activeValue = environmentOverrideSettings.overrideEnvironments;
                            bool? json = jsonEnvironments.Get<bool>(fieldName);

                            if (json != null && json != activeValue)
                            {
                                _contents.Add(new ListObject($"[Environments] {name}", $"{activeValue} > {json}"));

                                // copy fields from original overrideenvironmentsettings to our new copy
                                OverrideEnvironmentSettings modifiedOverrideEnvironmentSettings = new OverrideEnvironmentSettings();
                                modifiedOverrideEnvironmentSettings.SetField("_data", environmentOverrideSettings.GetField<Dictionary<EnvironmentTypeSO, EnvironmentInfoSO>, OverrideEnvironmentSettings>("_data"));

                                modifiedOverrideEnvironmentSettings.overrideEnvironments = json.Value;

                                _modifiedParameters.OverrideEnvironmentSettings = modifiedOverrideEnvironmentSettings;
                            }
                        }
                    }

                    Dictionary<string, object?>? jsonColors = settings.Get<Dictionary<string, object?>>("_colors");
                    if (jsonColors != null)
                    {
                        ColorSchemesSettings? colorSchemesSettings = OverrideColorScheme;

                        if (colorSchemesSettings != null)
                        {
                            Dictionary<string, object> settableColorSetting = SettingSetterSettableSettingsManager.SettingsTable["_colors"].First();
                            string name = (string)settableColorSetting["_name"];
                            string fieldName = (string)settableColorSetting["_fieldName"];
                            bool activeValue = colorSchemesSettings.overrideDefaultColors;
                            bool? json = jsonColors.Get<bool>(fieldName);

                            if (json != null && json != activeValue)
                            {
                                _contents.Add(new ListObject($"[Colors] {name}", $"{activeValue} > {json}"));

                                _modifiedParameters.OverrideColorScheme = json.Value ? colorSchemesSettings.GetOverrideColorScheme() : null;
                            }
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
                            string name = (string)settableGraphicSetting["_name"];
                            string fieldName = (string)settableGraphicSetting["_fieldName"];

                            object? json = jsonGraphics.Get<object>(fieldName);
                            if (json != null)
                            {
                                // substring is to remove underscore
                                object valueSO = typeof(MainSettingsModelSO).GetField(fieldName.Substring(1), BindingFlags.Instance | BindingFlags.Public).GetValue(mainSettingsModel);
                                object activeValue = valueSO switch
                                {
                                    BoolSO boolSO => boolSO.value,
                                    IntSO intSO => intSO.value,
                                    _ => throw new InvalidOperationException($"How the hell did you reach this? [{valueSO.GetType()}]"),
                                };
                                if (json is IConvertible)
                                {
                                    json = Convert.ChangeType(json, activeValue.GetType());
                                }

                                if (!json.Equals(activeValue))
                                {
                                    _contents.Add(new ListObject($"[Graphics] {name}", $"{activeValue} > {json}"));
                                    typeof(SettableMainSettings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_modifiedMainSettings, json);
                                }
                            }
                        }
                    }

                    _settableSettingsToSet = null;
                    foreach (KeyValuePair<string, Dictionary<string, ISettableSetting>> groupSettingPair in SettingSetterSettableSettingsManager.SettableSettings)
                    {
                        Dictionary<string, object?>? jsonGroup = settings.Get<Dictionary<string, object?>>(groupSettingPair.Key);
                        if (jsonGroup != null)
                        {
                            _settableSettingsToSet = new List<Tuple<ISettableSetting, object>>();

                            foreach (KeyValuePair<string, ISettableSetting> settableSettingPair in groupSettingPair.Value)
                            {
                                object? json = jsonGroup.Get<object>(settableSettingPair.Key);
                                ISettableSetting settableSetting = settableSettingPair.Value;
                                object activeValue = settableSetting.TrueValue;
                                if (json != null && !json.Equals(activeValue))
                                {
                                    _contents.Add(new ListObject($"[{settableSetting.GroupName}] {settableSetting.FieldName}", $"{activeValue} > {json}"));
                                    _settableSettingsToSet.Add(new Tuple<ISettableSetting, object>(settableSetting, json));
                                }
                            }
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
                        BeatSaberMarkupLanguage.BSMLParser.instance.Parse(ContentBSML, gameObject, this);
                        return;
                    }
                }
            }

            DoPresent = false;
        }

        [UIAction("decline-click")]
        private void OnDeclineClick()
        {
            _cachedMainSettings = null;
            _modifiedMainSettings = null;
            _settableSettingsToSet = null;
            StartWithParameters(_defaultParameters);
            Dismiss();
        }

        [UIAction("accept-click")]
        private void OnAcceptClick()
        {
            StartWithParameters(_modifiedParameters);
            Dismiss();
        }

        private void Dismiss()
        {
            _dismissViewController(ActiveFlowCoordinator, this, AnimationDirection.Horizontal, null, false);
        }

        private void StartWithParameters(StartStandardLevelParameters startParameters, bool force = false)
        {
            if (!force)
            {
                _cachedParameters = startParameters;

                if (_modifiedMainSettings != null)
                {
                    Plugin.Logger.Log($"Main settings modified.", IPA.Logging.Logger.Level.Trace);
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
                    foreach (Tuple<ISettableSetting, object> tuple in _settableSettingsToSet)
                    {
                        ISettableSetting settableSetting = tuple.Item1;
                        Plugin.Logger.Log($"Set settable setting [{settableSetting.FieldName}] in [{settableSetting.GroupName}] to [{tuple.Item2}].", IPA.Logging.Logger.Level.Trace);
                        settableSetting.SetTemporary(tuple.Item2);
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

            internal PracticeSettings PracticeSettings { get; set; }

            internal string BackButtonText { get; }

            internal bool UseTestNoteCutSoundEffects { get; }

            internal Action? BeforeSceneSwitchCallback { get; }

            internal Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelFinishedCallback { get; }
        }

        private struct ListObject
        {
            internal ListObject(string leftText, string rightText)
            {
                LeftText = leftText;
                RightText = rightText;
            }

            [UIValue("lefttext")]
            private string LeftText { get; }

            [UIValue("righttext")]
            private string RightText { get; }
        }

        private record SettableMainSettings
        {
#pragma warning disable IDE0044 // reflection!!
            private int _mirrorGraphicsSettings;
            private int _mainEffectGraphicsSettings;
            private bool _smokeGraphicsSettings;
            private bool _burnMarkTrailsEnabled;
            private bool _screenDisplacementEffectsEnabled;
            private int _maxShockwaveParticles;
#pragma warning restore IDE0044

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
