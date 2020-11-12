namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using System.Linq;
    using IPA.Utilities;
    using SiraUtil;
    using UnityEngine;

    // Somehow this how just become a wrapper for Sirautil
    public static class SaberColorizer
    {
        private static readonly HashSet<BSMColorManager> _bsmColorManagers = new HashSet<BSMColorManager>();

        internal static Color?[] SaberColorOverride { get; } = new Color?[2] { null, null };

        internal static SaberBurnMarkArea SaberBurnMarkArea { private get; set; }

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

            private BSMColorManager(Saber bsm, SaberType saberType)
            {
                _bsm = bsm;
                _saberType = saberType;
            }

            internal static IEnumerable<BSMColorManager> GetBSMColorManager(SaberType saberType)
            {
                return _bsmColorManagers.Where(n => n._saberType == saberType);
            }

            internal static BSMColorManager CreateBSMColorManager(Saber bsm, SaberType saberType)
            {
                BSMColorManager bsmcm;
                bsmcm = new BSMColorManager(bsm, saberType);
                _bsmColorManagers.Add(bsmcm);
                return bsmcm;
            }

            internal void SetSaberColor(Color? colorNullable)
            {
                if (colorNullable.HasValue)
                {
                    Color color = colorNullable.Value;

                    _bsm.ChangeColor(color);

                    SaberBurnMarkArea saberBurnMarkArea = SaberBurnMarkArea;
                    LineRenderer[] lineRenderers = _lineRenderersAccessor(ref saberBurnMarkArea);
                    lineRenderers[(int)_saberType].startColor = color;
                    lineRenderers[(int)_saberType].endColor = color;
                }
            }
        }
    }
}
