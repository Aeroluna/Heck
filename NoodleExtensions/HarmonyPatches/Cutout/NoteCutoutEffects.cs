using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Utilities;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.Cutout
{
    internal class NoteCutoutEffects : IAffinity
    {
        private static readonly Dictionary<Type, MethodInfo> _setArrowTransparencyMethods = new();
        private static readonly FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.Accessor _noteCutoutAnimateEffectAccessor = FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");
        private static readonly FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.Accessor _cutoutEffectAccessor = FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.GetAccessor("_cuttoutEffects");

        private readonly CutoutManager _cutoutManager;

        private NoteCutoutEffects(CutoutManager cutoutManager)
        {
            _cutoutManager = cutoutManager;
        }

        private static MethodInfo GetSetArrowTransparency(Type type)
        {
            if (_setArrowTransparencyMethods.TryGetValue(type, out MethodInfo value))
            {
                return value;
            }

            ////NoodleLogger.IPAlogger.Debug($"Base type is {baseType.Name}<{string.Join(", ", baseType.GenericTypeArguments.Select(t => t.Name))}>");
            MethodInfo? method = AccessTools.Method(type, "SetArrowTransparency");
            _setArrowTransparencyMethods[type] = method ?? throw new InvalidOperationException($"Type [{type.FullName}] does not contain method [SetArrowTransparency]");
            return method;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.HandleNoteControllerDidInit))]
        private void CreateCutoutWrapper(BaseNoteVisuals __instance, NoteControllerBase ____noteController)
        {
            if (_cutoutManager.NoteCutoutEffects.ContainsKey(____noteController))
            {
                return;
            }

            GameObject gameObject = __instance.gameObject;

            BaseNoteVisuals baseNoteVisuals = gameObject.GetComponent<BaseNoteVisuals>();
            CutoutAnimateEffect cutoutAnimateEffect = _noteCutoutAnimateEffectAccessor(ref baseNoteVisuals);
            CutoutEffect[] cutoutEffects = _cutoutEffectAccessor(ref cutoutAnimateEffect);
            CutoutEffect cutoutEffect = cutoutEffects.First(n => n.name != "NoteArrow"); // 1.11 NoteArrow has been added to the CutoutAnimateEffect and we don't want that
            _cutoutManager.NoteCutoutEffects.Add(____noteController, new CutoutEffectWrapper(cutoutEffect));

            if (____noteController is not ICubeNoteTypeProvider)
            {
                return;
            }

            Type constructed = typeof(DisappearingArrowControllerBase<>).MakeGenericType(____noteController.GetType());
            MonoBehaviour disappearingArrowController = (MonoBehaviour)gameObject.GetComponent(constructed);
            MethodInfo method = GetSetArrowTransparency(constructed);
            Action<float> delegat = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), disappearingArrowController, method);
            DisappearingArrowWrapper wrapper = new(delegat);
            wrapper.SetCutout(1); // i have no fucking idea how this fixes the weird ghost arrow bug
            _cutoutManager.NoteDisappearingArrowWrappers.Add(____noteController, wrapper);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BaseNoteVisuals), nameof(BaseNoteVisuals.Awake))]
        private void DestroyCutoutWrapper(NoteControllerBase ____noteController)
        {
            _cutoutManager.NoteCutoutEffects.Remove(____noteController);
            _cutoutManager.NoteDisappearingArrowWrappers.Remove(____noteController);
        }
    }
}
