namespace Chroma
{
    using System.Collections.Generic;
    using IPA.Utilities;
    using UnityEngine;

    internal static class ChromaColorManager
    {
        private static Color?[] _noteTypeColorOverrides = new Color?[] { null, null };

        private static Dictionary<BeatmapEventType, LightSwitchEventEffect> _lightSwitchDictionary;

        internal static Dictionary<BeatmapEventType, LightSwitchEventEffect> LightSwitchDictionary
        {
            get
            {
                if (_lightSwitchDictionary == null)
                {
                    _lightSwitchDictionary = new Dictionary<BeatmapEventType, LightSwitchEventEffect>();
                    foreach (LightSwitchEventEffect l in Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>())
                    {
                        _lightSwitchDictionary.Add(l.GetField<BeatmapEventType, LightSwitchEventEffect>("_event"), l);
                    }
                }

                return _lightSwitchDictionary;
            }
        }

        internal static Color? GetNoteTypeColorOverride(NoteType noteType)
        {
            return _noteTypeColorOverrides[noteType == NoteType.NoteA ? 0 : 1];
        }

        internal static void SetNoteTypeColorOverride(NoteType noteType, Color color)
        {
            _noteTypeColorOverrides[noteType == NoteType.NoteA ? 0 : 1] = color;
        }

        internal static void RemoveNoteTypeColorOverride(NoteType noteType)
        {
            _noteTypeColorOverrides[noteType == NoteType.NoteA ? 0 : 1] = null;
        }

        internal static void ClearLightSwitches()
        {
            _lightSwitchDictionary = null;
        }
    }
}
