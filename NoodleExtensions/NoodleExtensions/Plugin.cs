using BS_Utils.Utilities;
using HarmonyLib;
using IPA;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {
        // All objects
        internal const string POSITION = "_position";

        internal const string ROTATION = "_rotation"; // Rotation events included

        // Wall exclusives
        internal const string LOCALROTATION = "_localRotation";

        internal const string SCALE = "_scale";

        // Note exclusives
        internal const string CUTDIRECTION = "_cutDirection";

        internal const string FLIP = "_flip";

        internal static bool MappingExtensionsActive = false;
        internal static bool NoodleExtensionsActive = false;

        internal static Dictionary<NoteData, float> flipLineIndexes = new Dictionary<NoteData, float>();

        // Used by harmony patches
        internal static BeatmapObjectSpawnController beatmapObjectSpawnController
        {
            get
            {
                if (_bosc == null) _bosc = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
                return _bosc;
            }
        }

        private static BeatmapObjectSpawnController _bosc;

        internal static BeatmapObjectSpawnMovementData beatmapObjectSpawnMovementData
        {
            get
            {
                if (_bosmd == null) _bosmd = beatmapObjectSpawnController.GetPrivateField< BeatmapObjectSpawnMovementData>("_beatmapObjectSpawnMovementData");
                return _bosmd;
            }
        }

        private static BeatmapObjectSpawnMovementData _bosmd;

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            Logger.logger = pluginLogger;
        }

        [OnStart]
        public void OnApplicationStart()
        {
            SongCore.Collections.RegisterCapability("Noodle Extensions");
            var harmony = new Harmony("com.noodle.BeatSaber.NoodleExtensions");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "GameCore")
            {
                MappingExtensionsActive = CheckRequirements("Mapping Extensions");
                NoodleExtensionsActive = CheckRequirements("Noodle Extensions");
            }
        }

        private bool CheckRequirements(string capability)
        {
            var diff = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;
            var songData = SongCore.Collections.RetrieveDifficultyData(diff);
            return songData?.additionalDifficultyData._requirements.Contains(capability) ?? false;
        }

        internal static Vector3 GetNoteOffset(BeatmapObjectData beatmapObjectData, float? _startRow, float? _startHeight)
        {
            float _noteLinesCount = beatmapObjectSpawnController.GetPrivateField<float>("_noteLinesCount");
            float _noteLinesDistance = beatmapObjectSpawnMovementData.noteLinesDistance;

            float distance = -(_noteLinesCount - 1) * 0.5f + (_startRow.HasValue ? _noteLinesCount / 2 : 0); // Add last part to simulate https://github.com/spookyGh0st/beatwalls/#wall
            float lineIndex = _startRow.GetValueOrDefault(beatmapObjectData.lineIndex);
            distance = (distance + lineIndex) * _noteLinesDistance;

            return beatmapObjectSpawnController.transform.right * distance
                + new Vector3(0, LineYPosForLineLayer(beatmapObjectData, _startHeight), 0);
        }

        internal static float LineYPosForLineLayer(BeatmapObjectData beatmapObjectData, float? height)
        {
            float _noteLinesDistance = beatmapObjectSpawnMovementData.noteLinesDistance;
            float _baseLinesYPos = beatmapObjectSpawnController.GetPrivateField<float>("_baseLinesYPos");
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
    }
}