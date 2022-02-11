using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
using NoodleExtensions.Managers;
using UnityEngine;

namespace NoodleExtensions
{
    [UsedImplicitly]
    internal class ObjectInitializer : IGameNoteInitializer, IBombNoteInitializer, IObstacleInitializer
    {
        private static readonly Dictionary<Type, MethodInfo> _setArrowTransparencyMethods = new();
        private static readonly FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.Accessor _noteCutoutAnimateEffectAccessor = FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");
        private static readonly FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.Accessor _obstacleCutoutAnimateEffectAccessor = FieldAccessor<ObstacleDissolve, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");
        private static readonly FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.Accessor _cutoutEffectAccessor = FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.GetAccessor("_cuttoutEffects");

        private readonly CutoutManager _cutoutManager;

        private ObjectInitializer(CutoutManager cutoutManager)
        {
            _cutoutManager = cutoutManager;
        }

        public void InitializeGameNote(NoteControllerBase noteController)
        {
            CreateBasicNoteCutout(noteController);

            Type constructed = typeof(DisappearingArrowControllerBase<>).MakeGenericType(noteController.GetType());
            MonoBehaviour disappearingArrowController = (MonoBehaviour)noteController.GetComponent(constructed);
            MethodInfo method = GetSetArrowTransparency(constructed);
            Action<float> delegat = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), disappearingArrowController, method);
            DisappearingArrowWrapper wrapper = new(delegat);
            _cutoutManager.NoteDisappearingArrowWrappers.Add(noteController, wrapper);
        }

        public void InitializeBombNote(NoteControllerBase noteController)
        {
            CreateBasicNoteCutout(noteController);
        }

        public void InitializeObstacle(ObstacleControllerBase obstacleController)
        {
            ObstacleDissolve cutout = obstacleController.GetComponent<ObstacleDissolve>();
            _cutoutManager.ObstacleCutoutEffects.Add(
                obstacleController,
                new CutoutAnimateEffectWrapper(_obstacleCutoutAnimateEffectAccessor(ref cutout)));
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

        private void CreateBasicNoteCutout(NoteControllerBase noteController)
        {
            BaseNoteVisuals baseNoteVisuals = noteController.GetComponent<BaseNoteVisuals>();
            CutoutAnimateEffect cutoutAnimateEffect = _noteCutoutAnimateEffectAccessor(ref baseNoteVisuals);
            CutoutEffect[] cutoutEffects = _cutoutEffectAccessor(ref cutoutAnimateEffect);
            CutoutEffect cutoutEffect = cutoutEffects.First(n => n.name != "NoteArrow"); // 1.11 NoteArrow has been added to the CutoutAnimateEffect and we don't want that
            _cutoutManager.NoteCutoutEffects.Add(noteController, new CutoutEffectWrapper(cutoutEffect));
        }
    }
}
