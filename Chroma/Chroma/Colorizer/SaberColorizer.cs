namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using IPA.Utilities;
    using SiraUtil;
    using UnityEngine;

    // Somehow this how just become a wrapper for Sirautil
    public static class SaberColorizer
    {
        private static readonly Dictionary<SaberType, BSMColorManager> _bsmColorManagers = new Dictionary<SaberType, BSMColorManager>();

        internal static Color?[] SaberColorOverride { get; } = new Color?[2] { null, null };

        internal static SaberBurnMarkArea SaberBurnMarkArea { private get; set; }

        public static void SetSaberColor(this SaberType saberType, Color color)
        {
            BSMColorManager.GetBSMColorManager(saberType)?.SetSaberColor(color);
        }

        public static void SetAllSaberColors(Color? color0, Color? color1)
        {
            if (color0.HasValue)
            {
                BSMColorManager.GetBSMColorManager(SaberType.SaberA)?.SetSaberColor(color0.Value);
            }

            if (color1.HasValue)
            {
                BSMColorManager.GetBSMColorManager(SaberType.SaberB)?.SetSaberColor(color1.Value);
            }
        }

        internal static void ClearBSMColorManagers()
        {
            _bsmColorManagers.Clear();
        }

        /*
         * BSM ColorSO holders
         */

        internal static void BSMStart(Saber bcm, SaberType saberType)
        {
            if (saberType == SaberType.SaberA || saberType == SaberType.SaberB)
            {
                BSMColorManager.CreateBSMColorManager(bcm, saberType);
            }
        }

        private class BSMColorManager
        {
            private static readonly FieldAccessor<SaberBurnMarkArea, LineRenderer[]>.Accessor _lineRenderersAccessor = FieldAccessor<SaberBurnMarkArea, LineRenderer[]>.GetAccessor("_lineRenderers");

            private readonly Saber _bsm;
            private readonly SaberType _saberType;
            private Color _lastColor;

            private BSMColorManager(Saber bsm, SaberType saberType)
            {
                _bsm = bsm;
                _saberType = saberType;
            }

            internal static BSMColorManager GetBSMColorManager(SaberType saberType)
            {
                if (_bsmColorManagers.TryGetValue(saberType, out BSMColorManager colorManager))
                {
                    return colorManager;
                }

                return null;
            }

            internal static BSMColorManager CreateBSMColorManager(Saber bsm, SaberType saberType)
            {
                BSMColorManager bsmcm;
                bsmcm = new BSMColorManager(bsm, saberType);
                _bsmColorManagers.Add(saberType, bsmcm);
                return bsmcm;
            }

            internal void SetSaberColor(Color color)
            {
                if (color == _lastColor)
                {
                    return;
                }

                _bsm.ChangeColor(color);
                _lastColor = color;

                SaberBurnMarkArea saberBurnMarkArea = SaberBurnMarkArea;
                LineRenderer[] lineRenderers = _lineRenderersAccessor(ref saberBurnMarkArea);
                lineRenderers[(int)_saberType].startColor = color;
                lineRenderers[(int)_saberType].endColor = color;
            }
        }
    }
}
