using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Lighting;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Heck.Deserialize;
using IPA.Utilities;
using UnityEngine;
using static Chroma.ChromaController;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;
using static Heck.HeckController;

namespace Chroma
{
    internal class ChromaAssignFogEventData : ICustomEventCustomData
    {
        internal ChromaAssignFogEventData(Track track)
        {
            Track = track;
        }

        internal Track Track { get; }
    }

    internal class ChromaAnimateComponentData : ICustomEventCustomData
    {
        internal ChromaAnimateComponentData(
            CustomData customData,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, List<object>> pointDefinitions)
        {
            Track = customData.GetTrackArray(beatmapTracks, false).ToList();
            Duration = customData.Get<float?>(DURATION) ?? 0f;
            Easing = customData.GetStringToEnum<Functions?>(EASING) ?? Functions.easeLinear;

            string[] availableNames = { BLOOM_FOG_ENVIRONMENT, TUBE_BLOOM_PRE_PASS_LIGHT };
            List<KeyValuePair<string, object?>> componentKeys = customData.Where(n => availableNames.Contains(n.Key)).ToList();
            List<(string ComponentName, Dictionary<string, PointDefinition<float>?> PointDefinition)> coroutineInfos = new();
            foreach ((string key, object? value) in componentKeys)
            {
                if (value == null)
                {
                    continue;
                }

                CustomData component = (CustomData)value;
                Dictionary<string, PointDefinition<float>?> componentPoints = component.Keys
                    .ToDictionary(propertyKey => propertyKey, propertyKey => component.GetPointData<float>(propertyKey, pointDefinitions));
                coroutineInfos.Add((key, componentPoints));
            }

            CoroutineInfos = coroutineInfos;
        }

        internal IReadOnlyList<Track> Track { get; }

        internal float Duration { get; }

        internal Functions Easing { get; }

        internal IReadOnlyList<(string ComponentName, Dictionary<string, PointDefinition<float>?> PointDefinition)> CoroutineInfos { get; }
    }

    internal class ChromaNoteData : ChromaObjectData, ICopyable<IObjectCustomData>
    {
        internal ChromaNoteData(
            CustomData customData,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, List<object>> pointDefinitions,
            bool v2)
            : base(customData, beatmapTracks, pointDefinitions, v2)
        {
            SpawnEffect = customData.Get<bool?>(NOTE_SPAWN_EFFECT) ?? !customData.Get<bool?>(V2_DISABLE_SPAWN_EFFECT);
            DisableDebris = customData.Get<bool?>(NOTE_DEBRIS);
        }

        private ChromaNoteData(ChromaNoteData original)
            : base(original)
        {
            SpawnEffect = original.SpawnEffect;
            DisableDebris = original.DisableDebris;
        }

        internal bool? SpawnEffect { get; }

        internal bool? DisableDebris { get; }

        public IObjectCustomData Copy()
        {
            return new ChromaNoteData(this);
        }
    }

    internal class ChromaObjectData : IObjectCustomData
    {
        internal ChromaObjectData(ChromaObjectData original)
        {
            Color = original.Color;
            Track = original.Track;
            LocalPathColor = original.LocalPathColor;
        }

        internal ChromaObjectData(
            CustomData customData,
            Dictionary<string, Track> beatmapTracks,
            Dictionary<string, List<object>> pointDefinitions,
            bool v2)
        {
            Color = CustomDataDeserializer.GetColorFromData(customData, v2);
            CustomData? animationData = customData.Get<CustomData>(v2 ? V2_ANIMATION : ANIMATION);
            if (animationData != null)
            {
                LocalPathColor = animationData.GetPointData<Vector4>(v2 ? V2_COLOR : COLOR, pointDefinitions);
            }

            Track = customData.GetNullableTrackArray(beatmapTracks, v2)?.ToList();
        }

        internal Color? Color { get; }

        internal IReadOnlyList<Track>? Track { get; }

        internal PointDefinition<Vector4>? LocalPathColor { get; }
    }

