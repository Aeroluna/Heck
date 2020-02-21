using System.Linq;
using System.Text;

namespace Chroma.Utils
{
    public static class ChromaUtils
    {
        public static bool IsModInstalled(string mod)
        {
            return IPA.Loader.PluginManager.AllPlugins.Any(x => x.Metadata.Id == mod);
        }

        public static void SetSongCoreCapability(string capability, bool enabled = true)
        {
            // Gotta check for SongCore first
            if (!IsModInstalled("SongCore")) return;
            setCapability(capability, enabled);
        }

        public static bool CheckLightingEventRequirement()
        {
            if (!IsModInstalled("SongCore")) return Settings.ChromaConfig.CustomColourEventsEnabled;
            return checkLightingEventActivation() && Settings.ChromaConfig.CustomColourEventsEnabled;
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static void setCapability(string capability, bool enabled = true)
        {
            if (enabled) SongCore.Collections.RegisterCapability(capability);
            else SongCore.Collections.DeregisterizeCapability(capability);
        }

        private static bool checkLightingEventActivation()
        {
            var diff = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;
            var songData = SongCore.Collections.RetrieveDifficultyData(diff);
            return (songData?.additionalDifficultyData._suggestions.Contains("Chroma Lighting Events") ?? false ||
                songData.additionalDifficultyData._requirements.Contains("Chroma Lighting Events"));
        }
    }
}