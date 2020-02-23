using BS_Utils.Utilities;
using Harmony;
using IPA;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    public class Plugin : IBeatSaberPlugin
    {
        public const bool DebugMode = true;

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

        public void Init(object thisIsNull, IPALogger pluginLogger)
        {
            Logger.logger = pluginLogger;
        }

        public void OnApplicationStart()
        {
            SongCore.Collections.RegisterCapability("Noodle Extensions");
            var harmony = HarmonyInstance.Create("com.noodle.BeatSaber.NoodleExtensions");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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

        internal static Vector3 GetNoteOffset(BeatmapObjectData beatMapObjectData, float? _startRow, float? _startHeight)
        {
            float _noteLinesCount = beatmapObjectSpawnController.GetField<float>("_noteLinesCount");
            float _noteLinesDistance = beatmapObjectSpawnController.GetField<float>("_noteLinesDistance");

            float distance = -(_noteLinesCount - 1) * 0.5f + (_startRow.HasValue ? _noteLinesCount / 2 : 0); // Add last part to simulate https://github.com/spookyGh0st/beatwalls/#wall
            distance = (distance + _startRow.GetValueOrDefault(beatMapObjectData.lineIndex)) * _noteLinesDistance;

            float ypos = 0;
            if (_startHeight.HasValue)
            {
                ypos = _startHeight.Value * _noteLinesDistance;
            }
            else if (beatMapObjectData is NoteData noteData)
            {
                ypos = beatmapObjectSpawnController.LineYPosForLineLayer(noteData.startNoteLineLayer);
            }

            return beatmapObjectSpawnController.transform.right * distance
                + new Vector3(0, ypos, 0);
        }

        #region Unused

        public void OnApplicationQuit()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void OnUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        #endregion Unused
    }
}