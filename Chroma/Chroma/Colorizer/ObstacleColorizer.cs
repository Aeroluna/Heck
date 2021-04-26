namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using IPA.Utilities;
    using UnityEngine;

    public static class ObstacleColorizer
    {
        private static readonly Dictionary<ObstacleController, OCColorManager> _ocColorManagers = new Dictionary<ObstacleController, OCColorManager>();

        public static void Reset(this ObstacleController oc)
        {
            OCColorManager.GetOCColorManager(oc)?.Reset();
        }

        public static void ResetAllObstacleColors()
        {
            OCColorManager.ResetGlobal();

            foreach (KeyValuePair<ObstacleController, OCColorManager> ocColorManager in _ocColorManagers)
            {
                ocColorManager.Value.Reset();
            }
        }

        public static void SetObstacleColor(this ObstacleController oc, Color color)
        {
            OCColorManager.GetOCColorManager(oc)?.SetObstacleColor(color);
        }

        public static void SetAllObstacleColors(Color color)
        {
            OCColorManager.SetGlobalObstacleColor(color);
        }

        public static void SetActiveColors(this ObstacleController oc)
        {
            OCColorManager.GetOCColorManager(oc).SetActiveColors();
        }

        public static void SetAllActiveColors()
        {
            foreach (KeyValuePair<ObstacleController, OCColorManager> ocColorManager in _ocColorManagers)
            {
                ocColorManager.Value.SetActiveColors();
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

        internal static void OCStart(ObstacleController oc, Color original)
        {
            OCColorManager.CreateOCColorManager(oc, original);
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

            private readonly Color _color_Original;

            private Color? _color;

            private StretchableObstacle _stretchableObstacle;

            private OCColorManager(ObstacleController oc, Color original)
            {
                _stretchableObstacle = _stretchableObstacleAccessor(ref oc);

                _color_Original = original;

                _color = _color_Original;
            }

            internal static OCColorManager GetOCColorManager(ObstacleController oc)
            {
                if (_ocColorManagers.TryGetValue(oc, out OCColorManager colorManager))
                {
                    return colorManager;
                }

                return null;
            }

            internal static OCColorManager CreateOCColorManager(ObstacleController oc, Color original)
            {
                if (GetOCColorManager(oc) != null)
                {
                    return null;
                }

                OCColorManager occm;
                occm = new OCColorManager(oc, original);
                _ocColorManagers.Add(oc, occm);
                return occm;
            }

            internal static void SetGlobalObstacleColor(Color color)
            {
                _globalColor = color;
            }

            internal static void ResetGlobal()
            {
                _globalColor = null;
            }

            internal void Reset()
            {
                _color = null;
            }

            internal void SetObstacleColor(Color color)
            {
                _color = color;
            }

            internal void SetActiveColors()
            {
                Color finalColor = _color ?? _globalColor ?? _color_Original;
                ParametricBoxFrameController obstacleFrame = _obstacleFrameAccessor(ref _stretchableObstacle);

                if (finalColor == obstacleFrame.color)
                {
                    return;
                }

                ParametricBoxFakeGlowController obstacleFakeGlow = _obstacleFakeGlowAccessor(ref _stretchableObstacle);
                MaterialPropertyBlockController[] materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref _stretchableObstacle);
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
