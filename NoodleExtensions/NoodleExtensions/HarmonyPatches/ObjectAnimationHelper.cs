using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CustomJSONData.CustomBeatmap;
using CustomJSONData;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using static NoodleExtensions.Plugin;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.HarmonyPatches
{
    internal class ObjectAnimationHelper
    {
        internal static readonly MethodInfo AddComposite = SymbolExtensions.GetMethodInfo(() => AddCompositePos(new Vector3()));
        internal static readonly MethodInfo AddCompositeX = SymbolExtensions.GetMethodInfo(() => AddCompositePosX(0));
        internal static readonly MethodInfo AddCompositeY = SymbolExtensions.GetMethodInfo(() => AddCompositePosY(0));
        internal static readonly MethodInfo AddCompositeZ = SymbolExtensions.GetMethodInfo(() => AddCompositePosZ(0));
        private static Quaternion _; // this is literally just for the line below
        internal static readonly MethodInfo HandleNote = SymbolExtensions.GetMethodInfo(() => HandleNoteAnimation(0, ref _, ref _, null));
        internal static readonly MethodInfo AddFinalPos = SymbolExtensions.GetMethodInfo(() => AddActivePosition(new Vector3()));
        private static Vector3 compositePos;
        private static PositionData activePositionData;
        private static float trueTime;
        private static float HandleNoteAnimation(float time, ref Quaternion _worldRotation, ref Quaternion _inverseWorldRotation, MonoBehaviour monoBehaviour)
        {
            compositePos = new Vector3();
            activePositionData = null;
            trueTime = time;
            NoteData noteData = NoteControllerUpdate.cachedNoteData;
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                Quaternion? rotation = GetWorldRotation(dynData, time);
                if (rotation.HasValue)
                {
                    _worldRotation = rotation.Value;
                    _inverseWorldRotation = Quaternion.Inverse(rotation.Value);
                }

                Quaternion? localRotation = GetLocalRotation(dynData, time);

                monoBehaviour.transform.localRotation = _worldRotation * localRotation.GetValueOrDefault(Quaternion.identity);

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
                            compositePos += pos.endPosition * _noteLinesDistance;
                        }
                        else
                        {
                            if (!pos.relative) movementTime += time - pos.time;
                            activePositionData = pos;
                        }
                    }
                    return time - movementTime;
                }
            }
            return time;
        }

        private static Vector3 AddCompositePos(Vector3 original)
        {
            return original + compositePos;
        }

        private static float AddCompositePosX(float x)
        {
            return x + compositePos.x;
        }

        private static float AddCompositePosY(float y)
        {
            return y + compositePos.y;
        }

        private static float AddCompositePosZ(float z)
        {
            return z + compositePos.z;
        }

        private static Vector3 AddActivePosition(Vector3 original)
        {
            if (activePositionData != null) return original + Vector3.Lerp(activePositionData.startPosition, activePositionData.endPosition,
                            Easings.Interpolate((trueTime - activePositionData.time) / activePositionData.duration, activePositionData.easing)) * _noteLinesDistance;
            return original;
        }

        internal static Quaternion? GetWorldRotation(dynamic dynData, float time)
        {
            Quaternion? worldRotation = null;

            dynamic _rotation = Trees.at(dynData, ROTATION);
            if (_rotation != null)
            {
                if (_rotation is List<object> list)
                {
                    IEnumerable<float> _rot = (list)?.Select(Convert.ToSingle);
                    worldRotation = Quaternion.Euler(_rot.ElementAt(0), _rot.ElementAt(1), _rot.ElementAt(2));
                }
                else worldRotation = Quaternion.Euler(0, (float)_rotation, 0);
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

            IEnumerable<float> _localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(Convert.ToSingle);
            if (_localrot != null) localRotation = Quaternion.Euler(_localrot.ElementAt(0), _localrot.ElementAt(1), _localrot.ElementAt(2));

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
