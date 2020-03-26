﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        internal static Color? GetColorFromData(dynamic data, bool alpha = true, string member = "_color")
        {
            float[] color = ((List<object>)CustomJSONData.Trees.at(data, member))?.Select(n => Convert.ToSingle(n)).ToArray();
            if (color == null) return null;
            return new Color(color[0], color[1], color[2], color.Length > 3 && alpha ? color[3] : 1);
        }

        private static void setCapability(string capability, bool enabled = true)
        {
            if (enabled) SongCore.Collections.RegisterCapability(capability);
            else SongCore.Collections.DeregisterizeCapability(capability);
        }
    }
}