    internal class ChromaEventData : IEventCustomData
    {
        internal ChromaEventData(
            BasicBeatmapEventData beatmapEventData,
            LegacyLightHelper? legacyLightHelper,
            bool v2)
        {
            CustomData customData = ((ICustomData)beatmapEventData).customData;

            Color? color = CustomDataDeserializer.GetColorFromData(customData, v2);
            if (legacyLightHelper != null)
            {
                color ??= legacyLightHelper.GetLegacyColor(beatmapEventData);
            }

            PropID = v2 ? customData.Get<object>(V2_PROPAGATION_ID) : null;
            ColorData = color;
            Easing = customData.GetStringToEnum<Functions?>(v2 ? V2_EASING : EASING);
            LerpType = customData.GetStringToEnum<LerpType?>(v2 ? V2_LERP_TYPE : LERP_TYPE);
            LockPosition = customData.Get<bool?>(v2 ? V2_LOCK_POSITION : LOCK_POSITION).GetValueOrDefault(false);
            NameFilter = customData.Get<string>(v2 ? V2_NAME_FILTER : NAME_FILTER);
            Direction = customData.Get<int?>(v2 ? V2_DIRECTION : DIRECTION);
            CounterSpin = v2 ? customData.Get<bool?>(V2_COUNTER_SPIN) : null;
            Reset = v2 ? customData.Get<bool?>(V2_RESET) : null;
            Step = customData.Get<float?>(v2 ? V2_STEP : STEP);
            Prop = customData.Get<float?>(v2 ? V2_PROP : PROP);
            Speed = customData.Get<float?>(v2 ? V2_SPEED : SPEED) ?? (v2 ? customData.Get<float?>(V2_PRECISE_SPEED) : null);
            Rotation = customData.Get<float?>(v2 ? V2_ROTATION : RING_ROTATION);
            StepMult = v2 ? customData.Get<float?>(V2_STEP_MULT).GetValueOrDefault(1f) : 1;
            PropMult = v2 ? customData.Get<float?>(V2_PROP_MULT).GetValueOrDefault(1f) : 1;
            SpeedMult = v2 ? customData.Get<float?>(V2_SPEED_MULT).GetValueOrDefault(1f) : 1;

            if (v2)
            {
                CustomData? gradientObject = customData.Get<CustomData>(V2_LIGHT_GRADIENT);
                if (gradientObject != null)
                {
                    GradientObject = new GradientObjectData(
                        gradientObject.Get<float>(V2_DURATION),
                        CustomDataDeserializer.GetColorFromData(gradientObject, V2_START_COLOR) ?? Color.white,
                        CustomDataDeserializer.GetColorFromData(gradientObject, V2_END_COLOR) ?? Color.white,
                        gradientObject.GetStringToEnum<Functions?>(V2_EASING) ?? Functions.easeLinear);
                }
            }

            object? lightID = customData.Get<object>(v2 ? V2_LIGHT_ID : ChromaController.LIGHT_ID);
            if (lightID != null)
            {
                LightID = lightID switch
                {
                    List<object> lightIDobjects => lightIDobjects.Select(Convert.ToInt32).ToArray(),
                    long lightIDint => new[] { (int)lightIDint },
                    _ => null
                };
            }
        }

        internal int[]? LightID { get; }

        internal object? PropID { get; }

        internal Color? ColorData { get; }

        internal GradientObjectData? GradientObject { get; }

        internal Functions? Easing { get; }

        internal LerpType? LerpType { get; }

        internal bool LockPosition { get; }

        internal string? NameFilter { get; }

        internal int? Direction { get; }

        internal bool? CounterSpin { get; }

        internal bool? Reset { get; }

        internal float? Step { get; }

        internal float? Prop { get; }

        internal float? Speed { get; }

        internal float? Rotation { get; }

        internal float StepMult { get; }

        internal float PropMult { get; }

        internal float SpeedMult { get; }

        internal Dictionary<int, BasicBeatmapEventData>? NextSameTypeEvent { get; set; }

        internal class GradientObjectData
        {
            internal GradientObjectData(float duration, Color startColor, Color endColor, Functions easing)
            {
                Duration = duration;
                StartColor = startColor;
                EndColor = endColor;
                Easing = easing;
            }

            internal float Duration { get; }

            internal Color StartColor { get; }

            internal Color EndColor { get; }

            internal Functions Easing { get; }
        }
    }
}
