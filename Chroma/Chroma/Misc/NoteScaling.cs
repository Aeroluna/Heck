using Chroma.Beatmap.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Misc {

    public static class NoteScaling {

        public delegate void HandleNoteScalingDelegate(ref NoteData note, ref float scale);
        public static event HandleNoteScalingDelegate HandleNoteScalingEvent;

        public static float GetNoteScale(NoteData note) {
            float s = ChromaNoteScaleEvent.GetScale(note.time);
            HandleNoteScalingEvent?.Invoke(ref note, ref s);
            return s;
        }

    }

}
