namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using System.Linq;
    using IPA.Utilities;
    using UnityEngine;

    public static class SaberColorizer
    {
        private static readonly HashSet<BSMColorManager> _bsmColorManagers = new HashSet<BSMColorManager>();

        internal static Color?[] SaberColorOverride { get; } = new Color?[2] { null, null };

        internal static SaberBurnMarkArea SaberBurnMarkArea { private get; set; }

        public static void Reset(this SaberType saberType)
        {
            BSMColorManager.Reset(saberType);
        }

        public static void ResetAllSaberColors()
        {
            BSMColorManager.Reset(SaberType.SaberA);
            BSMColorManager.Reset(SaberType.SaberB);
        }

        public static void SetSaberColor(this SaberType saberType, Color? color)
        {
            foreach (BSMColorManager bsmColorManager in BSMColorManager.GetBSMColorManager(saberType))
            {
                bsmColorManager.SetSaberColor(color);
            }
        }

        public static void SetAllSaberColors(Color? color0, Color? color1)
        {
            foreach (BSMColorManager bsmColorManager in BSMColorManager.GetBSMColorManager(SaberType.SaberA))
            {
                bsmColorManager.SetSaberColor(color0);
            }

            foreach (BSMColorManager bsmColorManager in BSMColorManager.GetBSMColorManager(SaberType.SaberB))
            {
                bsmColorManager.SetSaberColor(color1);
            }
        }

        internal static void ClearBSMColorManagers()
        {
            ResetAllSaberColors();
            _bsmColorManagers.Clear();
        }

        /*
         * BSM ColorSO holders
         */

        internal static void BSMStart(BasicSaberModelController bcm, SaberType saberType)
        {
            if (saberType == SaberType.SaberA || saberType == SaberType.SaberB)
            {
                BSMColorManager.CreateBSMColorManager(bcm, saberType);
            }
        }

        private class BSMColorManager
        {
            private static readonly FieldAccessor<BasicSaberModelController, Xft.XWeaponTrail>.Accessor _saberWeaponTrailAccessor = FieldAccessor<BasicSaberModelController, Xft.XWeaponTrail>.GetAccessor("_saberWeaponTrail");
            private static readonly FieldAccessor<BasicSaberModelController, BasicSaberModelController.InitData>.Accessor _initDataAccessor = FieldAccessor<BasicSaberModelController, BasicSaberModelController.InitData>.GetAccessor("_initData");
            private static readonly FieldAccessor<BasicSaberModelController, SetSaberGlowColor[]>.Accessor _setSaberGlowColorsAccessor = FieldAccessor<BasicSaberModelController, SetSaberGlowColor[]>.GetAccessor("_setSaberGlowColors");
            private static readonly FieldAccessor<BasicSaberModelController, SetSaberFakeGlowColor[]>.Accessor _SetSaberFakeGlowAccessor = FieldAccessor<BasicSaberModelController, SetSaberFakeGlowColor[]>.GetAccessor("_setSaberFakeGlowColors");
            private static readonly FieldAccessor<BasicSaberModelController, Light>.Accessor _lightAccessor = FieldAccessor<BasicSaberModelController, Light>.GetAccessor("_light");

            private static readonly FieldAccessor<SaberBurnMarkArea, LineRenderer[]>.Accessor _lineRenderersAccessor = FieldAccessor<SaberBurnMarkArea, LineRenderer[]>.GetAccessor("_lineRenderers");

            private readonly Xft.XWeaponTrail _saberWeaponTrail;
            private readonly Color _trailTintColor;
            private readonly SetSaberGlowColor[] _setSaberGlowColors;
            private readonly SetSaberFakeGlowColor[] _setSaberFakeGlowColors;
            private readonly Light _light;

            private readonly BasicSaberModelController _bsm;
            private readonly SaberType _saberType;

            private BSMColorManager(BasicSaberModelController bsm, SaberType saberType)
            {
                _bsm = bsm;
                _saberType = saberType;

                _saberWeaponTrail = _saberWeaponTrailAccessor(ref _bsm);
                _trailTintColor = _initDataAccessor(ref _bsm).trailTintColor;
                _setSaberGlowColors = _setSaberGlowColorsAccessor(ref _bsm);
                _setSaberFakeGlowColors = _SetSaberFakeGlowAccessor(ref _bsm);
                _light = _lightAccessor(ref _bsm);
            }

            internal static IEnumerable<BSMColorManager> GetBSMColorManager(SaberType saberType)
            {
                return _bsmColorManagers.Where(n => n._saberType == saberType);
            }

            internal static BSMColorManager CreateBSMColorManager(BasicSaberModelController bsm, SaberType saberType)
            {
                BSMColorManager bsmcm;
                bsmcm = new BSMColorManager(bsm, saberType);
                _bsmColorManagers.Add(bsmcm);
                return bsmcm;
            }

            internal static void Reset(SaberType saberType)
            {
                SaberColorOverride[(int)saberType] = null;
            }

            internal void SetSaberColor(Color? colorNullable)
            {
                if (colorNullable.HasValue)
                {
                    Color color = colorNullable.Value;

                    SaberColorOverride[(int)_saberType] = color;

                    _saberWeaponTrail.color = (color * _trailTintColor).linear;
                    _light.color = color;

                    foreach (SetSaberGlowColor setSaberGlowColor in _setSaberGlowColors)
                    {
                        setSaberGlowColor.SetColors();
                    }

                    foreach (SetSaberFakeGlowColor setSaberFakeGlowColor in _setSaberFakeGlowColors)
                    {
                        setSaberFakeGlowColor.SetColors();
                    }

                    SaberBurnMarkArea saberBurnMarkArea = SaberBurnMarkArea;
                    LineRenderer[] lineRenderers = _lineRenderersAccessor(ref saberBurnMarkArea);
                    lineRenderers[(int)_saberType].startColor = color;
                    lineRenderers[(int)_saberType].endColor = color;

                    // Custom sabers suck
                    IEnumerable<Renderer> renderers = _bsm.transform.parent.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            if (material.HasProperty("_Color"))
                            {
                                if (material.HasProperty("_CustomColors"))
                                {
                                    if (material.GetFloat("_CustomColors") > 0)
                                    {
                                        material.SetColor("_Color", color);
                                    }
                                }
                                else if ((material.HasProperty("_Glow") && material.GetFloat("_Glow") > 0) || (material.HasProperty("_Bloom") && material.GetFloat("_Bloom") > 0))
                                {
                                    material.SetColor("_Color", color);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
