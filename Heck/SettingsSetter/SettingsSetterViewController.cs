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
using Heck.PlayView;
using IPA.Utilities;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
#if V1_37_1
using BeatSaber.GameSettings;
using BeatSaber.PerformancePresets;
using Heck.HarmonyPatches;
#endif

namespace Heck.SettingsSetter;

[PlayViewControllerSettings(0, "settings")]
internal class SettingsSetterViewController : BSMLResourceViewController, IPlayViewController, IParameterModifier
{
    private static string? _contentBSML;

    [UIValue("contents")]
    private readonly List<object> _contents = [];

    [UsedImplicitly]
    [UIObject("contentObject")]
    private readonly GameObject? _contentObject;

    [UIObject("decline-button")]
    private readonly GameObject? _declineButton;

    [UIComponent("top-vertical")]
    private readonly VerticalLayoutGroup _topVertical = null!;

    private SiraLog _log = null!;
    private BSMLParser _bsmlParser = null!;
#if !PRE_V1_39_1
    private SettingsManager _settingsManager = null!;
#elif V1_37_1
    private GraphicSettingsHandler _graphicSettingsHandler = null!;
    private PerformancePresetOverride _performancePresetOverride = null!;
#else
    private MainSystemInit _mainSystemInit = null!;
    private MainSettingsModelSO _mainSettings = null!;
#endif
    private ColorSchemesSettings _colorSchemesSettings = null!;
    private PlayerDataModel _playerDataModel = null!;

    private StartStandardLevelParameters? _defaultParameters;
    private StartStandardLevelParameters? _modifiedParameters;

#if !V1_37_1
    private SettableMainSettings? _cachedMainSettings;
#endif
    private SettableMainSettings? _modifiedMainSettings;

    private bool _isMultiplayer;
    private bool _declined;

    private OverrideEnvironmentSettings? _cachedOverrideEnvironmentSettings;

    private List<(ISettableSetting, object)>? _settableSettingsToSet;

    public event Action? Finished;

    public event Action<StartStandardLevelParameters>? ParametersModified;

    public override string ResourceName => "Heck.SettingsSetter.SettingsSetter.bsml";

    private static string ContentBSML
    {
        get
        {
            if (_contentBSML != null)
            {
                return _contentBSML;
            }

            using StreamReader reader = new(
                Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceStream("Heck.SettingsSetter.SettableSettingsContent.bsml") ??
                throw new InvalidOperationException("Failed to retrieve SettableSettingsContent.bsml."));
            _contentBSML = reader.ReadToEnd();

            return _contentBSML;
        }
    }

