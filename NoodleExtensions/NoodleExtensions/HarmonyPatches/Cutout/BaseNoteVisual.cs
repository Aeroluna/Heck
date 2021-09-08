namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Heck;
    using IPA.Utilities;
    using UnityEngine;

    [HeckPatch(typeof(BaseNoteVisuals))]
    [HeckPatch("Awake")]
    internal static class BaseNoteVisualsAwake
    {
        private static readonly Dictionary<Type, MethodInfo> _setArrowTransparencyMethods = new Dictionary<Type, MethodInfo>();
        private static readonly FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.Accessor _noteCutoutAnimateEffectAccessor = FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");
        private static readonly FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.Accessor _cutoutEffectAccessor = FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.GetAccessor("_cuttoutEffects");

        private static void Postfix(BaseNoteVisuals __instance, NoteControllerBase ____noteController)
        {
            GameObject gameObject = __instance.gameObject;

            BaseNoteVisuals baseNoteVisuals = gameObject.GetComponent<BaseNoteVisuals>();
            CutoutAnimateEffect cutoutAnimateEffect = _noteCutoutAnimateEffectAccessor(ref baseNoteVisuals);
            CutoutEffect[] cutoutEffects = _cutoutEffectAccessor(ref cutoutAnimateEffect);
            CutoutEffect cutoutEffect = cutoutEffects.First(n => n.name != "NoteArrow"); // 1.11 NoteArrow has been added to the CutoutAnimateEffect and we don't want that
            CutoutManager.NoteCutoutEffects.Add(____noteController, new CutoutEffectWrapper(cutoutEffect));

            if (____noteController is ICubeNoteTypeProvider)
            {
                Type constructed = typeof(DisappearingArrowControllerBase<>).MakeGenericType(____noteController.GetType());
                MonoBehaviour disappearingArrowController = (MonoBehaviour)gameObject.GetComponent(constructed);
                MethodInfo method = GetSetArrowTransparency(disappearingArrowController.GetType());
                DisappearingArrowWrapper wrapper = new DisappearingArrowWrapper(disappearingArrowController, method);
                wrapper.SetCutout(1); // i have no fucking idea how this fixes the weird ghost arrow bug
                CutoutManager.NoteDisappearingArrowWrappers.Add(____noteController, wrapper);
            }
        }

        private static MethodInfo GetSetArrowTransparency(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_setArrowTransparencyMethods.TryGetValue(type, out MethodInfo value))
            {
                return value;
            }

            Type baseType = type.BaseType;
            ////NoodleLogger.IPAlogger.Debug($"Base type is {baseType.Name}<{string.Join(", ", baseType.GenericTypeArguments.Select(t => t.Name))}>");
            MethodInfo method = baseType.GetMethod("SetArrowTransparency", BindingFlags.NonPublic | BindingFlags.Instance);
            _setArrowTransparencyMethods[type] = method ?? throw new InvalidOperationException($"Type [{type.FullName}] does not contain method [SetArrowTransparency]");
            return method;
        }
    }

    [HeckPatch(typeof(BaseNoteVisuals))]
    [HeckPatch("OnDestroy")]
    internal static class DisappearingArrowControllerBaseGameNoteControllerOnDestroy
    {
        private static void Postfix(NoteControllerBase ____noteController)
        {
            CutoutManager.NoteCutoutEffects.Remove(____noteController);
            CutoutManager.NoteDisappearingArrowWrappers.Remove(____noteController);
        }
    }
}
