using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Utils {

    public static class BaseGameMode {

        internal static bool isPartyMode = false;
        internal static string currentGameplayModeString = "Standard";

        /// <summary>
        /// Returns the stupid string thing
        /// </summary>
        public static string CurrentGameplayModeString {
            get { return currentGameplayModeString; }
        }

        /// <summary>
        /// Returns the currently selected BaseGameModeType
        /// </summary>
        public static BaseGameModeType CurrentBaseGameMode {
            get {
                return GetBaseGameModeType(currentGameplayModeString);
            }
        }

        /// <summary>
        /// Returns the BaseGameModeType enum from string
        /// </summary>
        /// <param name="gameplayModeString">Beat Saber uses a fucking string for this for some reason</param>
        /// <returns>Chroma's BaseGameModeType enum, AS IT SHOULD BE</returns>
        public static BaseGameModeType GetBaseGameModeType(string gameplayModeString) {
            switch (gameplayModeString) {
                case "Standard": return BaseGameModeType.SoloStandard;
                case "One Saber": return BaseGameModeType.SoloOneSaber;
                case "No Arrows": return BaseGameModeType.SoloNoArrows;
            }
            return BaseGameModeType.Unknown;
        }

        /// <summary>
        /// Returns whether the game is currently in party mode or not.
        /// Because things need to be harder for some reason.
        /// </summary>
        public static bool PartyMode {
            get { return isPartyMode; }
        }



        #region GameplayModeTracking
        /*
         * Gameplay Mode Tracking
         */

        private static BeatmapCharacteristicSelectionViewController _characteristicViewController;
        private static PartyFreePlayFlowCoordinator _partyFlowCoordinator;
        private static SoloFreePlayFlowCoordinator _soloFlowCoordinator;
        private static PracticeViewController _practiceViewController;
        private static StandardLevelDetailViewController _soloDetailView;

        public static void InitializeCoordinators() {

            if (_characteristicViewController == null) {
                _characteristicViewController = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSelectionViewController>().FirstOrDefault();
                if (_characteristicViewController == null) return;

                _characteristicViewController.didSelectBeatmapCharacteristicEvent += _characteristicViewController_didSelectBeatmapCharacteristicEvent;
            }

            if (_soloFlowCoordinator == null) {
                _soloFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().FirstOrDefault();
                if (_soloFlowCoordinator == null) return;
                _soloDetailView = _soloFlowCoordinator.GetField<StandardLevelDetailViewController>("_levelDetailViewController");
                _practiceViewController = _soloFlowCoordinator.GetField<PracticeViewController>("_practiceViewController");
                if (_soloDetailView != null) {
                    _soloDetailView.didPressPlayButtonEvent += _soloDetailView_didPressPlayButtonEvent;
                } else {
                    ChromaLogger.Log("Detail View Null", ChromaLogger.Level.WARNING);
                }
                if (_practiceViewController != null) {
                    _practiceViewController.didPressPlayButtonEvent += _practiceViewController_didPressPlayButtonEvent;
                } else {
                    ChromaLogger.Log("Practice View Null", ChromaLogger.Level.WARNING);
                }

            }

            if (_partyFlowCoordinator == null) {
                _partyFlowCoordinator = Resources.FindObjectsOfTypeAll<PartyFreePlayFlowCoordinator>().FirstOrDefault();
            }
        }

        private static void _practiceViewController_didPressPlayButtonEvent() {
            ChromaLogger.Log("Play Button Press");
            isPartyMode = _partyFlowCoordinator.isActivated;
            ChromaLogger.Log("Party: " + isPartyMode);
        }

        private static void _soloDetailView_didPressPlayButtonEvent(StandardLevelDetailViewController obj) {
            ChromaLogger.Log("Play Button Press ");
            isPartyMode = _partyFlowCoordinator.isActivated;
            ChromaLogger.Log("Party: " + isPartyMode);
        }

        private static void _characteristicViewController_didSelectBeatmapCharacteristicEvent(BeatmapCharacteristicSelectionViewController arg1, BeatmapCharacteristicSO arg2) {
            currentGameplayModeString = arg2.characteristicName;
            ChromaLogger.Log("Gameplaymode Change : |" + currentGameplayModeString + "|");
        }
        #endregion

    }

}
