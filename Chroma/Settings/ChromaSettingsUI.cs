using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Chroma.EnvironmentEnhancement.Saved;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.Settings;

internal class ChromaSettingsUI : IInitializable, IDisposable
{
    private const string NO_ENVIRONMENT = "None";

    private readonly Config _config;
    private readonly GameplaySetup _gameplaySetup;
    private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

#if !PRE_V1_37_1
    private readonly BeatmapDataLoader _beatmapDataLoader;
#else
    private readonly BeatmapDataCache _beatmapDataCache;
#endif

    [UsedImplicitly]
    [UIValue("environmentoptions")]
    private List<object?> _environmentOptions;

    // i wish nico backported the bsml updates :(
    private ChromaSettingsUI(
        Config config,
#if !V1_29_1
        GameplaySetup gameplaySetup,
#endif
        SavedEnvironmentLoader savedEnvironmentLoader,
#if !PRE_V1_37_1
        BeatmapDataLoader beatmapDataLoader)
#else
        BeatmapDataCache beatmapDataCache)
#endif
    {
        _config = config;
#if !V1_29_1
        _gameplaySetup = gameplaySetup;
#else
        _gameplaySetup = GameplaySetup.instance;
#endif
        _savedEnvironmentLoader = savedEnvironmentLoader;
#if !PRE_V1_37_1
        _beatmapDataLoader = beatmapDataLoader;
#else
        _beatmapDataCache = beatmapDataCache;
#endif
        _environmentOptions = _savedEnvironmentLoader.Environments.Keys.Cast<object?>().Prepend(null).ToList();
    }

    public void Initialize()
    {
        _gameplaySetup.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", this);
    }

    public void Dispose()
    {
        _gameplaySetup.RemoveTab("Chroma");
    }

    // TODO: do a comparison instead of just always wiping the cache
    private void ClearCache()
    {
#if !PRE_V1_37_1
        _beatmapDataLoader._lastUsedBeatmapDataCache = default;
#else
        _beatmapDataCache.difficultyBeatmap = null;
#endif
    }

#pragma warning disable CA1822
    [UsedImplicitly]
    [UIValue("rgbevents")]
#pragma warning disable SA1201
    public bool ChromaEventsDisabled
#pragma warning restore SA1201
    {
        get => _config.ChromaEventsDisabled;
        set => _config.ChromaEventsDisabled = value;
    }

    [UsedImplicitly]
    [UIValue("platform")]
    public bool EnvironmentEnhancementsDisabled
    {
        get => _config.EnvironmentEnhancementsDisabled;
        set
        {
            ClearCache();
            _config.EnvironmentEnhancementsDisabled = value;
        }
    }

    [UsedImplicitly]
    [UIValue("notecolors")]
    public bool NoteColoringDisabled
    {
        get => _config.NoteColoringDisabled;
        set => _config.NoteColoringDisabled = value;
    }

    [UsedImplicitly]
    [UIValue("zenwalls")]
    public bool ForceZenWallsEnabled
    {
        get => _config.ForceZenWallsEnabled;
        set
        {
            ClearCache();
            _config.ForceZenWallsEnabled = value;
        }
    }

    [UsedImplicitly]
    [UIValue("environmentenabled")]
    public bool CustomEnvironmentEnabled
    {
        get => _config.CustomEnvironmentEnabled;
        set
        {
            ClearCache();
            _config.CustomEnvironmentEnabled = value;
        }
    }

    [UsedImplicitly]
    [UIValue("environment")]
    public string? CustomEnvironment
    {
        get
        {
            string? name = _config.CustomEnvironment;
            return name != null && _savedEnvironmentLoader.Environments.ContainsKey(name) ? name : null;
        }

        set
        {
            ClearCache();
            _config.CustomEnvironment = value;
        }
    }

    [UsedImplicitly]
    [UIAction("environmentformat")]
    private string FormatCustomEnvironment(string? name)
    {
        if (name == null)
        {
            return NO_ENVIRONMENT;
        }

        SavedEnvironment environment = _savedEnvironmentLoader.Environments[name]!;
        return $"{environment.Name} v{environment.EnvironmentVersion}";
    }
#pragma warning restore CA1822
}
