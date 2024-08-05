using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Animation.Transform;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.EnvironmentComponent;

internal class ParametricBoxControllerTransformOverride : IAffinity, IDisposable
{
    private static readonly MethodInfo _localPositionSetter =
        AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localPosition));

    private static readonly MethodInfo _localScaleSetter =
        AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localScale));

    private readonly CodeInstruction _getTransformPosition;

    private readonly CodeInstruction _getTransformScale;

    private readonly Dictionary<ParametricBoxController, Vector3> _positions = new();
    private readonly Dictionary<ParametricBoxController, Vector3> _scales = new();

    private ParametricBoxControllerTransformOverride()
    {
        _getTransformScale =
            InstanceTranspilers
                .EmitInstanceDelegate<Func<Vector3, ParametricBoxController, Vector3>>(GetTransformScale);
        _getTransformPosition =
            InstanceTranspilers.EmitInstanceDelegate<Func<Vector3, ParametricBoxController, Vector3>>(
                GetTransformPosition);
    }

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_getTransformScale);
        InstanceTranspilers.DisposeDelegate(_getTransformPosition);
    }

    internal void SetTransform(ParametricBoxController parametricBoxController, TransformData transformData)
    {
        if (transformData.Position.HasValue || transformData.LocalPosition.HasValue)
        {
            UpdatePosition(parametricBoxController);
        }

        if (transformData.Scale.HasValue)
        {
            UpdateScale(parametricBoxController);
        }
    }

    internal void UpdatePosition(ParametricBoxController parametricBoxController)
    {
        _positions[parametricBoxController] = parametricBoxController.transform.localPosition;
    }

    internal void UpdateScale(ParametricBoxController parametricBoxController)
    {
        _scales[parametricBoxController] = parametricBoxController.transform.localScale;
    }

    private Vector3 GetTransformPosition(Vector3 @default, ParametricBoxController parametricBoxController)
    {
        return _positions.TryGetValue(parametricBoxController, out Vector3 position) ? position : @default;
    }

    private Vector3 GetTransformScale(Vector3 @default, ParametricBoxController parametricBoxController)
    {
        return _scales.TryGetValue(parametricBoxController, out Vector3 scale) ? scale : @default;
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(ParametricBoxController), nameof(ParametricBoxController.Refresh))]
    private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- base.transform.localScale = new Vector3(this.width * 0.5f, this.height * 0.5f, this.length * 0.5f);
             * ++ base.transform.localScale = GetTransformScale(new Vector3(this.width * 0.5f, this.height * 0.5f, this.length * 0.5f), this);
             * -- base.transform.localPosition = new Vector3(0f, (0.5f - this.heightCenter) * this.height, 0f);
             * ++ base.transform.localPosition = GetTransformPosition(new Vector3(0f, (0.5f - this.heightCenter) * this.height, 0f), this);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _localScaleSetter))
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                _getTransformScale)
            .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _localPositionSetter))
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                _getTransformPosition)
            .InstructionEnumeration();
    }
}
