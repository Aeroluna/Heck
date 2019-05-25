using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Colours {

    public abstract class ColourSelector {

        public float chance = 1f;
        public int priority = 0;

        public bool ChanceSuccess() {
            return UnityEngine.Random.value <= chance;
        }

        public Color GetColor() {
            return GetColor(Time.time);
        }

        public abstract Color GetColor(float time);

        /// <summary>
        /// If a selector is capable of returning multiple colours, it is dynamic
        /// </summary>
        /// <returns>True if the selector is dynamic</returns>
        public abstract bool IsDynamic();

        public virtual bool IsSmooth() {
            return false;
        }

        public static ColourSelector Deserialize(string json) {
            return JsonConvert.DeserializeObject<ColourSelector>(json);
        }

        public string Serialize() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }

}
