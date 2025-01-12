#if V1_37_1
using BeatSaber.GameSettings;
using BeatSaber.PerformancePresets;
using BGLib.JsonExtension;
using Heck.SettingsSetter;
using Newtonsoft.Json;
using SiraUtil.Affinity;

namespace Heck.HarmonyPatches;

internal class PerformancePresetOverride : IAffinity
{
    private readonly GraphicSettingsHandler _graphicSettingsHandler;

    private PerformancePresetOverride(GraphicSettingsHandler graphicSettingsHandler)
    {
        _graphicSettingsHandler = graphicSettingsHandler;
    }

    internal SettingsSetterViewController.SettableMainSettings? SettingsOverride { get; set; }

    [AffinityPrefix]
    [AffinityPatch(typeof(SettingsApplicatorSO), nameof(SettingsApplicatorSO.ApplyPerformancePreset))]
    private void Prefix(ref PerformancePreset preset)
    {
        if (SettingsOverride == null)
        {
            return;
        }

        CustomPerformancePreset customPerformancePreset = JsonConvert.DeserializeObject<CustomPerformancePreset>(
            JsonConvert.SerializeObject(preset, JsonSettings.compactWithDefault))!;
        customPerformancePreset.presetName = "HeckPresetOverride";
        customPerformancePreset.mirrorGraphics = (MirrorQualityPreset)SettingsOverride.MirrorGraphicsSettings;
        customPerformancePreset.mainEffectGraphics =
            (MainEffectPreset)SettingsOverride.MainEffectGraphicsSettings;
        customPerformancePreset.smokeGraphics = SettingsOverride.SmokeGraphicsSettings;
        customPerformancePreset.burnMarkTrails = SettingsOverride.BurnMarkTrailsEnabled;
        customPerformancePreset.screenDisplacementEffects =
            SettingsOverride.ScreenDisplacementEffectsEnabled;
        customPerformancePreset.maxShockwaveParticles = SettingsOverride.MaxShockwaveParticles;
        preset = customPerformancePreset;
        _graphicSettingsHandler._currentPreset = customPerformancePreset;
        SettingsOverride = null;
    }
}
#endif
