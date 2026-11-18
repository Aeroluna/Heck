using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.PlayView;
using Hive.Versioning;
using IPA.Loader;
using IPA.Utilities;
using JetBrains.Annotations;
using SiraUtil.Logging;
using TMPro;
using UnityEngine;
using Zenject;
using Version = Hive.Versioning.Version;

namespace Heck.VersionCheck;

[PlayViewControllerSettings(200, "missing dependencies")]
[ViewDefinition("Heck.VersionCheck.VersionCheck.bsml")]
internal class VersionCheckViewController : BSMLAutomaticViewController, IPlayViewController
{
    private static string? _contentBsml;

    [UsedImplicitly]
    [UIObject("contentObject")]
    private readonly GameObject? _contentObject;

    [UsedImplicitly]
    [UIValue("contents")]
    private readonly List<object> _contents = [];

    [UsedImplicitly]
    [UIComponent("header")]
    private readonly TMP_Text _header = null!;

    private SiraLog _log = null!;
    private BSMLParser _bsmlParser = null!;

    private string _songName = string.Empty;

    public event Action? Finished;

    private static string ContentBsml
    {
        get
        {
            if (_contentBsml != null)
            {
                return _contentBsml;
            }

            using StreamReader reader = new(
                typeof(VersionCheckViewController).Assembly.GetManifestResourceStream(
                    "Heck.VersionCheck.VersionCheckContent.bsml") ??
                throw new InvalidOperationException("Failed to retrieve VersionCheckContent.bsml."));
            _contentBsml = reader.ReadToEnd();

            return _contentBsml;
        }
    }

    public bool Init(StartStandardLevelParameters startParameters)
    {
        // When in doubt, wrap everything in one big try catch statement!
        try
        {
#if !PRE_V1_37_1
            CustomData beatmapCustomData =
                startParameters.BeatmapLevel.GetBeatmapCustomData(startParameters.BeatmapKey);
#else
            CustomData beatmapCustomData = startParameters.DifficultyBeatmap.GetBeatmapCustomData();
#endif

            // strictly checks if the mod is loaded unlike songcore
            CustomData? requirements = beatmapCustomData.Get<CustomData>("_requiredVersions");
            if (requirements == null)
            {
                return false;
            }

            _contents.Clear();

            PluginMetadata[] enabledPlugins = PluginManager.EnabledPlugins.ToArray();
            foreach ((string key, object? value) in requirements)
            {
                if (value is not string stringValue)
                {
                    throw new InvalidOperationException($"[{key}] was null.");
                }

                VersionRange requestedRange = new(stringValue);
                Version? foundVersion = null;
                PluginMetadata? found = enabledPlugins.FirstOrDefault(n => n.Id == key);
                if (found != null)
                {
                    // I LOVE HIVE VERSIONING
                    // I LOVE IMPLEMENTING AN ARBITRARY REPLACEMENT FOR SYSTEM.VERSION
                    // I LOVE USING A LIBRARY FOR A DEAD PROJECT FOR NO REASON
                    // I LOVE BEING STRUNG ALONG FOR YEARS FOR A PROJECT THAT WAS OBVIOUSLY NEVER GONNA HAPPEN
                    // sorry i had to vent
                    foundVersion = found.HVersion;

                    if (requestedRange.Matches(foundVersion))
                    {
                        // requested versions found, continue
                        continue;
                    }
                }

                string message = requestedRange.ToString();
                if (foundVersion != null)
                {
                    message += $" (you have {foundVersion.Major}.{foundVersion.Minor}.{foundVersion.Patch})";
                }

                _contents.Add(
                    new ListObject(key, message));
            }

            if (_contents.Count == 0)
            {
                return false;
            }

            if (_contentObject != null)
            {
                Destroy(_contentObject);
            }

#if !PRE_V1_37_1
            _songName = startParameters.BeatmapLevel.songName;
#else
            _songName = startParameters.DifficultyBeatmap.level.songName;
#endif

            _bsmlParser.Parse(ContentBsml, gameObject, this);

            return true;
        }
        catch (Exception e)
        {
            _log.Error("Exception occurred while checking mod versions");
            _log.Error(e);
        }

        return false;
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

        if (addedToHierarchy)
        {
            _header.text =
                $"{_songName} requires mods that are either missing or not up-to-date.\nUpdate your mods to play this map.";
        }
    }

    [UsedImplicitly]
    [Inject]
    private void Construct(
#if !V1_29_1
        BSMLParser bsmlParser,
#endif
        SiraLog log)
    {
#if !V1_29_1
        _bsmlParser = bsmlParser;
#else
        _bsmlParser = BSMLParser.instance;
#endif
        _log = log;
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
}
