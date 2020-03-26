using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static NoodleExtensions.NoodleController.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions
{
    internal static class NoodleController
    {
        internal static Vector3 GetNoteOffset(BeatmapObjectData beatmapObjectData, float? _startRow, float? _startHeight)
        {
            float distance = -(_noteLinesCount - 1) * 0.5f + (_startRow.HasValue ? _noteLinesCount / 2 : 0); // Add last part to simulate https://github.com/spookyGh0st/beatwalls/#wall
            float lineIndex = _startRow.GetValueOrDefault(beatmapObjectData.lineIndex);
            distance = (distance + lineIndex) * _noteLinesDistance;

            return _rightVec * distance
                + new Vector3(0, LineYPosForLineLayer(beatmapObjectData, _startHeight), 0);
        }

        internal static float LineYPosForLineLayer(BeatmapObjectData beatmapObjectData, float? height)
        {
            float ypos = 0;
            if (height.HasValue)
            {
                ypos = (height.Value * _noteLinesDistance) + _baseLinesYPos; // offset by 0.25
            }
            else if (beatmapObjectData is NoteData noteData)
            {
                ypos = beatmapObjectSpawnMovementData.LineYPosForLineLayer(noteData.startNoteLineLayer);
            }
            return ypos;
        }

        // poof random extension
        internal static float? ToNullableFloat(this object @this)
        {
            if (@this == null || @this == DBNull.Value) return null;
            return Convert.ToSingle(@this);
        }

        internal static void InitNoodlePatches()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                object[] noodleattributes = type.GetCustomAttributes(typeof(NoodlePatch), true);
                if (noodleattributes.Length > 0)
                {
                    Type declaringType = null;
                    string methodName = null;
                    foreach (NoodlePatch n in noodleattributes)
                    {
                        if (n.declaringType != null) declaringType = n.declaringType;
                        if (n.methodName != null) methodName = n.methodName;
                    }
                    if (declaringType == null || methodName == null) throw new ArgumentException("Type or Method Name not described");

                    MethodInfo original = declaringType.GetMethod(methodName);
                    MethodInfo prefix = AccessTools.Method(type, "Prefix");
                    MethodInfo postfix = AccessTools.Method(type, "Postfix");

                    NoodlePatches.Add(new NoodlePatchData(original, prefix, postfix));
                }
            }
        }

        private static List<NoodlePatchData> NoodlePatches = new List<NoodlePatchData>();

        internal static void ToggleNoodlePatches(bool value)
        {
            if (value && !Harmony.HasAnyPatches(Plugin.HARMONYID))
                NoodlePatches.ForEach(n => Plugin.harmony.Patch(n.originalMethod, n.prefix != null ? new HarmonyMethod(n.prefix) : null, n.postfix != null ? new HarmonyMethod(n.postfix) : null));
            else Plugin.harmony.UnpatchAll(Plugin.HARMONYID);
        }

        internal static void InitBeatmapObjectSpawnController(BeatmapObjectSpawnController bosc)
        {
            BeatmapObjectSpawnMovementData bosmd = Traverse.Create(bosc).Field("_beatmapObjectSpawnMovementData").GetValue<BeatmapObjectSpawnMovementData>();
            beatmapObjectSpawnMovementData = bosmd;
            var bosmdTraversal = new Traverse(bosmd);
            foreach (FieldInfo f in typeof(BeatmapObjectSpawnMovementDataVariables).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(n => n.Name != "beatmapObjectSpawnMovementData"))
            {
                f.SetValue(null, bosmdTraversal.Field(f.Name).GetValue());
            }
        }

        internal static class BeatmapObjectSpawnMovementDataVariables
        {
            internal static BeatmapObjectSpawnMovementData beatmapObjectSpawnMovementData;
#pragma warning disable 0649
            internal static float _topObstaclePosY;
            internal static float _jumpOffsetY;
            internal static float _verticalObstaclePosY;
            internal static float _jumpDistance;
            internal static float _noteJumpMovementSpeed;
            internal static float _noteLinesDistance;
            internal static float _baseLinesYPos;
            internal static Vector3 _moveStartPos;
            internal static Vector3 _moveEndPos;
            internal static Vector3 _jumpEndPos;
            internal static float _noteLinesCount;
            internal static Vector3 _rightVec;
#pragma warning restore 0649
        }
    }
}