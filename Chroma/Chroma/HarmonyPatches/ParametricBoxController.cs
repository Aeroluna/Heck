namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma;
    using Heck;
    using HarmonyLib;
    using UnityEngine;

    [HeckPatch(typeof(ParametricBoxController))]
    [HeckPatch("Refresh")]
    internal static class ParametricBoxControllerRefresh
    {
        private static readonly MethodInfo _getTransformScale = SymbolExtensions.GetMethodInfo(() => GetTransformScale(Vector3.zero, null));
        private static readonly MethodInfo _getTransformPosition = SymbolExtensions.GetMethodInfo(() => GetTransformPosition(Vector3.zero, null));

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundScale = false;
            bool foundPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundScale &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "set_localScale")
                {
                    foundScale = true;

                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, _getTransformScale),
                    };
                    instructionList.InsertRange(i, codeInstructions);
                }

                if (!foundPosition &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "set_localPosition")
                {
                    foundPosition = true;

                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, _getTransformPosition),
                    };
                    instructionList.InsertRange(i, codeInstructions);
                }
            }

            if (!foundScale)
            {
                Plugin.Logger.Log("Failed to find callvirt to set_localScale!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundPosition)
            {
                Plugin.Logger.Log("Failed to find callvirt to set_localPosition!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static Vector3 GetTransformScale(Vector3 @default, ParametricBoxController parametricBoxController)
        {
            if (ParametricBoxControllerParameters.TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters))
            {
                if (parameters.Scale.HasValue)
                {
                    return parameters.Scale.Value;
                }
            }

            return @default;
        }

        private static Vector3 GetTransformPosition(Vector3 @default, ParametricBoxController parametricBoxController)
        {
            if (ParametricBoxControllerParameters.TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters))
            {
                if (parameters.Position.HasValue)
                {
                    return parameters.Position.Value;
                }
            }

            return @default;
        }
    }
}
