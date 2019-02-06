using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Extensions {

    public static class TubeBloomPrePassLightExtensions {

        public static void Reset(this TubeBloomPrePassLight light) {
            AmbientLightColourHolder holder = AmbientLightColourHolder.GetTubeLightColourHolder(light);
            holder.ResetColour();
        }

        public static void ApplyColour(this TubeBloomPrePassLight light, Color color) {
            AmbientLightColourHolder holder = AmbientLightColourHolder.GetTubeLightColourHolder(light);
            holder.ApplyColour(color);
        }

        private class AmbientLightColourHolder {

            public static Dictionary<TubeBloomPrePassLight, AmbientLightColourHolder> tubeLightOriginals = new Dictionary<TubeBloomPrePassLight, AmbientLightColourHolder>();

            public static AmbientLightColourHolder GetTubeLightColourHolder(TubeBloomPrePassLight light) {
                if (tubeLightOriginals.TryGetValue(light, out AmbientLightColourHolder holder)) return holder;
                else return new AmbientLightColourHolder(light);
            }

            TubeBloomPrePassLight light;

            public Color fieldColour;
            public Color color;

            public Dictionary<Renderer, Color> rendColors = new Dictionary<Renderer, Color>();

            public AmbientLightColourHolder(TubeBloomPrePassLight light) {
                this.light = light;

                fieldColour = light.GetField<Color>("_color");
                color = light.color;

                Renderer[] rends = light.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in rends) {
                    if (rend.materials.Length > 0) {
                        if (rend.material.shader.name == "Custom/ParametricBox" || rend.material.shader.name == "Custom/ParametricBoxOpaque") {
                            rendColors.Add(rend, rend.material.GetColor("_Color"));
                        }
                    }
                }

                tubeLightOriginals.Add(light, this);
            }

            public void ResetColour() {
                light.SetField("_color", fieldColour);
                light.color = color;
                foreach (KeyValuePair<Renderer, Color> kv in rendColors) {
                    kv.Key.material.SetColor("_Color", kv.Value);
                }
            }

            public void ApplyColour(Color c) {
                light.SetField("_color", c);
                light.color = c;
                foreach (Renderer rend in rendColors.Keys) {
                    rend.material.SetColor("_Color", c);
                }
            }

        }

    }
}
