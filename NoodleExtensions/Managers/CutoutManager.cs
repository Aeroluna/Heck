using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NoodleExtensions.Managers
{
    [UsedImplicitly]
    internal class CutoutManager
    {
        internal Dictionary<ObstacleControllerBase, CutoutAnimateEffectWrapper> ObstacleCutoutEffects { get; } = new();

        internal Dictionary<NoteControllerBase, CutoutEffectWrapper> NoteCutoutEffects { get; } = new();

        internal Dictionary<NoteControllerBase, DisappearingArrowWrapper> NoteDisappearingArrowWrappers { get; } = new();
    }

    internal abstract class CutoutWrapper
    {
        internal float Cutout { get; private set; } = 1;

        internal virtual void SetCutout(float cutout)
        {
            Cutout = cutout;
        }
    }

    internal class CutoutEffectWrapper : CutoutWrapper
    {
        private readonly CutoutEffect _cutoutEffect;

        internal CutoutEffectWrapper(CutoutEffect cutoutEffect)
        {
            _cutoutEffect = cutoutEffect;
        }

        internal override void SetCutout(float cutout)
        {
            base.SetCutout(cutout);
            _cutoutEffect.SetCutout(1 - cutout);
        }
    }

    internal class CutoutAnimateEffectWrapper : CutoutWrapper
    {
        private readonly CutoutAnimateEffect _cutoutAnimateEffect;

        internal CutoutAnimateEffectWrapper(CutoutAnimateEffect cutoutAnimateEffect)
        {
            _cutoutAnimateEffect = cutoutAnimateEffect;
        }

        internal override void SetCutout(float cutout)
        {
            base.SetCutout(cutout);
            _cutoutAnimateEffect.SetCutout(1 - cutout);
        }
    }

    internal class DisappearingArrowWrapper : CutoutWrapper
    {
        private readonly Action<float> _method;

        internal DisappearingArrowWrapper(Action<float> method)
        {
            _method = method;
        }

        internal override void SetCutout(float cutout)
        {
            base.SetCutout(cutout);
            _method(cutout);
        }
    }
}
