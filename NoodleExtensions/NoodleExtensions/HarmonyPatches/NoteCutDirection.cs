using Harmony;
using System;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteCutDirectionExtensions), new Type[] { typeof(NoteCutDirection) })]
    [HarmonyPatch("Rotation", MethodType.Normal)]
    internal class cyanisadumbstinkyfurry
    {
        public static void Postfix(NoteData notedata)
        {
            Logger.Log("Cyan is a furry");
        }
    }
}