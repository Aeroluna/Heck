using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Colours {

    public class RandomColourSelector : ColourSelector {

        public override Color GetColor(float time) {
            //return Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f); //UnityEngine.Random.ColorHSV().ColorWithAlpha(1f);
            System.Random rand = new System.Random(Mathf.FloorToInt(4.41f * time));
            return Color.HSVToRGB((float)rand.NextDouble(), 1f, 1f); //UnityEngine.Random.ColorHSV().ColorWithAlpha(1f);
        }

        public override bool IsDynamic() {
            return true;
        }

    }

}
