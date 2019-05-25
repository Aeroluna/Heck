using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Colours {

    public class PaletteColourSelector : ColourSelector {

        public Color[] palette;
        public bool smooth;

        public PaletteColourSelector(Color[] palette, bool smooth) {
            this.palette = palette;
            this.smooth = smooth;
        }

        public override Color GetColor(float time) {
            return GetRandomFromArray(palette, time); 
        }

        public override bool IsDynamic() {
            return palette.Length > 1;
        }

        public override bool IsSmooth() {
            return IsDynamic();
        }

        public static Color GetRandomFromArray(Color[] colors, float time, float seedMult = 8) {
            System.Random rand = new System.Random(Mathf.FloorToInt(seedMult * time));
            return colors[rand.Next(0, colors.Length)];
        }
    }

}
