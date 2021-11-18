namespace NoodleExtensions
{
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    internal static class CutoutManager
    {
        internal static Dictionary<ObstacleControllerBase, CutoutAnimateEffectWrapper> ObstacleCutoutEffects { get; } = new Dictionary<ObstacleControllerBase, CutoutAnimateEffectWrapper>();

        internal static Dictionary<NoteControllerBase, CutoutEffectWrapper> NoteCutoutEffects { get; } = new Dictionary<NoteControllerBase, CutoutEffectWrapper>();

        internal static Dictionary<NoteControllerBase, DisappearingArrowWrapper> NoteDisappearingArrowWrappers { get; } = new Dictionary<NoteControllerBase, DisappearingArrowWrapper>();
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
        private readonly MonoBehaviour _disappearingArrowController;

        private readonly MethodInfo _method;

        internal DisappearingArrowWrapper(MonoBehaviour disappearingArrowController, MethodInfo method)
        {
            _disappearingArrowController = disappearingArrowController;
            _method = method;
        }

        internal override void SetCutout(float cutout)
        {
            base.SetCutout(cutout);

            // gross nasty reflection
            _method.Invoke(_disappearingArrowController, new object[] { cutout });
        }
    }
}
