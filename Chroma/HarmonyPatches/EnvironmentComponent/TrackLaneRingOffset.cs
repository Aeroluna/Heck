using System.Collections.Generic;
using HarmonyLib;
using Heck;
using Heck.Animation.Transform;
using IPA.Utilities;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    // This whole file effectively changes _rotZ and _posZ from directly affecting the coordinate to being an offset
    [HeckPatch(PatchType.Environment)]
    internal class TrackLaneRingOffset : IAffinity
    {
        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _rotZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_rotZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _posZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_posZ");

        private readonly Dictionary<TrackLaneRing, Quaternion> _rotationOffsets = new();

        internal static void UpdatePosition(TrackLaneRing trackLaneRing)
        {
            _positionOffsetAccessor(ref trackLaneRing) = trackLaneRing.transform.localPosition;
        }

        internal void SetTransform(TrackLaneRing trackLaneRing, TransformData transformData)
        {
            if (transformData.Position.HasValue || transformData.LocalPosition.HasValue)
            {
                UpdatePosition(trackLaneRing);
                _positionOffsetAccessor(ref trackLaneRing) = trackLaneRing.transform.localPosition;
                _posZAccessor(ref trackLaneRing) = 0;
            }

            // ReSharper disable once InvertIf
            if (transformData.Rotation.HasValue || transformData.LocalRotation.HasValue)
            {
                UpdateRotation(trackLaneRing);
                _rotZAccessor(ref trackLaneRing) = 0;
            }
        }

        internal void UpdateRotation(TrackLaneRing trackLaneRing)
        {
            _rotationOffsets[trackLaneRing] = trackLaneRing.transform.localRotation;
        }

        // 7 days...
        internal void CopyRing(TrackLaneRing originalRing, TrackLaneRing newRing)
        {
            if (_rotationOffsets.TryGetValue(originalRing, out Quaternion offset))
            {
                _rotationOffsets.Add(newRing, offset);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TrackLaneRing), nameof(TrackLaneRing.Init))]
        private static void TrackLaneRingInit(ref float ____posZ, Vector3 position)
        {
            ____posZ = position.z;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TrackLaneRing), nameof(TrackLaneRing.FixedUpdateRing))]
        private static bool TrackLaneRingFixedUpdateRing(
            float fixedDeltaTime,
            ref float ____prevRotZ,
            ref float ____rotZ,
            ref float ____prevPosZ,
            ref float ____posZ,
            float ____destRotZ,
            float ____rotationSpeed,
            float ____destPosZ,
            float ____moveSpeed)
        {
            ____prevRotZ = ____rotZ;
            ____rotZ = Mathf.Lerp(____rotZ, ____destRotZ, fixedDeltaTime * ____rotationSpeed);
            ____prevPosZ = ____posZ;
            ____posZ = Mathf.Lerp(____posZ, ____destPosZ, fixedDeltaTime * ____moveSpeed);

            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(TrackLaneRing), nameof(TrackLaneRing.LateUpdateRing))]
        private bool ApplyOffset(
            TrackLaneRing __instance,
            float interpolationFactor,
            float ____prevRotZ,
            float ____rotZ,
            Vector3 ____positionOffset,
            float ____prevPosZ,
            float ____posZ,
            Transform ____transform)
        {
            if (!_rotationOffsets.TryGetValue(__instance, out Quaternion rotation))
            {
                rotation = Quaternion.identity;
            }

            float interpolatedZPos = ____prevPosZ + ((____posZ - ____prevPosZ) * interpolationFactor);
            Vector3 positionZOffset = rotation * Vector3.forward * interpolatedZPos;
            Vector3 pos = ____positionOffset + positionZOffset;

            float interpolatedZRot = ____prevRotZ + ((____rotZ - ____prevRotZ) * interpolationFactor);
            Quaternion rotationZOffset = Quaternion.AngleAxis(interpolatedZRot, Vector3.forward);
            Quaternion rot = rotation * rotationZOffset;

            ____transform.localRotation = rot;
            ____transform.localPosition = pos;

            return false;
        }
    }
}
