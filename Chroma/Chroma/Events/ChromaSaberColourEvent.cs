using Chroma.Extensions;
using Chroma.Utils;
using CustomJSONData;
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
                int id = (int)Trees.at(dynData, "_id");
                Color color = ChromaUtils.GetColorFromData(dynData, false);

                foreach (SaberColourizer saber in SaberColourizer.saberColourizers)
                {
                    if (saber.warm == (id == 0))
                    {
                        saber.Colourize(color);
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