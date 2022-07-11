using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SiraUtil.Affinity;

namespace Heck.HarmonyPatches
{
    internal class BurstSliderDataRegisterer : IAffinity, IDisposable
    {
        private static readonly ConstructorInfo _noteSpawnDataCtor = AccessTools.FirstConstructor(typeof(BeatmapObjectSpawnMovementData.NoteSpawnData), _ => true);

        private readonly HashSet<(object? Id, DeserializedData DeserializedData)> _deserializedDatas;
        private readonly CodeInstruction _registerBurstSliderNoteData;

        internal BurstSliderDataRegisterer(HashSet<(object? Id, DeserializedData DeserializedData)> deserializedDatas)
        {
            _deserializedDatas = deserializedDatas;
            _registerBurstSliderNoteData = InstanceTranspilers.EmitInstanceDelegate<Action<SliderData, NoteData>>(RegisterBurstSliderNoteData);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_registerBurstSliderNoteData);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(BurstSliderSpawner), nameof(BurstSliderSpawner.ProcessSliderData))]
        private IEnumerable<CodeInstruction> ReplaceConditionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _noteSpawnDataCtor))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_S, 20),
                    _registerBurstSliderNoteData)
                .InstructionEnumeration();
        }

        private void RegisterBurstSliderNoteData(SliderData sliderData, NoteData noteData)
        {
            foreach ((object? Id, DeserializedData DeserializedData) deserializedData in _deserializedDatas)
            {
                DeserializedData data = deserializedData.DeserializedData;
                if (data.Resolve(sliderData, out IObjectCustomData? objectCustomData) &&
                    objectCustomData is ICopyable<IObjectCustomData> copyable)
                {
                    data.RegisterNewObject(noteData, copyable.Copy());
                }
            }
        }
    }
}
