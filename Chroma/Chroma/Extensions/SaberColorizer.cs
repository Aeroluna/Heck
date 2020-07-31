namespace Chroma.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using IPA.Utilities;
    using UnityEngine;

    internal class SaberColorizer
    {
        private static readonly FieldAccessor<BasicSaberModelController, BasicSaberModelController.InitData>.Accessor _basicSaberModelControllerAccessor = FieldAccessor<BasicSaberModelController, BasicSaberModelController.InitData>.GetAccessor("_initData");

        private SetSaberGlowColor[] _glowColors;
        private MeshRenderer[] _meshRenderers;
        private MaterialPropertyBlock[] _blocks;
        private SetSaberGlowColor.PropertyTintColorPair[][] _tintPairs;
        private Xft.XWeaponTrail _weaponTrail;
        private Color _trailTintColor;

        private List<Material> _customMats = new List<Material>();

        private SaberColorizer(Saber saber)
        {
            SaberType = saber.saberType;

            _glowColors = saber.GetComponentsInChildren<SetSaberGlowColor>();
            _meshRenderers = new MeshRenderer[_glowColors.Length];
            _blocks = new MaterialPropertyBlock[_glowColors.Length];
            _tintPairs = new SetSaberGlowColor.PropertyTintColorPair[_glowColors.Length][];
            for (int i = 0; i < _glowColors.Length; i++)
            {
                _meshRenderers[i] = _glowColors[i].GetField<MeshRenderer, SetSaberGlowColor>("_meshRenderer");

                _blocks[i] = _glowColors[i].GetField<MaterialPropertyBlock, SetSaberGlowColor>("_materialPropertyBlock");
                if (_blocks[i] == null)
                {
                    _blocks[i] = new MaterialPropertyBlock();
                    _glowColors[i].SetField("_materialPropertyBlock", _blocks[i]);
                }

                _tintPairs[i] = _glowColors[i].GetField<SetSaberGlowColor.PropertyTintColorPair[], SetSaberGlowColor>("_propertyTintColorPairs");
                _meshRenderers[i].SetPropertyBlock(_blocks[i], 0);
            }

            // Custom sabers??
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
                                _customMats.Add(material);
                            }
                        }
                        else if ((material.HasProperty("_Glow") && material.GetFloat("_Glow") > 0) || (material.HasProperty("_Bloom") && material.GetFloat("_Bloom") > 0))
                        {
                            _customMats.Add(material);
                        }
                    }
                }
            }

            _weaponTrail = saber.gameObject.GetComponentInChildren<Xft.XWeaponTrail>();
            BasicSaberModelController basicSaberModelController = saber.gameObject.GetComponentInChildren<BasicSaberModelController>();
            _trailTintColor = _basicSaberModelControllerAccessor(ref basicSaberModelController).trailTintColor;
        }

        internal static SaberBurnMarkArea SaberBurnMarkArea { get; set; } = null;

        internal static Color? CurrentAColor { get; set; } = null;

        internal static Color? CurrentBColor { get; set; } = null;

        internal static SaberColorizer[] SaberColorizers { get; private set; } = new SaberColorizer[0];

        internal SaberType SaberType { get; set; }

        internal static void InitializeSabers(Saber[] sabers)
        {
            SaberColorizers = sabers.Select(n => new SaberColorizer(n)).ToArray();
        }

        internal void Colorize(Color color)
        {
            if (SaberType == SaberType.SaberA)
            {
                CurrentAColor = color;
            }
            else
            {
                CurrentBColor = color;
            }

            for (int i = 0; i < _glowColors.Length; i++)
            {
                for (int j = 0; j < _tintPairs[i].Length; j++)
                {
                    _blocks[i].SetColor(_tintPairs[i][j].property, color * _tintPairs[i][j].tintColor);
                }

                _meshRenderers[i].SetPropertyBlock(_blocks[i], 0);
            }

            foreach (Material material in _customMats)
            {
                material.SetColor("_Color", color);
            }

            if (SaberBurnMarkArea != null)
            {
                LineRenderer[] lineRenderers = SaberBurnMarkArea.GetField<LineRenderer[], SaberBurnMarkArea>("_lineRenderers");
                lineRenderers[(int)SaberType].startColor = color;
                lineRenderers[(int)SaberType].endColor = color;
            }

            _weaponTrail.color = (color * _trailTintColor).linear;
        }
    }
}
