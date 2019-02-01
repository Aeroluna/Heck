using Chroma.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Chroma.Utils {

    public static class SceneUtils {
        
        private static int[] targetGameSceneIndexes = new int[] { 3 };

        public static bool IsTargetGameScene(Scene scene) {
            if (ChromaConfig.DebugMode) {
                ChromaLogger.Log(scene.name + " : " + scene.buildIndex);
                ChromaLogger.Log("gameplayMode string : |" + BaseGameMode.CurrentGameplayModeString + "|");
                ChromaLogger.Log("Base Game Mode : |" + BaseGameMode.CurrentBaseGameMode + "|");
            }
            return IsTargetGameScene(scene.buildIndex);
        }

        public static bool IsTargetGameScene(int a) {
            for (int i = 0; i < targetGameSceneIndexes.Length; i++) {
                if (targetGameSceneIndexes[i] == a) return true;
            }
            return false;
        }

    }

}
