using System.Collections.Generic;
using Heck;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace Chroma.HarmonyPatches.Events
{
    internal class MovementBeatmapChromafier : IAffinity
    {
        private readonly Dictionary<MovementBeatmapEventEffect, ChromaEventData> _lastBeatmapEventData = new();

        private readonly DeserializedData _deserializedData;

        private MovementBeatmapChromafier([Inject(Id = ChromaController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MovementBeatmapEventEffect), nameof(MovementBeatmapEventEffect.HandleBeatmapEvent))]
        private void SaveBeatmapEvent(MovementBeatmapEventEffect __instance, BasicBeatmapEventData basicBeatmapEventData)
        {
            if (_deserializedData.Resolve(basicBeatmapEventData, out ChromaEventData? chromaEventData))
            {
                _lastBeatmapEventData[__instance] = chromaEventData;
            }
            else
            {
                _lastBeatmapEventData.Remove(__instance);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MovementBeatmapEventEffect), nameof(MovementBeatmapEventEffect.FixedUpdate))]
        private bool ChromaifiedFixedUpdate(
            MovementBeatmapEventEffect __instance,
            ref Vector3 ____prevPositionOffset,
            ref Vector3 ____currentPositionOffset,
            MovementBeatmapEventEffect.MovementData[] ____movementData,
            int ____currentMovementDataIdx,
            float ____transitionSpeed)
        {
            bool customAvail = _lastBeatmapEventData.TryGetValue(__instance, out ChromaEventData chromaEventData);
            Vector3 finalPos;
            if (customAvail &&
                ____movementData.Length == 2 &&
                chromaEventData.Step.HasValue)
            {
                Vector3 dir = (____movementData[1].localPositionOffset - ____movementData[0].localPositionOffset).normalized;
                finalPos = chromaEventData.Step.Value * dir;
            }
            else
            {
                finalPos = ____movementData[____currentMovementDataIdx].localPositionOffset;
            }

            float speed = customAvail && chromaEventData.Speed.HasValue ? chromaEventData.Speed.Value : ____transitionSpeed;

            ____prevPositionOffset = ____currentPositionOffset;
            ____currentPositionOffset = Vector3.LerpUnclamped(____currentPositionOffset, finalPos, Time.fixedDeltaTime * speed);
            if ((____currentPositionOffset - finalPos).sqrMagnitude < 0.01)
            {
                __instance.enabled = false;
            }

            return false;
        }
    }
}
