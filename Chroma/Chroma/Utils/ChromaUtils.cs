using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Utils
{
    public static class ChromaUtils
    {
        public static bool IsModInstalled(string mod) {
            return IPA.Loader.PluginManager.AllPlugins.Any(x => x.Metadata.Id == mod);
        }

        public static void SetSongCoreCapability(string capability, bool enabled = true) {
            // Gotta check for SongCore first
            if (!IsModInstalled("SongCore")) return;
            SetCapability(capability, enabled);
        }

        private static void SetCapability(string capability, bool enabled = true)
        {
            if (enabled) SongCore.Collections.RegisterCapability(capability);
            else SongCore.Collections.DeregisterizeCapability(capability);
        }
    }
}
