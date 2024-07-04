#if !LATEST
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Chroma.Colorizer;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer
{
    internal class ObstacleEffectsColorize : IAffinity, IDisposable
    {
        private static readonly MethodInfo _setPositionAndRotation = AccessTools.Method(typeof(ObstacleSaberSparkleEffect), nameof(ObstacleSaberSparkleEffect.SetPositionAndRotation));

        private static readonly FieldInfo _effectsField = AccessTools.Field(typeof(ObstacleSaberSparkleEffectManager), nameof(ObstacleSaberSparkleEffectManager._effects));

        private readonly CodeInstruction _setObstacleSparksColor;
        private readonly ObstacleColorizerManager _manager;

        private ObstacleEffectsColorize(ObstacleColorizerManager manager)
        {
            _manager = manager;
            _setObstacleSparksColor = InstanceTranspilers.EmitInstanceDelegate<Action<ObstacleSaberSparkleEffect, ObstacleController>>(SetObstacleSaberSparkleColor);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_setObstacleSparksColor);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(ObstacleSaberSparkleEffectManager), nameof(ObstacleSaberSparkleEffectManager.Update))]
        private IEnumerable<CodeInstruction> SetObstacleSparksColorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * this._effects[i].SetPositionAndRotation(vector, this.GetEffectRotation(vector, obstacleController.transform, bounds));
                 * ++ SetObstacleSaberSparkleColor(this._effects[i], obstacleController);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _setPositionAndRotation))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _effectsField),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldelem_Ref),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    _setObstacleSparksColor)
                .InstructionEnumeration();
        }

        private void SetObstacleSaberSparkleColor(ObstacleSaberSparkleEffect obstacleSaberSparkleEffect, ObstacleController obstacleController)
        {
            Color.RGBToHSV(_manager.GetColorizer(obstacleController).Color, out float h, out float s, out _);
            obstacleSaberSparkleEffect.color = Color.HSVToRGB(h, s, 1);
        }
    }
}
#endif
