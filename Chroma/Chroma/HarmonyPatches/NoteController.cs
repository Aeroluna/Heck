using Chroma.Events;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Init")]
    internal class NoteControllerInit
    {
        private static void Prefix(NoteController __instance, NoteData noteData)
        {
            // They said it couldn't be done, they called me a madman
            if (noteData.noteType == NoteType.Bomb)
            {
                if (VFX.TechnicolourController.Instantiated())
                    VFX.TechnicolourController.Instance._bombControllers.Add(__instance);

                Color? c = null;

                // Technicolour
                if (ColourManager.TechnicolourBombs && ChromaConfig.TechnicolourBombsStyle != ColourManager.TechnicolourStyle.GRADIENT)
                {
                    c = ColourManager.GetTechnicolour(true, Time.time + __instance.GetInstanceID(), ChromaConfig.TechnicolourBombsStyle);
                }

                // NoteScales
                if (ChromaBombColourEvent.CustomBombColours.Count > 0)
                {
                    foreach (KeyValuePair<float, Color> d in ChromaBombColourEvent.CustomBombColours)
                    {
                        if (d.Key <= noteData.time) c = d.Value;
                    }
                }

                // CustomJSONData _customData individual scale override
                try
                {
                    if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered)
                    {
                        dynamic dynData = customData.customData;

                        List<object> color = Trees.at(dynData, "_color");
                        if (color != null)
                        {
                            float r = Convert.ToSingle(color[0]);
                            float g = Convert.ToSingle(color[1]);
                            float b = Convert.ToSingle(color[2]);

                            c = new Color(r, g, b);
                        }
                    }
                }
                catch (Exception e)
                {
                    ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                    ChromaLogger.Log(e);
                }

                if (c.HasValue)
                {
                    Material mat = __instance.noteTransform.gameObject.GetComponent<Renderer>().material;
                    mat.SetColor("_SimpleColor", c.Value);
                }
            }
        }
    }
}