    public bool Init(StartStandardLevelParameters startParameters)
    {
        _declined = true;

        // When in doubt, wrap everything in one big try catch statement!
        try
        {
#if !PRE_V1_37_1
            CustomData beatmapCustomData =
                startParameters.BeatmapLevel.GetBeatmapCustomData(startParameters.BeatmapKey);
#else
            CustomData beatmapCustomData = startParameters.DifficultyBeatmap.GetBeatmapCustomData();
#endif
            CustomData? settings = beatmapCustomData.Get<CustomData>("_settings");
            if (settings != null)
            {
                _contents.Clear();
                _modifiedParameters = startParameters.Copy();
                _isMultiplayer = _modifiedParameters is StartMultiplayerLevelParameters;

                CustomData? jsonPlayerOptions = settings.Get<CustomData>("_playerOptions");
                if (jsonPlayerOptions != null)
                {
                    PlayerSpecificSettings playerSettings = startParameters.PlayerSpecificSettings;
                    List<Dictionary<string, object>> settablePlayerSettings =
                        SettingSetterSettableSettingsManager.SettingsTable["_playerOptions"];

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

                        FieldInfo field =
                            typeof(PlayerSpecificSettings).GetField(
                                fieldName,
                                BindingFlags.Instance | BindingFlags.NonPublic) ??
                            throw new InvalidOperationException($"Unable to find field with name {fieldName}.");
                        object activeValue = field.GetValue(playerSettings);
                        json = json switch
                        {
                            string jsonString => fieldName switch
                            {
                                "_noteJumpDurationTypeSettings" => Enum.Parse(typeof(NoteJumpDurationTypeSettings), jsonString),
                                _ => Enum.Parse(typeof(EnvironmentEffectsFilterPreset), jsonString)
                            },
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

                CustomData? jsonModifiers = settings.Get<CustomData>("_modifiers");
                if (jsonModifiers != null)
                {
                    GameplayModifiers gameplayModifiers = startParameters.GameplayModifiers;
                    List<Dictionary<string, object>> settableGameplayModifiers =
                        SettingSetterSettableSettingsManager.SettingsTable["_modifiers"];

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

                        FieldInfo field =
                            typeof(GameplayModifiers).GetField(
                                fieldName,
                                BindingFlags.Instance | BindingFlags.NonPublic) ??
                            throw new InvalidOperationException($"Unable to find field with name {fieldName}.");
                        object activeValue = field.GetValue(gameplayModifiers);
                        json = json switch
                        {
                            string jsonString => fieldName switch
                            {
                                "_energyType" => Enum.Parse(typeof(GameplayModifiers.EnergyType), jsonString),
                                "_enabledObstacleType" => Enum.Parse(
                                    typeof(GameplayModifiers.EnabledObstacleType),
                                    jsonString),
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

                CustomData? jsonEnvironments = settings.Get<CustomData>("_environments");
                if (jsonEnvironments != null)
                {
                    OverrideEnvironmentSettings? environmentOverrideSettings;
                    if (_isMultiplayer)
                    {
                        _cachedOverrideEnvironmentSettings = _playerDataModel.playerData.overrideEnvironmentSettings;
                        environmentOverrideSettings = _cachedOverrideEnvironmentSettings;
                    }
                    else
                    {
                        environmentOverrideSettings = startParameters.OverrideEnvironmentSettings;
                    }

                    if (environmentOverrideSettings != null)
                    {
                        Dictionary<string, object> settableEnvironmentSetting =
                            SettingSetterSettableSettingsManager.SettingsTable["_environments"].First();
                        string settingName = (string)settableEnvironmentSetting["_name"];
                        string fieldName = (string)settableEnvironmentSetting["_fieldName"];
                        bool activeValue = environmentOverrideSettings.overrideEnvironments;
                        bool? json = jsonEnvironments.Get<bool>(fieldName);

                        if (json != activeValue)
                        {
                            _contents.Add(new ListObject($"[Environments] {settingName}", $"{activeValue} -> {json}"));

                            // copy fields from original overrideenvironmentsettings to our new copy
                            OverrideEnvironmentSettings modifiedOverrideEnvironmentSettings = new();
                            modifiedOverrideEnvironmentSettings.SetField("_data", environmentOverrideSettings._data);

                            modifiedOverrideEnvironmentSettings.overrideEnvironments = json.Value;

                            if (_isMultiplayer)
                            {
                                // must be set directly for multiplayer
                                _playerDataModel.playerData.SetProperty(
                                    "overrideEnvironmentSettings",
                                    modifiedOverrideEnvironmentSettings);
                            }
                            else
                            {
                                _modifiedParameters.OverrideEnvironmentSettings = modifiedOverrideEnvironmentSettings;
                            }
                        }
                    }
                }

                CustomData? jsonColors = settings.Get<CustomData>("_colors");
                if (jsonColors != null)
                {
                    Dictionary<string, object> settableColorSetting =
                        SettingSetterSettableSettingsManager.SettingsTable["_colors"].First();
                    string settingName = (string)settableColorSetting["_name"];
                    string fieldName = (string)settableColorSetting["_fieldName"];
                    bool activeValue = _colorSchemesSettings.overrideDefaultColors;
                    bool? json = jsonColors.Get<bool>(fieldName);

                    if (json != activeValue)
                    {
                        _contents.Add(new ListObject($"[Colors] {settingName}", $"{activeValue} -> {json}"));

                        _modifiedParameters.OverrideColorScheme =
                            json.Value ? _colorSchemesSettings.GetOverrideColorScheme() : null;
                    }
                }

                _modifiedMainSettings = null;
#if !V1_37_1
                _cachedMainSettings = null;
#endif
                CustomData? jsonGraphics = settings.Get<CustomData>("_graphics");
                if (jsonGraphics != null)
                {
                    List<Dictionary<string, object>> settableGraphicsSettings =
                        SettingSetterSettableSettingsManager.SettingsTable["_graphics"];

#if !PRE_V1_39_1
                    BeatSaber.Settings.QualitySettings qualitySettings = _settingsManager.settings.quality;
                    _cachedMainSettings = new SettableMainSettings(
                        (int)qualitySettings.mirror,
                        (int)qualitySettings.mainEffect,
                        qualitySettings.smokeGraphics,
                        qualitySettings.burnMarkTrails,
                        qualitySettings.screenDisplacementEffects,
                        qualitySettings.maxShockwaveParticles);

                    _modifiedMainSettings = _cachedMainSettings with { };
#elif V1_37_1
                    if (!_graphicSettingsHandler.TryGetCurrentPerformancePreset(out PerformancePreset? preset))
                    {
                        throw new Exception("Could not get current performance preset.");
                    }

                    _modifiedMainSettings = new SettableMainSettings(
                        (int)preset.mirrorGraphics,
                        (int)preset.mainEffectGraphics,
                        preset.smokeGraphics,
                        preset.burnMarkTrails,
                        preset.screenDisplacementEffects,
                        preset.maxShockwaveParticles);
#else
                    _cachedMainSettings = new SettableMainSettings(
                        _mainSettings.mirrorGraphicsSettings,
                        _mainSettings.mainEffectGraphicsSettings,
                        _mainSettings.smokeGraphicsSettings,
                        _mainSettings.burnMarkTrailsEnabled,
                        _mainSettings.screenDisplacementEffectsEnabled,
                        _mainSettings.maxShockwaveParticles);

                    _modifiedMainSettings = _cachedMainSettings with { };
#endif

                    foreach (Dictionary<string, object> settableGraphicSetting in settableGraphicsSettings)
                    {
                        string settingName = (string)settableGraphicSetting["_name"];
                        string fieldName = (string)settableGraphicSetting["_fieldName"];

                        object? json = jsonGraphics.Get<object>(fieldName);
                        if (json == null)
                        {
                            continue;
                        }

                        FieldInfo field =
                            typeof(SettableMainSettings).GetField(
                                fieldName,
                                BindingFlags.Instance | BindingFlags.NonPublic) ??
                            throw new InvalidOperationException($"Unable to find field with name {fieldName}.");
                        object activeValue = field.GetValue(_modifiedMainSettings);
                        if (json is IConvertible)
                        {
                            json = Convert.ChangeType(json, activeValue.GetType());
                        }

                        if (json.Equals(activeValue))
                        {
                            continue;
                        }

                        _contents.Add(new ListObject($"[Graphics] {settingName}", $"{activeValue} -> {json}"));
                        field.SetValue(_modifiedMainSettings, json);
                    }
                }

                _settableSettingsToSet = null;
                foreach ((string s, Dictionary<string, ISettableSetting> value) in SettingSetterSettableSettingsManager
                             .SettableSettings)
                {
                    CustomData? jsonGroup = settings.Get<CustomData>(s);
                    if (jsonGroup == null)
                    {
                        continue;
                    }

                    _settableSettingsToSet = [];

                    foreach ((string key, ISettableSetting settableSetting) in value)
                    {
                        object? json = jsonGroup.Get<object>(key);
                        object activeValue = settableSetting.TrueValue;
                        if (json == null || json.Equals(activeValue))
                        {
                            continue;
                        }

                        _contents.Add(
                            new ListObject(
                                $"[{settableSetting.GroupName}] {settableSetting.FieldName}",
                                $"{activeValue} -> {json}"));
                        _settableSettingsToSet.Add((settableSetting, json));
                    }
                }

                if (_contents.Count != 0)
                {
                    if (_contentObject != null)
                    {
                        Destroy(_contentObject);
                    }

                    if (_isMultiplayer)
                    {
                        if (_declineButton != null)
                        {
                            _declineButton.SetActive(true);
                        }
                    }

                    ParametersModified?.Invoke(_modifiedParameters);
                    _declined = false;
                    _defaultParameters = startParameters;
                    _bsmlParser.Parse(ContentBSML, gameObject, this);
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            _log.Error("Could not setup settable settings");
            _log.Error(e);
        }

        return false;
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

        if (!addedToHierarchy)
        {
            return;
        }

        RectTransform rect = (RectTransform)_topVertical.transform;
#if !PRE_V1_37_1
        rect.anchorMax = new Vector2(rect.anchorMax.x, 1.55f);
#else
        rect.anchorMax = new Vector2(rect.anchorMax.x, 1.25f);
#endif
    }

    [UsedImplicitly]
    private void OnPlay()
    {
        if (_declined)
        {
            return;
        }

        if (_modifiedMainSettings != null)
        {
            _log.Trace("Main settings modified");

#if !PRE_V1_39_1
            _settingsManager.settings.quality.mirror =
                (BeatSaber.Settings.QualitySettings.MirrorQuality)_modifiedMainSettings.MirrorGraphicsSettings;
            _settingsManager.settings.quality.mainEffect =
                (BeatSaber.Settings.QualitySettings.MainEffectOption)_modifiedMainSettings.MainEffectGraphicsSettings;
            _settingsManager.settings.quality.smokeGraphics = _modifiedMainSettings.SmokeGraphicsSettings;
            _settingsManager.settings.quality.depthTexture = _modifiedMainSettings.SmokeGraphicsSettings;
            _settingsManager.settings.quality.burnMarkTrails = _modifiedMainSettings.BurnMarkTrailsEnabled;
            _settingsManager.settings.quality.screenDisplacementEffects = _modifiedMainSettings.ScreenDisplacementEffectsEnabled;
            _settingsManager.settings.quality.maxShockwaveParticles = _modifiedMainSettings.MaxShockwaveParticles;
#elif V1_37_1
            _performancePresetOverride.SettingsOverride = _modifiedMainSettings;
#else
            _mainSettings.mirrorGraphicsSettings.value = _modifiedMainSettings.MirrorGraphicsSettings;
            _mainSettings.mainEffectGraphicsSettings.value = _modifiedMainSettings.MainEffectGraphicsSettings;
            _mainSettings.smokeGraphicsSettings.value = _modifiedMainSettings.SmokeGraphicsSettings;
            _mainSettings.burnMarkTrailsEnabled.value = _modifiedMainSettings.BurnMarkTrailsEnabled;
            _mainSettings.screenDisplacementEffectsEnabled.value =
                _modifiedMainSettings.ScreenDisplacementEffectsEnabled;
            _mainSettings.maxShockwaveParticles.value = _modifiedMainSettings.MaxShockwaveParticles;
            _mainSettings.depthTextureEnabled.value = _mainSettings.smokeGraphicsSettings;
            _mainSystemInit.Init();
#endif
            _modifiedMainSettings = null;
        }

        // ReSharper disable once InvertIf
        if (_settableSettingsToSet != null)
        {
            foreach ((ISettableSetting settableSetting, object value) in _settableSettingsToSet)
            {
                _log.Trace(
                    $"Set settable setting [{settableSetting.FieldName}] in [{settableSetting.GroupName}] to [{value}]");
                settableSetting.SetTemporary(value);
            }
        }
    }

    [UsedImplicitly]
    private void OnFinish()
    {
        if (_settableSettingsToSet != null)
        {
            foreach ((ISettableSetting settableSetting, object _) in _settableSettingsToSet)
            {
                _log.Trace($"Restored settable setting [{settableSetting.FieldName}] in [{settableSetting.GroupName}]");
                settableSetting.SetTemporary(null);
            }

            _settableSettingsToSet = null;
        }

        // Needed for multiplayer
        // ReSharper disable once InvertIf
        if (_cachedOverrideEnvironmentSettings != null)
        {
            _playerDataModel.playerData.SetProperty("overrideEnvironmentSettings", _cachedOverrideEnvironmentSettings);
            _cachedOverrideEnvironmentSettings = null;
        }

#if !V1_37_1
        if (_cachedMainSettings == null)
        {
            return;
        }

        _log.Trace("Main settings restored");

#if !PRE_V1_39_1
        _settingsManager.settings.quality.mirror =
            (BeatSaber.Settings.QualitySettings.MirrorQuality)_cachedMainSettings.MirrorGraphicsSettings;
        _settingsManager.settings.quality.mainEffect =
            (BeatSaber.Settings.QualitySettings.MainEffectOption)_cachedMainSettings.MainEffectGraphicsSettings;
        _settingsManager.settings.quality.smokeGraphics = _cachedMainSettings.SmokeGraphicsSettings;
        _settingsManager.settings.quality.burnMarkTrails = _cachedMainSettings.BurnMarkTrailsEnabled;
        _settingsManager.settings.quality.screenDisplacementEffects = _cachedMainSettings.ScreenDisplacementEffectsEnabled;
        _settingsManager.settings.quality.maxShockwaveParticles = _cachedMainSettings.MaxShockwaveParticles;
#else
        _mainSettings.mirrorGraphicsSettings.value = _cachedMainSettings.MirrorGraphicsSettings;
        _mainSettings.mainEffectGraphicsSettings.value = _cachedMainSettings.MainEffectGraphicsSettings;
        _mainSettings.smokeGraphicsSettings.value = _cachedMainSettings.SmokeGraphicsSettings;
        _mainSettings.burnMarkTrailsEnabled.value = _cachedMainSettings.BurnMarkTrailsEnabled;
        _mainSettings.screenDisplacementEffectsEnabled.value = _cachedMainSettings.ScreenDisplacementEffectsEnabled;
        _mainSettings.maxShockwaveParticles.value = _cachedMainSettings.MaxShockwaveParticles;
        _mainSettings.depthTextureEnabled.value = _mainSettings.smokeGraphicsSettings;
        _mainSystemInit.Init();
#endif

        _cachedMainSettings = null;
#endif
    }

    [UsedImplicitly]
    [Inject]
    private void Construct(
        SiraLog log,
        GameplaySetupViewController gameplaySetupViewController,
#if !V1_29_1
        BSMLParser bsmlParser,
#endif
#if !PRE_V1_39_1
        SettingsManager settingsManager,
#elif V1_37_1
        GraphicSettingsHandler graphicSettingsHandler,
        PerformancePresetOverride performancePresetOverride,
#endif
        PlayerDataModel playerDataModel)
    {
        _log = log;
        _colorSchemesSettings = gameplaySetupViewController.colorSchemesSettings;
        _playerDataModel = playerDataModel;
#if !V1_29_1
        _bsmlParser = bsmlParser;
#else
        _bsmlParser = BSMLParser.instance;
#endif
#if !PRE_V1_39_1
        _settingsManager = settingsManager;
#elif V1_37_1
        _graphicSettingsHandler = graphicSettingsHandler;
        _performancePresetOverride = performancePresetOverride;
#else
        _mainSettings = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>().First();
        _mainSystemInit = Resources.FindObjectsOfTypeAll<MainSystemInit>().First();
#endif
    }

    [UsedImplicitly]
    [UIAction("decline-click")]
    private void OnDeclineClick()
    {
        if (_defaultParameters == null)
        {
            throw new InvalidOperationException($"[{nameof(_defaultParameters)}] was null.");
        }

#if !V1_37_1
        _cachedMainSettings = null;
#endif
        _modifiedMainSettings = null;
        _settableSettingsToSet = null;
        if (_cachedOverrideEnvironmentSettings != null)
        {
            _playerDataModel.playerData.SetProperty("overrideEnvironmentSettings", _cachedOverrideEnvironmentSettings);
            _cachedOverrideEnvironmentSettings = null;
        }

        _declined = true;
        ParametersModified?.Invoke(_defaultParameters);
        Finished?.Invoke();
    }

    [UsedImplicitly]
    [UIAction("accept-click")]
    private void OnAcceptClick()
    {
        if (_modifiedParameters == null)
        {
            throw new InvalidOperationException($"[{nameof(_modifiedParameters)}] was null.");
        }

        Finished?.Invoke();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private readonly struct ListObject
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

    [SuppressMessage("ReSharper", "ConvertToAutoProperty", Justification = "Fields are set through reflection")]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal record SettableMainSettings
    {
        private int _mirrorGraphicsSettings;
        private int _mainEffectGraphicsSettings;
        private bool _smokeGraphicsSettings;
        private bool _burnMarkTrailsEnabled;
        private bool _screenDisplacementEffectsEnabled;
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
