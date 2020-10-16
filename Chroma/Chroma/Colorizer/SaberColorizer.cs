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

        internal static void BSMStart(SaberModelController bcm, SaberType saberType)
        {
            if (saberType == SaberType.SaberA || saberType == SaberType.SaberB)
            {
                BSMColorManager.CreateBSMColorManager(bcm, saberType);
            }
        }

        private class BSMColorManager
        {
            private static readonly FieldAccessor<SaberModelController, SaberTrail>.Accessor _saberWeaponTrailAccessor = FieldAccessor<SaberModelController, SaberTrail>.GetAccessor("_saberTrail");
            private static readonly FieldAccessor<SaberModelController, SaberModelController.InitData>.Accessor _initDataAccessor = FieldAccessor<SaberModelController, SaberModelController.InitData>.GetAccessor("_initData");
            private static readonly FieldAccessor<SaberModelController, SetSaberGlowColor[]>.Accessor _setSaberGlowColorsAccessor = FieldAccessor<SaberModelController, SetSaberGlowColor[]>.GetAccessor("_setSaberGlowColors");
            private static readonly FieldAccessor<SaberModelController, SetSaberFakeGlowColor[]>.Accessor _SetSaberFakeGlowAccessor = FieldAccessor<SaberModelController, SetSaberFakeGlowColor[]>.GetAccessor("_setSaberFakeGlowColors");
            private static readonly FieldAccessor<SaberModelController, TubeBloomPrePassLight>.Accessor _saberLightAccessor = FieldAccessor<SaberModelController, TubeBloomPrePassLight>.GetAccessor("_saberLight");

            private static readonly FieldAccessor<SaberTrail, Color>.Accessor _saberTrailColorAccessor = FieldAccessor<SaberTrail, Color>.GetAccessor("_color");

            private static readonly FieldAccessor<SaberBurnMarkArea, LineRenderer[]>.Accessor _lineRenderersAccessor = FieldAccessor<SaberBurnMarkArea, LineRenderer[]>.GetAccessor("_lineRenderers");

            private readonly SaberTrail _saberWeaponTrail;
            private readonly Color _trailTintColor;
            private readonly SetSaberGlowColor[] _setSaberGlowColors;
            private readonly SetSaberFakeGlowColor[] _setSaberFakeGlowColors;
            private readonly TubeBloomPrePassLight _saberLight;

            private readonly SaberModelController _bsm;
            private readonly SaberType _saberType;

            private BSMColorManager(SaberModelController bsm, SaberType saberType)
            {
                _bsm = bsm;
                _saberType = saberType;

                _saberWeaponTrail = _saberWeaponTrailAccessor(ref _bsm);
                _trailTintColor = _initDataAccessor(ref _bsm).trailTintColor;
                _setSaberGlowColors = _setSaberGlowColorsAccessor(ref _bsm);
                _setSaberFakeGlowColors = _SetSaberFakeGlowAccessor(ref _bsm);
                _saberLight = _saberLightAccessor(ref _bsm);
            }

            internal static IEnumerable<BSMColorManager> GetBSMColorManager(SaberType saberType)
            {
                return _bsmColorManagers.Where(n => n._saberType == saberType);
            }

            internal static BSMColorManager CreateBSMColorManager(SaberModelController bsm, SaberType saberType)
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

                    SaberTrail saberTrail = _saberWeaponTrail;
                    _saberTrailColorAccessor(ref saberTrail) = (color * _trailTintColor).linear;
                    _saberLight.color = color;

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
