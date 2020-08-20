namespace Chroma.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    internal static class BombColorizer
    {
        private static readonly HashSet<BNCColorManager> _bncColorManagers = new HashSet<BNCColorManager>();

        internal static void ClearBNCColorManagers()
        {
            _bncColorManagers.Clear();
        }

        internal static void Reset(this BombNoteController bnc)
        {
            BNCColorManager.GetBNCColorManager(bnc)?.Reset();
        }

        internal static void ResetAllBombColors()
        {
            foreach (BNCColorManager bncColorManager in _bncColorManagers)
            {
                bncColorManager.Reset();
            }
        }

        internal static void SetBombColor(this BombNoteController bnc, Color? color)
        {
            BNCColorManager.GetBNCColorManager(bnc)?.SetBombColor(color);
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
            private readonly BombNoteController _nc;

            private readonly Color _color_Original;

            private readonly Material _bombMaterial;

            private BNCColorManager(BombNoteController nc)
            {
                _nc = nc;

                _bombMaterial = nc.noteTransform.gameObject.GetComponent<Renderer>().material;

                _color_Original = _bombMaterial.GetColor("_SimpleColor");
            }

            internal static BNCColorManager GetBNCColorManager(BombNoteController nc)
            {
                return _bncColorManagers.FirstOrDefault(n => n._nc == nc);
            }

            internal static BNCColorManager CreateBNCColorManager(BombNoteController nc)
            {
                if (GetBNCColorManager(nc) != null)
                {
                    return null;
                }

                BNCColorManager bnccm;
                bnccm = new BNCColorManager(nc);
                _bncColorManagers.Add(bnccm);
                return bnccm;
            }

            internal void Reset()
            {
                _bombMaterial.SetColor("_SimpleColor", _color_Original);
            }

            internal void SetBombColor(Color? color)
            {
                if (color.HasValue)
                {
                    _bombMaterial.SetColor("_SimpleColor", color.Value);
                }
            }
        }
    }
}
