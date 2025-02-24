#if V1_37_1
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatSaber.GameSettings;
using BeatSaber.PerformancePresets;
using BGLib.JsonExtension;
using HarmonyLib;
using Heck.SettingsSetter;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Heck.HarmonyPatches;

[HeckPatch]
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal static class PerformancePresetOverride
{
    internal static SettingsSetterViewController.SettableMainSettings? SettingsOverride { get; set; }

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(GraphicSettingsHandler)
            .GetMethods(AccessTools.all)
            .Where(n => n.Name == nameof(GraphicSettingsHandler.TryGetCurrentPerformancePreset));
    }

    [HarmonyPrefix]
    private static void Prefix(ref PerformancePreset? __state, ref PerformancePreset ____currentPreset)
    {
        if (SettingsOverride == null)
        {
            return;
        }

        __state = ____currentPreset;
        CustomPerformancePreset customPerformancePreset = JsonConvert.DeserializeObject<CustomPerformancePreset>(
            JsonConvert.SerializeObject(____currentPreset, JsonSettings.compactWithDefault))!;
        customPerformancePreset.presetName = "HeckPresetOverride";
        customPerformancePreset.mirrorGraphics = (MirrorQualityPreset)SettingsOverride.MirrorGraphicsSettings;
        customPerformancePreset.mainEffectGraphics =
            (MainEffectPreset)SettingsOverride.MainEffectGraphicsSettings;
        customPerformancePreset.smokeGraphics = SettingsOverride.SmokeGraphicsSettings;
        customPerformancePreset.depthTexture = SettingsOverride.SmokeGraphicsSettings;
        customPerformancePreset.burnMarkTrails = SettingsOverride.BurnMarkTrailsEnabled;
        customPerformancePreset.screenDisplacementEffects =
            SettingsOverride.ScreenDisplacementEffectsEnabled;
        customPerformancePreset.maxShockwaveParticles = SettingsOverride.MaxShockwaveParticles;
        ____currentPreset = customPerformancePreset;
    }

    [HarmonyPostfix]
    private static void Postfix(ref PerformancePreset? __state, ref PerformancePreset ____currentPreset)
    {
        if (__state != null)
        {
            ____currentPreset = __state;
        }
    }
}
#endif
