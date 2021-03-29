namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class BombColorizer
    {
        private static readonly Dictionary<BombNoteController, BNCColorManager> _bncColorManagers = new Dictionary<BombNoteController, BNCColorManager>();

        public static void Reset(this BombNoteController bnc)
        {
            BNCColorManager.GetBNCColorManager(bnc)?.Reset();
        }

        public static void ResetAllBombColors()
        {
            BNCColorManager.ResetGlobal();

            foreach (KeyValuePair<BombNoteController, BNCColorManager> bncColorManager in _bncColorManagers)
            {
                bncColorManager.Value.Reset();
            }
        }

        public static void SetBombColor(this BombNoteController bnc, Color? color)
        {
            BNCColorManager.GetBNCColorManager(bnc)?.SetBombColor(color);
        }

        public static void SetAllBombColors(Color? color)
        {
            BNCColorManager.SetGlobalBombColor(color);

            foreach (KeyValuePair<BombNoteController, BNCColorManager> bncColorManager in _bncColorManagers)
            {
                bncColorManager.Value.Reset();
            }
        }

        internal static void ClearBNCColorManagers()
        {
            ResetAllBombColors();
            _bncColorManagers.Clear();
        }

        /*
         * NC ColorSO holders
         */

        internal static void BNCStart(BombNoteController bnc)
        {
            BNCColorManager.CreateBNCColorManager(bnc);
        }

        private class BNCColorManager
        {
            private static Color? _globalColor = null;

            private readonly BombNoteController _nc;

            private readonly Color _color_Original;

            private readonly Material _bombMaterial;

            private BNCColorManager(BombNoteController nc)
            {
                _nc = nc;

                _bombMaterial = nc.noteTransform.gameObject.GetComponent<Renderer>().material;

                _color_Original = _bombMaterial.GetColor("_SimpleColor");
                if (_globalColor.HasValue)
                {
                    _bombMaterial.SetColor("_SimpleColor", _globalColor.Value);
                }
            }

            internal static BNCColorManager GetBNCColorManager(BombNoteController nc)
            {
                if (_bncColorManagers.TryGetValue(nc, out BNCColorManager colorManager))
                {
                    return colorManager;
                }

                return null;
            }

            internal static BNCColorManager CreateBNCColorManager(BombNoteController nc)
            {
                if (GetBNCColorManager(nc) != null)
                {
                    return null;
                }

                BNCColorManager bnccm;
                bnccm = new BNCColorManager(nc);
                _bncColorManagers.Add(nc, bnccm);
                return bnccm;
            }

            internal static void SetGlobalBombColor(Color? color)
            {
                if (color.HasValue)
                {
                    _globalColor = color.Value;
                }
            }

            internal static void ResetGlobal()
            {
                _globalColor = null;
            }

            internal void Reset()
            {
                if (_globalColor.HasValue)
                {
                    _bombMaterial.SetColor("_SimpleColor", _globalColor.Value);
                }
                else
                {
                    _bombMaterial.SetColor("_SimpleColor", _color_Original);
                }
            }

            internal void SetBombColor(Color? color)
            {
                if (color.HasValue && color.Value != _bombMaterial.GetColor("_SimpleColor"))
                {
                    _bombMaterial.SetColor("_SimpleColor", color.Value);
                }
            }
        }
    }
}
