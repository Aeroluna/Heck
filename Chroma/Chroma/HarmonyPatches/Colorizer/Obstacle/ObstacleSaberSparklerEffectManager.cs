namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ObstacleSaberSparkleEffectManager))]
    [HarmonyPatch("Update")]
    internal static class ObstacleSaberSparkleEffectManagerUpdate
    {
        private static readonly MethodInfo _setObstacleSaberSparklerColor = SymbolExtensions.GetMethodInfo(() => SetObstacleSaberSparkleColor(null, null));
        private static readonly FieldInfo _effectsField = AccessTools.Field(typeof(ObstacleSaberSparkleEffectManager), "_effects");

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundPosition &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "SetPositionAndRotation")
                {
                    foundPosition = true;

                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _effectsField),
                        new CodeInstruction(OpCodes.Ldloc_3),
                        new CodeInstruction(OpCodes.Ldelem_Ref),
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Call, _setObstacleSaberSparklerColor),
                    };
                    instructionList.InsertRange(i + 1, codeInstructions);
                }
            }

            if (!foundPosition)
            {
                Plugin.Logger.Log("Failed to find callvirt to SetPositionAndRotation!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static void SetObstacleSaberSparkleColor(ObstacleSaberSparkleEffect obstacleSaberSparkleEffect, ObstacleController obstacleController)
        {
            if (obstacleController.TryGetObstacleColorizer(out ObstacleColorizer obstacleColorizer))
            {
                Color.RGBToHSV(obstacleColorizer.Color, out float h, out float s, out _);
                obstacleSaberSparkleEffect.color = Color.HSVToRGB(h, s, 1);
            }
        }
    }
}
