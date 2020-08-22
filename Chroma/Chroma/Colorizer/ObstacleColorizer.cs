namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using System.Linq;
    using IPA.Utilities;
    using UnityEngine;

    public static class ObstacleColorizer
    {
        private static readonly HashSet<OCColorManager> _ocColorManagers = new HashSet<OCColorManager>();

        public static void Reset(this ObstacleController oc)
        {
            OCColorManager.GetOCColorManager(oc)?.Reset();
        }

        public static void ResetAllObstacleColors()
        {
            OCColorManager.ResetGlobal();

            foreach (OCColorManager ocColorManager in _ocColorManagers)
            {
                ocColorManager.Reset();
            }
        }

        public static void SetObstacleColor(this ObstacleController oc, Color? color)
        {
            OCColorManager.GetOCColorManager(oc)?.SetObstacleColor(color);
        }

        public static void SetAllObstacleColors(Color? color)
        {
            OCColorManager.SetGlobalObstacleColor(color);

            foreach (OCColorManager ocColorManager in _ocColorManagers)
            {
                ocColorManager.Reset();
            }
        }

        public static void SetActiveColors(this ObstacleController oc)
        {
            OCColorManager.GetOCColorManager(oc).SetActiveColors();
        }

        public static void SetAllActiveColors()
        {
            foreach (OCColorManager ocColorManager in _ocColorManagers)
            {
                ocColorManager.SetActiveColors();
            }
        }

        internal static void ClearOCColorManagers()
        {
            ResetAllObstacleColors();
            _ocColorManagers.Clear();
        }

        /*
         * OC ColorSO holders
         */

        internal static void OCStart(ObstacleController oc)
        {
            OCColorManager.CreateOCColorManager(oc);
        }

        private class OCColorManager
        {
            private static readonly FieldAccessor<ObstacleController, StretchableObstacle>.Accessor _stretchableObstacleAccessor = FieldAccessor<ObstacleController, StretchableObstacle>.GetAccessor("_stretchableObstacle");
            private static readonly FieldAccessor<StretchableObstacle, ParametricBoxFrameController>.Accessor _obstacleFrameAccessor = FieldAccessor<StretchableObstacle, ParametricBoxFrameController>.GetAccessor("_obstacleFrame");
            private static readonly FieldAccessor<StretchableObstacle, ParametricBoxFakeGlowController>.Accessor _obstacleFakeGlowAccessor = FieldAccessor<StretchableObstacle, ParametricBoxFakeGlowController>.GetAccessor("_obstacleFakeGlow");
            private static readonly FieldAccessor<StretchableObstacle, float>.Accessor _addColorMultiplierAccessor = FieldAccessor<StretchableObstacle, float>.GetAccessor("_addColorMultiplier");
            private static readonly FieldAccessor<StretchableObstacle, float>.Accessor _obstacleCoreLerpToWhiteFactorAccessor = FieldAccessor<StretchableObstacle, float>.GetAccessor("_obstacleCoreLerpToWhiteFactor");
            private static readonly FieldAccessor<StretchableObstacle, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<StretchableObstacle, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
            private static readonly int _tintColorID = Shader.PropertyToID("_TintColor");
            private static readonly int _addColorID = Shader.PropertyToID("_AddColor");

            private static Color? _globalColor = null;

            private readonly ObstacleController _oc;

            private readonly Color _color_Original;

            private readonly SimpleColorSO _color;

            private StretchableObstacle _stretchableObstacle;

            private OCColorManager(ObstacleController oc)
            {
                _oc = oc;
                _stretchableObstacle = _stretchableObstacleAccessor(ref _oc);

                _color_Original = oc.GetField<SimpleColorSO, ObstacleController>("_color").color;

                if (_color == null)
                {
                    _color = ScriptableObject.CreateInstance<SimpleColorSO>();
                    _color.SetColor(_color_Original);
                }

                oc.SetField("_color", _color);
            }

            internal static OCColorManager GetOCColorManager(ObstacleController oc)
            {
                return _ocColorManagers.FirstOrDefault(n => n._oc == oc);
            }

            internal static OCColorManager CreateOCColorManager(ObstacleController oc)
            {
                if (GetOCColorManager(oc) != null)
                {
                    return null;
                }

                OCColorManager occm;
                occm = new OCColorManager(oc);
                _ocColorManagers.Add(occm);
                return occm;
            }

            internal static void SetGlobalObstacleColor(Color? color)
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
                    _color.SetColor(_globalColor.Value);
                }
                else
                {
                    _color.SetColor(_color_Original);
                }
            }

            internal void SetObstacleColor(Color? color)
            {
                if (color.HasValue)
                {
                    _color.SetColor(color.Value);
                }
            }

            internal void SetActiveColors()
            {
                ParametricBoxFrameController obstacleFrame = _obstacleFrameAccessor(ref _stretchableObstacle);
                ParametricBoxFakeGlowController obstacleFakeGlow = _obstacleFakeGlowAccessor(ref _stretchableObstacle);
                MaterialPropertyBlockController[] materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref _stretchableObstacle);
                Color finalColor = _color;
                obstacleFrame.color = finalColor;
                obstacleFrame.Refresh();
                obstacleFakeGlow.color = finalColor;
                obstacleFakeGlow.Refresh();
                Color value = finalColor * _addColorMultiplierAccessor(ref _stretchableObstacle);
                value.a = 0f;
                float obstacleCoreLerpToWhiteFactor = _obstacleCoreLerpToWhiteFactorAccessor(ref _stretchableObstacle);
                foreach (MaterialPropertyBlockController materialPropertyBlockController in materialPropertyBlockControllers)
                {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(_addColorID, value);
                    materialPropertyBlockController.materialPropertyBlock.SetColor(_tintColorID, Color.Lerp(finalColor, Color.white, obstacleCoreLerpToWhiteFactor));
                    materialPropertyBlockController.ApplyChanges();
                }
            }
        }
    }
}
