using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap {
    
    public class CustomBeatmapBomb : CustomBeatmapObject {

        private NoteData _note;

        public BeatmapObjectData Note {
            get { return _note; }
        }

        public CustomBeatmapBomb(NoteData note) : base(note) {
            this._note = note;
        }

    }

}
