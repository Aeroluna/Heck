using Chroma.Beatmap.Events;
using Chroma.Misc;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Init")]
    class NoteControllerInit {

        static void Postfix(NoteController __instance, NoteData ____noteData) {
            __instance.noteTransform.localScale = Vector3.one /*__instance.noteTransform.localScale*/ * NoteScaling.GetNoteScale(____noteData); //ChromaNoteScaleEvent.GetScale(____noteData.time);
        }

    }

}
