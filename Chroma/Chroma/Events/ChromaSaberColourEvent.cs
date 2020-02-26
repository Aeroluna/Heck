using Chroma.Extensions;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaSaberColourEvent
    {
        public static void Callback(CustomEventData eventData)
        {
            try
            {
                // Pull and assign all custom data
                dynamic dynData = eventData.data;
                List<object> asaber = Trees.at(dynData, "_leftColor");
                List<object> bsaber = Trees.at(dynData, "_rightColor");

                Color? aColor = null;
                if (asaber != null)
                {
                    float aR = Convert.ToSingle(asaber[0]);
                    float aG = Convert.ToSingle(asaber[1]);
                    float aB = Convert.ToSingle(asaber[2]);
                    aColor = new Color(aR, aG, aB);
                }
                Color? bColor = null;
                if (bsaber != null)
                {
                    float bR = Convert.ToSingle(bsaber[0]);
                    float bG = Convert.ToSingle(bsaber[1]);
                    float bB = Convert.ToSingle(bsaber[2]);
                    bColor = new Color(bR, bG, bB);
                }
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