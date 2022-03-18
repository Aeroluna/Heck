using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches.Events
{
    internal class RingStepChromafier : IAffinity, IDisposable
    {
        private static readonly FieldInfo _moveSpeedField = AccessTools.Field(typeof(TrackLaneRingsPositionStepEffectSpawner), "_moveSpeed");

        private readonly CodeInstruction _getPrecisionStep;
        private readonly CodeInstruction _getPrecisionSpeed;
        private readonly CustomData _customData;

        private RingStepChromafier([Inject(Id = ChromaController.ID)] CustomData customData)
        {
            _customData = customData;
            _getPrecisionStep = InstanceTranspilers.EmitInstanceDelegate<Func<float, BasicBeatmapEventData, float>>(GetPrecisionStep);
            _getPrecisionSpeed = InstanceTranspilers.EmitInstanceDelegate<Func<float, BasicBeatmapEventData, float>>(GetPrecisionSpeed);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_getPrecisionStep);
            InstanceTranspilers.DisposeDelegate(_getPrecisionSpeed);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(TrackLaneRingsPositionStepEffectSpawner), nameof(TrackLaneRingsPositionStepEffectSpawner.HandleBeatmapEvent))]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
                .SetOpcodeAndAdvance(OpCodes.Ldarg_1)
                .InsertAndAdvance(
                    _getPrecisionStep,
                    new CodeInstruction(OpCodes.Stloc_0))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _moveSpeedField))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    _getPrecisionSpeed)
                .InstructionEnumeration();
        }

        private float GetPrecisionStep(float @default, BasicBeatmapEventData beatmapEventData)
        {
            _customData.Resolve(beatmapEventData, out ChromaEventData? chromaData);
            return chromaData is { Step: { } } ? chromaData.Step.Value : @default;
        }

        private float GetPrecisionSpeed(float @default, BasicBeatmapEventData beatmapEventData)
        {
            _customData.Resolve(beatmapEventData, out ChromaEventData? chromaData);
            return chromaData is { Speed: { } } ? chromaData.Speed.Value : @default;
        }
    }
}
