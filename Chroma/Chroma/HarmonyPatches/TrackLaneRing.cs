namespace Chroma.HarmonyPatches
{
    using UnityEngine;

    // This whole file effectively changes _rotZ and _posZ from directly affecting the coordinate to being an offset
    [ChromaPatch(typeof(TrackLaneRing))]
    [ChromaPatch("Init")]
    internal static class TrackLaneRingInit
    {
        private static void Postfix(ref float ____posZ, Vector3 position)
        {
            ____posZ = position.z;
        }
    }

    [ChromaPatch(typeof(TrackLaneRing))]
    [ChromaPatch("FixedUpdateRing")]
    internal static class TrackLaneRingFixedUpdateRing
    {
        private static bool Prefix(
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
    }

    [ChromaPatch(typeof(TrackLaneRing))]
    [ChromaPatch("LateUpdateRing")]
    internal static class TrackLaneRingLateUpdateRing
    {
        private static bool Prefix(
            TrackLaneRing __instance,
            float interpolationFactor,
            float ____prevRotZ,
            float ____rotZ,
            Vector3 ____positionOffset,
            float ____prevPosZ,
            float ____posZ,
            Transform ____transform)
        {
            if (!EnvironmentEnhancementManager.RingRotationOffsets.TryGetValue(__instance, out Quaternion rotation))
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
