using System.Collections.Generic;
using Chroma.Lighting;
using Heck;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Events
{
    internal class RingRotationChromafier : IAffinity
    {
        private readonly Dictionary<TrackLaneRingsRotationEffect, ChromaRingsRotationEffect> _chromaRings = new();

        private readonly ChromaRingsRotationEffect.Factory _factory;
        private readonly DeserializedData _deserializedData;

        private RingRotationChromafier(
            ChromaRingsRotationEffect.Factory factory,
            [Inject(Id = ChromaController.ID)] DeserializedData deserializedData)
        {
            _factory = factory;
            _deserializedData = deserializedData;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(TrackLaneRingsRotationEffectSpawner), nameof(TrackLaneRingsRotationEffectSpawner.Start))]
        private void CreateChromaRing(TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect)
        {
            // custom platforms (terrible acronym) causes this to run twice for some reason, so stop the second
            if (_chromaRings.ContainsKey(____trackLaneRingsRotationEffect))
            {
                return;
            }

            _chromaRings.Add(____trackLaneRingsRotationEffect, _factory.Create(____trackLaneRingsRotationEffect));
            ____trackLaneRingsRotationEffect.enabled = false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(TrackLaneRingsRotationEffect), nameof(TrackLaneRingsRotationEffect.FixedUpdate))]
        private bool DisableUpdate()
        {
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(TrackLaneRingsRotationEffect), nameof(TrackLaneRingsRotationEffect.AddRingRotationEffect))]
        private bool RerouteAddRingRotation(TrackLaneRingsRotationEffect __instance, float angle, float step, int propagationSpeed, float flexySpeed)
        {
            if (_chromaRings.TryGetValue(__instance, out ChromaRingsRotationEffect chromaRing))
            {
                chromaRing.AddRingRotationEffect(angle, step, propagationSpeed, flexySpeed);
            }

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(TrackLaneRingsRotationEffectSpawner), nameof(TrackLaneRingsRotationEffectSpawner.HandleBeatmapEvent))]
        private bool ChromaRingHandleCallback(
            TrackLaneRingsRotationEffectSpawner __instance,
            BasicBeatmapEventData basicBeatmapEventData,
            TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect,
            float ____rotation,
            float ____rotationStep,
            int ____rotationPropagationSpeed,
            float ____rotationFlexySpeed,
            TrackLaneRingsRotationEffectSpawner.RotationStepType ____rotationStepType)
        {
            if (!_deserializedData.Resolve(basicBeatmapEventData, out ChromaEventData? chromaData))
            {
                return false;
            }

            // Added in 1.8
            float rotationStep = ____rotationStepType switch
            {
                TrackLaneRingsRotationEffectSpawner.RotationStepType.Range0ToMax =>
                    Random.Range(0f, ____rotationStep),
                TrackLaneRingsRotationEffectSpawner.RotationStepType.Range =>
                    Random.Range(-____rotationStep, ____rotationStep),
                TrackLaneRingsRotationEffectSpawner.RotationStepType.MaxOr0 =>
                    (Random.value < 0.5f) ? ____rotationStep : 0f,
                _ => 0f
            };

            string? nameFilter = chromaData.NameFilter;
            if (nameFilter != null)
            {
                if (!__instance.name.ToLower().Equals(nameFilter.ToLower()))
                {
                    return false;
                }
            }

            int? dir = chromaData.Direction;

            bool rotRight;
            if (!dir.HasValue)
            {
                rotRight = Random.value < 0.5f;
            }
            else
            {
                rotRight = dir == 1;
            }

            bool? counterSpin = chromaData.CounterSpin;
            if (counterSpin is true)
            {
                if (!__instance.name.Contains("Big"))
                {
                    rotRight = !rotRight;
                }
            }

            bool? reset = chromaData.Reset;
            if (reset is true)
            {
                TriggerRotation(____trackLaneRingsRotationEffect, rotRight, ____rotation, 0, 50, 50);
                return false;
            }

            float step = chromaData.Step.GetValueOrDefault(rotationStep);
            float prop = chromaData.Prop.GetValueOrDefault(____rotationPropagationSpeed);
            float speed = chromaData.Speed.GetValueOrDefault(____rotationFlexySpeed);
            float rotation = chromaData.Rotation.GetValueOrDefault(____rotation);

            float stepMult = chromaData.StepMult;
            float propMult = chromaData.PropMult;
            float speedMult = chromaData.SpeedMult;

            TriggerRotation(____trackLaneRingsRotationEffect, rotRight, rotation, step * stepMult, prop * propMult, speed * speedMult);
            return false;
        }

        private void TriggerRotation(
            TrackLaneRingsRotationEffect trackLaneRingsRotationEffect,
            bool rotRight,
            float rotation,
            float rotationStep,
            float rotationPropagationSpeed,
            float rotationFlexySpeed)
        {
            _chromaRings[trackLaneRingsRotationEffect].AddRingRotationEffect(
                trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (rotation * (rotRight ? -1 : 1)),
                rotationStep,
                rotationPropagationSpeed,
                rotationFlexySpeed);
        }
    }
}
