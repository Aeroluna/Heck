using IPA.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.Extensions
{
    internal class SaberColourizer
    {
        internal bool warm;

        private SetSaberGlowColor[] glowColors;
        private MeshRenderer[] meshRenderers;
        private MaterialPropertyBlock[] blocks;
        private SetSaberGlowColor.PropertyTintColorPair[][] tintPairs;
        private Xft.XWeaponTrail weaponTrail;

        private List<Material> customMats = new List<Material>();

        internal static SaberBurnMarkArea saberBurnMarkArea = null;

        internal static Color? currentAColor = null;
        internal static Color? currentBColor = null;

        private SaberColourizer(Saber saber)
        {
            warm = saber.saberType == SaberType.SaberA;

            glowColors = saber.GetComponentsInChildren<SetSaberGlowColor>();
            meshRenderers = new MeshRenderer[glowColors.Length];
            blocks = new MaterialPropertyBlock[glowColors.Length];
            tintPairs = new SetSaberGlowColor.PropertyTintColorPair[glowColors.Length][];
            for (int i = 0; i < glowColors.Length; i++)
            {
                meshRenderers[i] = glowColors[i].GetField<MeshRenderer, SetSaberGlowColor>("_meshRenderer");

                blocks[i] = glowColors[i].GetField<MaterialPropertyBlock, SetSaberGlowColor>("_materialPropertyBlock");
                if (blocks[i] == null)
                {
                    blocks[i] = new MaterialPropertyBlock();
                    glowColors[i].SetField("_materialPropertyBlock", blocks[i]);
                }
                tintPairs[i] = glowColors[i].GetField<SetSaberGlowColor.PropertyTintColorPair[], SetSaberGlowColor>("_propertyTintColorPairs");
                meshRenderers[i].SetPropertyBlock(blocks[i], 0);
            }

            //Custom sabers??
            Renderer[] renderers = saber.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                foreach (Material material in renderers[i].materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        if (material.HasProperty("_CustomColors"))
                        {
                            if (material.GetFloat("_CustomColors") > 0)
                            {
                                customMats.Add(material);
                            }
                        }
                        else if ((material.HasProperty("_Glow") && material.GetFloat("_Glow") > 0) || (material.HasProperty("_Bloom") && material.GetFloat("_Bloom") > 0))
                        {
                            customMats.Add(material);
                        }
                    }
                }
            }

            weaponTrail = saber.gameObject.GetComponentInChildren<Xft.XWeaponTrail>();
        }

        internal static SaberColourizer[] saberColourizers { get; private set; } = new SaberColourizer[0];

        internal static void InitializeSabers(Saber[] sabers)
        {
            saberColourizers = sabers.Select(n => new SaberColourizer(n)).ToArray();
        }

        internal void Colourize(Color color)
        {
            if (warm) currentAColor = color;
            else currentBColor = color;

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

            if (saberBurnMarkArea != null)
            {
                LineRenderer[] _lineRenderers = saberBurnMarkArea.GetField<LineRenderer[], SaberBurnMarkArea>("_lineRenderers");
                _lineRenderers[warm ? 0 : 1].startColor = color;
                _lineRenderers[warm ? 0 : 1].endColor = color;
            }

            weaponTrail.color = color;
        }
    }
}