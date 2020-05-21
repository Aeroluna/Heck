using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    internal class ObjectAnimationHelper
    {
        internal static readonly MethodInfo _addCompositePos = SymbolExtensions.GetMethodInfo(() => AddCompositePos(new Vector3()));
        internal static readonly MethodInfo _addCompositeX = SymbolExtensions.GetMethodInfo(() => AddCompositeX(0));
        internal static readonly MethodInfo _addCompositeY = SymbolExtensions.GetMethodInfo(() => AddCompositeY(0));
        internal static readonly MethodInfo _addCompositeZ = SymbolExtensions.GetMethodInfo(() => AddCompositeZ(0));
        private static Quaternion _; // this is literally just for the line below
        internal static readonly MethodInfo _handleNoteAnimation = SymbolExtensions.GetMethodInfo(() => HandleNoteAnimation(0, ref _, ref _, null));
        internal static readonly MethodInfo _addActivePosition = SymbolExtensions.GetMethodInfo(() => AddActivePosition(new Vector3()));
        private static Vector3 _compositePos;
        private static PositionData _activePositionData;
        private static float _trueTime;

        private static float HandleNoteAnimation(float time, ref Quaternion worldRotation, ref Quaternion inverseWorldRotation, MonoBehaviour monoBehaviour)
        {
            _compositePos = new Vector3();
            _activePositionData = null;
            _trueTime = time;
            NoteData noteData = NoteControllerUpdate._cachedNoteData;
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                Quaternion? rotation = GetWorldRotation(dynData, time);
                if (rotation.HasValue)
                {
                    worldRotation = rotation.Value;
                    inverseWorldRotation = Quaternion.Inverse(rotation.Value);
                }

                Quaternion? localRotation = GetLocalRotation(dynData, time);

                monoBehaviour.transform.localRotation = worldRotation * localRotation.GetValueOrDefault(Quaternion.identity);

                List<PositionData> positionData = Trees.at(dynData, "varPosition");
                if (positionData != null)
                {
                    IEnumerable<PositionData> truncatedPosition = positionData
                        .Where(n => n.time < time);

                    float movementTime = 0;
                    foreach (PositionData pos in truncatedPosition)
                    {
                        if (pos.time + pos.duration < time)
                        {
                            if (!pos.relative) movementTime += pos.duration;
                            _compositePos += pos.endPosition * _noteLinesDistance;
                        }
                        else
                        {
                            if (!pos.relative) movementTime += time - pos.time;
                            _activePositionData = pos;
                        }
                    }
                    return time - movementTime;
                }
            }
            return time;
        }

        private static Vector3 AddCompositePos(Vector3 original)
        {
            return original + _compositePos;
        }

        private static float AddCompositeX(float x)
        {
            return x + _compositePos.x;
        }

        private static float AddCompositeY(float y)
        {
            return y + _compositePos.y;
        }

        private static float AddCompositeZ(float z)
        {
            return z + _compositePos.z;
        }

        private static Vector3 AddActivePosition(Vector3 original)
        {
            if (_activePositionData != null) return original + Vector3.Lerp(_activePositionData.startPosition, _activePositionData.endPosition,
                            Easings.Interpolate((_trueTime - _activePositionData.time) / _activePositionData.duration, _activePositionData.easing)) * _noteLinesDistance;
            return original;
        }

        internal static Quaternion? GetWorldRotation(dynamic dynData, float time)
        {
            Quaternion? worldRotation = null;

            dynamic rotation = Trees.at(dynData, ROTATION);
            if (rotation != null)
            {
                if (rotation is List<object> list)
                {
                    IEnumerable<float> _rot = (list)?.Select(Convert.ToSingle);
                    worldRotation = Quaternion.Euler(_rot.ElementAt(0), _rot.ElementAt(1), _rot.ElementAt(2));
                }
                else worldRotation = Quaternion.Euler(0, (float)rotation, 0);
            }

            List<RotationData> rotationData = Trees.at(dynData, "varRotation");
            if (rotationData != null)
            {
                RotationData truncatedRotation = rotationData
                    .Where(n => n.time < time)
                    .Where(n => n.time + n.duration > time)
                    .LastOrDefault();
                if (truncatedRotation != null)
                {
                    return Quaternion.Lerp(truncatedRotation.startRotation, truncatedRotation.endRotation,
                        Easings.Interpolate((time - truncatedRotation.time) / truncatedRotation.duration, truncatedRotation.easing));
                }
            }
            return worldRotation;
        }

        internal static Quaternion? GetLocalRotation(dynamic dynData, float time)
        {
            Quaternion? localRotation = null;

            IEnumerable<float> localRotRaw = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(Convert.ToSingle);
            if (localRotRaw != null) localRotation = Quaternion.Euler(localRotRaw.ElementAt(0), localRotRaw.ElementAt(1), localRotRaw.ElementAt(2));

            List<RotationData> localRotationData = Trees.at(dynData, "varLocalRotation");
            if (localRotationData != null)
            {
                RotationData truncatedRotation = localRotationData
                    .Where(n => n.time < time)
                    .Where(n => n.time + n.duration > time)
                    .LastOrDefault();
                if (truncatedRotation != null)
                    return Quaternion.Lerp(truncatedRotation.startRotation, truncatedRotation.endRotation,
                        Easings.Interpolate((time - truncatedRotation.time) / truncatedRotation.duration, truncatedRotation.easing));
            }
            return localRotation;
        }
    }
}