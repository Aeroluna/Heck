namespace Chroma.HarmonyPatches
{
    using Chroma.Utils;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Init")]
    internal class NoteControllerInit
    {
#pragma warning disable SA1313
        private static void Prefix(NoteController __instance, NoteData noteData)
#pragma warning restore SA1313
        {
            // They said it couldn't be done, they called me a madman
            if (noteData.noteType == NoteType.Bomb)
            {
                Color? c = null;

                // CustomJSONData _customData individual scale override
                if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered)
                {
                    dynamic dynData = customData.customData;

                    c = ChromaUtils.GetColorFromData(dynData, false) ?? c;
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
