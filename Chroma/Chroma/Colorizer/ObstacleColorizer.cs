namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using IPA.Utilities;
    using UnityEngine;

    public class ObstacleColorizer : ObjectColorizer
    {
        private static readonly FieldAccessor<ObstacleController, ColorManager>.Accessor _colorManagerAccessor = FieldAccessor<ObstacleController, ColorManager>.GetAccessor("_colorManager");

        private static readonly FieldAccessor<StretchableObstacle, ParametricBoxFrameController>.Accessor _obstacleFrameAccessor = FieldAccessor<StretchableObstacle, ParametricBoxFrameController>.GetAccessor("_obstacleFrame");
        private static readonly FieldAccessor<StretchableObstacle, ParametricBoxFakeGlowController>.Accessor _obstacleFakeGlowAccessor = FieldAccessor<StretchableObstacle, ParametricBoxFakeGlowController>.GetAccessor("_obstacleFakeGlow");
        private static readonly FieldAccessor<StretchableObstacle, float>.Accessor _addColorMultiplierAccessor = FieldAccessor<StretchableObstacle, float>.GetAccessor("_addColorMultiplier");
        private static readonly FieldAccessor<StretchableObstacle, float>.Accessor _obstacleCoreLerpToWhiteFactorAccessor = FieldAccessor<StretchableObstacle, float>.GetAccessor("_obstacleCoreLerpToWhiteFactor");
        private static readonly FieldAccessor<StretchableObstacle, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<StretchableObstacle, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");

        private static readonly int _tintColorID = Shader.PropertyToID("_TintColor");
        private static readonly int _addColorID = Shader.PropertyToID("_AddColor");

        private readonly ParametricBoxFrameController _obstacleFrame;
        private readonly ParametricBoxFakeGlowController _obstacleFakeGlow;
        private readonly float _addColorMultiplier;
        private readonly float _obstacleCoreLerpToWhiteFactor;
        private readonly MaterialPropertyBlockController[] _materialPropertyBlockControllers;

        internal ObstacleColorizer(ObstacleControllerBase obstacleController)
        {
            StretchableObstacle stretchableObstacle = obstacleController.GetComponent<StretchableObstacle>();
            _obstacleFrame = _obstacleFrameAccessor(ref stretchableObstacle);
            _obstacleFakeGlow = _obstacleFakeGlowAccessor(ref stretchableObstacle);
            _addColorMultiplier = _addColorMultiplierAccessor(ref stretchableObstacle);
            _obstacleCoreLerpToWhiteFactor = _obstacleCoreLerpToWhiteFactorAccessor(ref stretchableObstacle);
            _materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref stretchableObstacle);

            if (obstacleController is ObstacleController trueObstacleController)
            {
                OriginalColor = _colorManagerAccessor(ref trueObstacleController).obstaclesColor;
            }
            else
            {
                // Fallback
                OriginalColor = Color.white;
            }

            Colorizers.Add(obstacleController, this);
        }

        public static Dictionary<ObstacleControllerBase, ObstacleColorizer> Colorizers { get; } = new Dictionary<ObstacleControllerBase, ObstacleColorizer>();

        public static Color? GlobalColor { get; private set; }

        protected override Color? GlobalColorGetter => GlobalColor;

        public static void GlobalColorize(Color? color)
        {
            GlobalColor = color;
            foreach (KeyValuePair<ObstacleControllerBase, ObstacleColorizer> valuePair in Colorizers)
            {
                valuePair.Value.Refresh();
            }
        }

        protected override void Refresh()
        {
            Color color = Color;
            if (color == _obstacleFrame.color)
            {
                return;
            }

            _obstacleFrame.color = color;
            _obstacleFrame.Refresh();
            if (_obstacleFakeGlow != null)
            {
                _obstacleFakeGlow.color = color;
                _obstacleFakeGlow.Refresh();
            }

            Color value = color * _addColorMultiplier;
            value.a = 0f;
            foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers)
            {
                materialPropertyBlockController.materialPropertyBlock.SetColor(_addColorID, value);
                materialPropertyBlockController.materialPropertyBlock.SetColor(_tintColorID, Color.Lerp(color, Color.white, _obstacleCoreLerpToWhiteFactor));
                materialPropertyBlockController.ApplyChanges();
            }
        }
    }
}
