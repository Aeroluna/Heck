using Chroma.Events;
using Chroma.Settings;
using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
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

                // BombColours
                List<TimedColor> colors = ChromaBombColourEvent.BombColours.Where(n => n.time <= noteData.time).ToList();
                if (colors.Count > 0) c = colors.Last().color;

                // CustomJSONData _customData individual scale override
                try
                {
                    if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered)
                    {
                        dynamic dynData = customData.customData;

                        c = ChromaUtils.GetColorFromData(dynData, false) ?? c;
                    }
                }
                catch (Exception e)
                {
                    Logger.Log("INVALID _customData", Logger.Level.WARNING);
                    Logger.Log(e);
                }

                if (!c.HasValue)
                {
                    // I shouldn't hard code this... but i can't be bothered to not atm
                    c = new Color(0.251f, 0.251f, 0.251f, 0);
                }

                Material mat = __instance.noteTransform.gameObject.GetComponent<Renderer>().material;
                mat.SetColor("_SimpleColor", c.Value);
            }
        }
    }
}