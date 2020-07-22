namespace Chroma.HarmonyPatches
{
    using System.Linq;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        private static SimpleColorSO _customObstacleColor;
        private static SimpleColorSO _defaultObstacleColor;

        internal static SimpleColorSO DefaultObstacleColorSO
        {
            get
            {
                if (_defaultObstacleColor == null)
                {
                    _defaultObstacleColor = Resources.FindObjectsOfTypeAll<ColorManager>().First().GetField<SimpleColorSO, ColorManager>("_obstaclesColor");
                }

                return _defaultObstacleColor;
            }
        }

        internal static SimpleColorSO CustomObstacleColorSO
        {
            get
            {
                if (_customObstacleColor == null)
                {
                    _customObstacleColor = ScriptableObject.CreateInstance<SimpleColorSO>();
                }

                return _customObstacleColor;
            }
        }

        internal static void ClearObstacleColors()
        {
            _defaultObstacleColor = null;
            Object.Destroy(_customObstacleColor);
            _customObstacleColor = null;
        }

#pragma warning disable SA1313
        private static void Prefix(ref SimpleColorSO ____color, ObstacleData obstacleData)
#pragma warning restore SA1313
        {
            // CustomJSONData _customData individual color override
            if (obstacleData is CustomObstacleData customData && ChromaBehaviour.LightingRegistered)
            {
                dynamic dynData = customData.customData;

                Color? c = ChromaUtils.GetColorFromData(dynData);

                if (c.HasValue)
                {
                    ____color = CustomObstacleColorSO;
                    ____color.SetColor(c.Value);
                }
                else
                {
                    ____color = DefaultObstacleColorSO;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Update")]
    internal class ObstacleControllerUpdate
    {
        private static readonly FieldAccessor<StretchableObstacle, ParametricBoxFrameController>.Accessor _obstacleFrameAccessor = FieldAccessor<StretchableObstacle, ParametricBoxFrameController>.GetAccessor("_obstacleFrame");
        private static readonly FieldAccessor<StretchableObstacle, ParametricBoxFakeGlowController>.Accessor _obstacleFakeGlowAccessor = FieldAccessor<StretchableObstacle, ParametricBoxFakeGlowController>.GetAccessor("_obstacleFakeGlow");
        private static readonly FieldAccessor<StretchableObstacle, float>.Accessor _addColorMultiplierAccessor = FieldAccessor<StretchableObstacle, float>.GetAccessor("_addColorMultiplier");
        private static readonly FieldAccessor<StretchableObstacle, float>.Accessor _obstacleCoreLerpToWhiteFactorAccessor = FieldAccessor<StretchableObstacle, float>.GetAccessor("_obstacleCoreLerpToWhiteFactor");
        private static readonly FieldAccessor<StretchableObstacle, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<StretchableObstacle, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
        private static readonly int _tintColorID = Shader.PropertyToID("_TintColor");
        private static readonly int _addColorID = Shader.PropertyToID("_AddColor");

#pragma warning disable SA1313
        private static void Postfix(ref SimpleColorSO ____color, StretchableObstacle ____stretchableObstacle, ObstacleData ____obstacleData, AudioTimeSyncController ____audioTimeSyncController, float ____startTimeOffset, float ____move1Duration, float ____move2Duration, float ____obstacleDuration)
#pragma warning restore SA1313
        {
            if (Chroma.Plugin.NoodleExtensionsActive)
            {
                TrackColorize(ref ____color, ____stretchableObstacle, ____obstacleData, ____audioTimeSyncController, ____startTimeOffset, ____move1Duration, ____move2Duration, ____obstacleDuration);
            }
        }

        private static void TrackColorize(ref SimpleColorSO color, StretchableObstacle stretchableObstacle, ObstacleData obstacleData, AudioTimeSyncController audioTimeSyncController, float startTimeOffset, float move1Duration, float move2Duration, float obstacleDuration)
        {
            if (obstacleData is CustomObstacleData customData && ChromaBehaviour.LightingRegistered)
            {
                dynamic dynData = customData.customData;
                Track track = AnimationHelper.GetTrack(dynData);
                dynamic animationObject = Trees.at(dynData, "_animation");

                if (track != null || animationObject != null)
                {
                    float jumpDuration = move2Duration;
                    float elapsedTime = audioTimeSyncController.songTime - startTimeOffset;
                    float normalTime = (elapsedTime - move1Duration) / (jumpDuration + obstacleDuration);

                    Chroma.AnimationHelper.GetColorOffset(animationObject, track, normalTime, out Color? colorOffset);

                    if (colorOffset.HasValue)
                    {
                        color = ObstacleControllerInit.CustomObstacleColorSO;
                        color.SetColor(colorOffset.Value);

                        ParametricBoxFrameController obstacleFrame = _obstacleFrameAccessor(ref stretchableObstacle);
                        ParametricBoxFakeGlowController obstacleFakeGlow = _obstacleFakeGlowAccessor(ref stretchableObstacle);
                        MaterialPropertyBlockController[] materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref stretchableObstacle);
                        Color finalColor = color;
                        obstacleFrame.color = finalColor;
                        obstacleFrame.Refresh();
                        obstacleFakeGlow.color = finalColor;
                        obstacleFakeGlow.Refresh();
                        Color value = finalColor * _addColorMultiplierAccessor(ref stretchableObstacle);
                        value.a = 0f;
                        float obstacleCoreLerpToWhiteFactor = _obstacleCoreLerpToWhiteFactorAccessor(ref stretchableObstacle);
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
    }
}
