using System.Linq;

namespace Chroma.Utils
{
    internal static class ChromaUtils
    {
        internal static bool IsModInstalled(string mod)
        {
            return IPA.Loader.PluginManager.AllPlugins.Any(x => x.Metadata.Id == mod);
        }

        internal static void SetSongCoreCapability(string capability, bool enabled = true)
        {
            // Gotta check for SongCore first
            if (!IsModInstalled("SongCore")) return;
            setCapability(capability, enabled);
        }

        internal static bool CheckLightingEventRequirement()
        {
            if (!IsModInstalled("SongCore")) return Settings.ChromaConfig.CustomColourEventsEnabled;
            return checkLightingEventActivation() && Settings.ChromaConfig.CustomColourEventsEnabled;
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
            return ((songData?.additionalDifficultyData._suggestions.Contains(Plugin.REQUIREMENT_NAME) ?? false) ||
                songData.additionalDifficultyData._requirements.Contains(Plugin.REQUIREMENT_NAME));
        }
    }
}