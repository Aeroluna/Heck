using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using static NoodleExtensions.NoodleController.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions
{
    internal static class NoodleController
    {
        internal static bool MappingExtensionsActive = false;
        internal static bool NoodleExtensionsActive = false;

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

        internal static void InitBeatmapObjectSpawnController(BeatmapObjectSpawnController bosc)
        {
            BeatmapObjectSpawnMovementData bosmd = Traverse.Create(bosc).Field("_beatmapObjectSpawnMovementData").GetValue<BeatmapObjectSpawnMovementData>();
            //Logger.Log("BeatmapObjectSpawnMovementData Found");
            beatmapObjectSpawnMovementData = bosmd;
            var bosmdTraversal = new Traverse(bosmd);
            foreach (FieldInfo f in typeof(BeatmapObjectSpawnMovementDataVariables).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(n => n.Name != "beatmapObjectSpawnMovementData"))
            {
                //Logger.Log(f.Name);
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
