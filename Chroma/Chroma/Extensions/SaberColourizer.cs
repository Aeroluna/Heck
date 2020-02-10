using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Extensions
{
    internal class SaberColourizer
    {
        public bool warm;

        private SetSaberGlowColor[] glowColors;
        private MeshRenderer[] meshRenderers;
        private MaterialPropertyBlock[] blocks;
        private SetSaberGlowColor.PropertyTintColorPair[][] tintPairs;

        private List<Material> customMats = new List<Material>();

        public SaberColourizer(Saber saber)
        {
            warm = saber.saberType == Saber.SaberType.SaberA;

            glowColors = saber.GetComponentsInChildren<SetSaberGlowColor>();
            meshRenderers = new MeshRenderer[glowColors.Length];
            blocks = new MaterialPropertyBlock[glowColors.Length];
            tintPairs = new SetSaberGlowColor.PropertyTintColorPair[glowColors.Length][];
            for (int i = 0; i < glowColors.Length; i++)
            {
                meshRenderers[i] = glowColors[i].GetPrivateField<MeshRenderer>("_meshRenderer");

                blocks[i] = glowColors[i].GetPrivateField<MaterialPropertyBlock>("_materialPropertyBlock");
                if (blocks[i] == null)
                {
                    blocks[i] = new MaterialPropertyBlock();
                    glowColors[i].SetPrivateField("_materialPropertyBlock", blocks[i]);
                }
                tintPairs[i] = glowColors[i].GetPrivateField<SetSaberGlowColor.PropertyTintColorPair[]>("_propertyTintColorPairs");
                meshRenderers[i].SetPropertyBlock(blocks[i], 0);
            }

            //Custom sabers??
            Renderer[] renderers = saber.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                foreach (Material material in renderers[i].materials)
                {
                    if ((material.HasProperty("_Glow") && material.GetFloat("_Glow") > 0f) || (material.HasProperty("_Bloom") && material.GetFloat("_Bloom") > 0f))
                    {
                        customMats.Add(material);
                    }
                }
            }
        }

        public static SaberColourizer[] saberColourizers;

        public static void InitializeSabers(Saber[] sabers)
        {
            saberColourizers = new SaberColourizer[sabers.Length];
            for (int i = 0; i < sabers.Length; i++)
            {
                saberColourizers[i] = new SaberColourizer(sabers[i]);
            }
        }

        public void Colourize(Color color)
        {
            for (int i = 0; i < glowColors.Length; i++)
            {
                for (int j = 0; j < tintPairs[i].Length; j++)
                {
                    blocks[i].SetColor(tintPairs[i][j].property, color * tintPairs[i][j].tintColor);
                }

                meshRenderers[i].SetPropertyBlock(blocks[i], 0);
            }

            foreach (Material material in customMats)
            {
                material.SetColor("_Color", color);
            }
        }
    }
}