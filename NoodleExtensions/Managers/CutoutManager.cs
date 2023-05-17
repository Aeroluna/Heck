using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NoodleExtensions.Managers
{
    [UsedImplicitly]
    public class CutoutManager
    {
        public Dictionary<ObstacleControllerBase, CutoutAnimateEffectWrapper> ObstacleCutoutEffects { get; } = new();

        public Dictionary<NoteControllerBase, CutoutEffectWrapper> NoteCutoutEffects { get; } = new();

        public Dictionary<NoteControllerBase, DisappearingArrowWrapper> NoteDisappearingArrowWrappers { get; } = new();

        public Dictionary<SliderMovement, CutoutAnimateEffectWrapper> SliderCutoutEffects { get; } = new();
    }

    public abstract class CutoutWrapper
    {
        public float Cutout { get; private set; } = 1;

        public virtual void SetCutout(float cutout)
        {
            Cutout = cutout;
        }
    }

    public class CutoutEffectWrapper : CutoutWrapper
    {
        private readonly CutoutEffect _cutoutEffect;

        public CutoutEffectWrapper(CutoutEffect cutoutEffect)
        {
            _cutoutEffect = cutoutEffect;
        }

        public override void SetCutout(float cutout)
        {
            base.SetCutout(cutout);
            _cutoutEffect.SetCutout(1 - cutout);
        }
    }

    public class CutoutAnimateEffectWrapper : CutoutWrapper
    {
        private readonly CutoutAnimateEffect _cutoutAnimateEffect;

        public CutoutAnimateEffectWrapper(CutoutAnimateEffect cutoutAnimateEffect)
        {
            _cutoutAnimateEffect = cutoutAnimateEffect;
        }

        public override void SetCutout(float cutout)
        {
            base.SetCutout(cutout);
            _cutoutAnimateEffect.SetCutout(1 - cutout);
        }
    }

    public class DisappearingArrowWrapper : CutoutWrapper
    {
        private readonly Action<float> _method;

        public DisappearingArrowWrapper(Action<float> method)
        {
            _method = method;
        }

        public override void SetCutout(float cutout)
        {
            base.SetCutout(cutout);
            _method(cutout);
        }
    }
}
