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
    public class ObjectInitializer : IGameNoteInitializer, IBombNoteInitializer, IObstacleInitializer, ISliderInitializer
    {
        private static readonly Dictionary<Type, MethodInfo> _setArrowTransparencyMethods = new();

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
            ObstacleDissolve obstacleDissolve = obstacleController.GetComponent<ObstacleDissolve>();
            _cutoutManager.ObstacleCutoutEffects.Add(
                obstacleController,
                new CutoutAnimateEffectWrapper(obstacleDissolve._cutoutAnimateEffect));
        }

        public void InitializeSlider(SliderControllerBase sliderController)
        {
            SliderMovement sliderMovement = sliderController.GetComponent<SliderMovement>();
            _cutoutManager.SliderCutoutEffects.Add(
                sliderMovement,
                new CutoutAnimateEffectWrapper(sliderController._cutoutAnimateEffect));
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
            CutoutAnimateEffect cutoutAnimateEffect = baseNoteVisuals._cutoutAnimateEffect;
            CutoutEffect[] cutoutEffects = cutoutAnimateEffect._cuttoutEffects;
            CutoutEffect cutoutEffect = cutoutEffects.First(n => n.name != "NoteArrow"); // 1.11 NoteArrow has been added to the CutoutAnimateEffect and we don't want that
            _cutoutManager.NoteCutoutEffects.Add(noteController, new CutoutEffectWrapper(cutoutEffect));
        }
    }
}
