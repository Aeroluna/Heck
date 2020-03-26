using Chroma.Extensions;
using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using System;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaSaberColourEvent
    {
        internal static void Callback(CustomEventData eventData)
        {
            try
            {
                // Pull and assign all custom data
                dynamic dynData = eventData.data;
                Color? aColor = ChromaUtils.GetColorFromData(dynData, false, "_leftColor");
                Color? bColor = ChromaUtils.GetColorFromData(dynData, false, "_rightColor");

                foreach (SaberColourizer saber in SaberColourizer.saberColourizers)
                {
                    if (saber.warm ? aColor.HasValue : bColor.HasValue)
                    {
                        saber.Colourize(saber.warm ? aColor.Value : bColor.Value);
                    }
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log("INVALID CUSTOM EVENT", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }
        }
    }
}