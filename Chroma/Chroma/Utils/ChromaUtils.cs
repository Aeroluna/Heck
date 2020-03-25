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

        private static void setCapability(string capability, bool enabled = true)
        {
            if (enabled) SongCore.Collections.RegisterCapability(capability);
            else SongCore.Collections.DeregisterizeCapability(capability);
        }
    }
}