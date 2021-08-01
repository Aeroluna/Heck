namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ObstacleSaberSparkleEffectManager))]
    [HarmonyPatch("Update")]
    internal static class ObstacleSaberSparkleEffectManagerUpdate
    {
        private static readonly MethodInfo _setPositionAndRotation = AccessTools.Method(typeof(ObstacleSaberSparkleEffect), nameof(ObstacleSaberSparkleEffect.SetPositionAndRotation));

        private static readonly MethodInfo _setObstacleSaberSparklerColor = AccessTools.Method(typeof(ObstacleSaberSparkleEffectManagerUpdate), nameof(SetObstacleSaberSparkleColor));
        private static readonly FieldInfo _effectsField = AccessTools.Field(typeof(ObstacleSaberSparkleEffectManager), "_effects");

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _setPositionAndRotation))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _effectsField),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldelem_Ref),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, _setObstacleSaberSparklerColor))
                .InstructionEnumeration();
        }

        private static void SetObstacleSaberSparkleColor(ObstacleSaberSparkleEffect obstacleSaberSparkleEffect, ObstacleController obstacleController)
        {
            Color.RGBToHSV(obstacleController.GetObstacleColorizer().Color, out float h, out float s, out _);
            obstacleSaberSparkleEffect.color = Color.HSVToRGB(h, s, 1);
        }
    }
}
