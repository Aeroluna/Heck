using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.Beatmap {

    public abstract class CustomBeatmapObject {

        private BeatmapObjectData _data;

        public BeatmapObjectData Data {
            get { return _data; }
        }

        public CustomBeatmapObject(BeatmapObjectData data) {
            this._data = data;
        }

    }

}
