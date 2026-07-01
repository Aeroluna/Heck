using System;
using System.Collections.Generic;
using System.Reflection;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Events;

/// <summary>
/// Applies Chroma custom colors to GLS (LightColorGroupEffect) events.
///
/// V3 maps: CustomJSONData injects <see cref="ICustomData"/> into <c>CustomLightColorBeatmapEventData</c>
/// via its <c>LightColorBaseDataConvertV3</c> transpiler. Colors are read directly from the event's
/// <c>customData["color"]</c> at playback time — no file I/O or lookup table required.
///
/// V4 maps: not yet supported. Will be handled by V4→V3 deserialization when implemented.
///
/// Patch strategy:
///   POSTFIX HandleColorChangeBeatmapEvent — after SetData has run, patch _fromColor/_toColor directly.
///     SetColor(t) does Color.LerpUnclamped(_fromColor, _toColor, t) every frame, so these two fields
///     fully control color interpolation. We preserve the game's alpha (brightness) from each field
///     and replace only the RGB with the custom color. _fromColor gets currentEventData's color;
///     _toColor gets nextSameTypeEventData's color (if it has a custom color), otherwise left alone.
/// </summary>
internal class GlsColorChromafier : IAffinity, IInitializable
{
    private static readonly FieldInfo FromColorField =
        typeof(LightColorGroupEffect).GetField("_fromColor", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo ToColorField =
        typeof(LightColorGroupEffect).GetField("_toColor", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo AltFromColorField =
        typeof(LightColorGroupEffect).GetField("_alternativeFromColor", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo AltToColorField =
        typeof(LightColorGroupEffect).GetField("_alternativeToColor", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo SetColorMethod =
        typeof(LightColorGroupEffect).GetMethod("SetColor", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <inheritdoc/>
    public void Initialize()
    {
    }

    private static void ApplyColorWithAlpha(FieldInfo field, LightColorGroupEffect instance, Color customColor)
    {
        Color existing = (Color)field.GetValue(instance);
        field.SetValue(instance, customColor.ColorWithAlpha(existing.a));
    }

    private static Color? ResolveCustomColor(LightColorBeatmapEventData eventData)
    {
        if (eventData is ICustomData customDataEvent)
        {
            return customDataEvent.customData.GetColor("color");
        }

        return null;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(LightColorGroupEffect), nameof(LightColorGroupEffect.HandleColorChangeBeatmapEvent))]
    private void PostfixHandleColorChange(
        LightColorGroupEffect __instance,
        LightColorBeatmapEventData currentEventData)
    {
        Color? fromColor = ResolveCustomColor(currentEventData);
        var nextEventData = currentEventData.nextSameTypeEventData as LightColorBeatmapEventData;
        bool hasTween = nextEventData != null && nextEventData.easeType != EaseType.None;
        Color? toColor = hasTween ? ResolveCustomColor(nextEventData!) : fromColor;

        if (fromColor.HasValue)
        {
            ApplyColorWithAlpha(FromColorField, __instance, fromColor.Value);
            ApplyColorWithAlpha(AltFromColorField, __instance, fromColor.Value);

            if (!hasTween)
            {
                // Without this we would deviate from the out of the box behavior and change to the next color immediately instead of waiting for its node.
                // This is because the default implementation sets the color to 0f when there is no tween. It looks dumb AF, though.
                SetColorMethod.Invoke(__instance, new object[] { 0f });
                return;
            }
        }

        if (toColor.HasValue)
        {
            ApplyColorWithAlpha(ToColorField, __instance, toColor.Value);
            ApplyColorWithAlpha(AltToColorField, __instance, toColor.Value);
        }
    }
}
