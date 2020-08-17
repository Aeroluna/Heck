namespace Chroma.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using IPA.Utilities;
    using UnityEngine;

    internal static class ObstacleControllerExtensions
    {
        private static readonly HashSet<OCColorManager> _ocColorManagers = new HashSet<OCColorManager>();

        internal static void ClearOCColorManagers()
        {
            _ocColorManagers.Clear();
        }

        internal static void Reset(this ObstacleController oc)
        {
            OCColorManager.GetOCColorManager(oc)?.Reset();
        }

        internal static void ResetAllObstacleColors()
        {
            foreach (OCColorManager ocColorManager in _ocColorManagers)
            {
                ocColorManager.Reset();
            }
        }

        internal static void SetObstacleColor(this ObstacleController oc, Color? color)
        {
            OCColorManager.GetOCColorManager(oc)?.SetObstacleColor(color);
        }

        internal static void SetActiveColors(this ObstacleController oc)
        {
            OCColorManager.GetOCColorManager(oc).SetActiveColors();
        }

        internal static void SetAllActiveColors()
        {
            foreach (OCColorManager ocColorManager in _ocColorManagers)
            {
                ocColorManager.SetActiveColors();
            }
        }

        /*
         * OC ColorSO holders
         */

        internal static void OCStart(ObstacleController oc)
        {
            OCColorManager.CreateOCColorManager(oc);
        }

        internal static void OCDestroy(ObstacleController oc)
        {
            OCColorManager.GetOCColorManager(oc)?.OCDestroyed();
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

            private readonly ObstacleController _oc;

            private readonly Color _color_Original;

            private readonly SimpleColorSO _color;

            private StretchableObstacle _stretchableObstacle;

            private OCColorManager(ObstacleController oc)
            {
                _oc = oc;
                _stretchableObstacle = _stretchableObstacleAccessor(ref _oc);
                InitializeSOs(oc, "_color", ref _color, ref _color_Original);
            }

            internal static OCColorManager GetOCColorManager(ObstacleController oc)
            {
                return _ocColorManagers.FirstOrDefault(n => n._oc == oc);
            }

            internal static OCColorManager CreateOCColorManager(ObstacleController oc)
            {
                // TODO TODO TODO: FIFX FIXFXIFXIFX FIX
                if (GetOCColorManager(oc) != null)
                {
                    return null;
                }

                OCColorManager occm;
                occm = new OCColorManager(oc);
                _ocColorManagers.Add(occm);
                return occm;
            }

            internal void OCDestroyed()
            {
                _ocColorManagers.Remove(this);
            }

            internal void Reset()
            {
                _color.SetColor(_color_Original);
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

            private void InitializeSOs(ObstacleController oc, string id, ref SimpleColorSO sColorSO, ref Color originalColor)
            {
                originalColor = oc.GetField<SimpleColorSO, ObstacleController>(id).color;

                if (sColorSO == null)
                {
                    sColorSO = ScriptableObject.CreateInstance<SimpleColorSO>();
                    sColorSO.SetColor(originalColor);
                }

                oc.SetField(id, sColorSO);
            }
        }
    }
}
