using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Colours {

    public class SimpleColourSelector : ColourSelector {

        public Color color;

        public SimpleColourSelector(Color color) {
            this.color = color;
        }

        public override Color GetColor(float time) {
            return color;
        }

        public override bool IsDynamic() {
            return false;
        }

    }

}
