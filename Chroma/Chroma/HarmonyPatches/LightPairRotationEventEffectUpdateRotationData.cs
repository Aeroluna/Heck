using Chroma.Beatmap.Events;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using BS_Utils.Utilities;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("UpdateRotationData")]
    class LightPairRotationEventEffectUpdateRotationData {

        //Laser rotation
        static bool Prefix(LightPairRotationEventEffect __instance, ref BeatmapEventType ____eventL) {

            BeatmapEventData beatmapEventData = LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.lastLightPairRotationEventEffectData;

            // Thank you +1 Rabbit for providing this code
            // Since LightPairRotationEventEffect.RotationData is a private internal member, we need to get its type dynamically.
            Type RotationData = Type.GetType("LightPairRotationEventEffect+RotationData,MainAssembly");
            // The reflection method to get the rotation data must have its generic method created dynamically, so as to use the dynamic type.
            MethodInfo GetPrivateFieldM = typeof(ReflectionUtil).GetMethod("GetPrivateField", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(string) }, null);
            GetPrivateFieldM = GetPrivateFieldM.MakeGenericMethod(RotationData);

            string rotationName = beatmapEventData.type == ____eventL ? "_rotationDataL" : "_rotationDataR";

            var _rotationData = GetPrivateFieldM.Invoke(null, new object[] { __instance, rotationName });

            try {
                if (beatmapEventData is CustomBeatmapEventData customData && _rotationData != null) {
                    dynamic dynData = customData.customData;
                    if (dynData != null) {

                        bool? lockPosition = Trees.at(dynData, "_lockPosition");
                        if (lockPosition == null) lockPosition = false;

                        float? precisionSpeed = (float?)Trees.at(dynData, "_preciseSpeed");
                        if (precisionSpeed == null) precisionSpeed = beatmapEventData.value;

                        int? dir = (int?)Trees.at(dynData, "_direction");
                        if (dir == null) dir = -1;

                        bool rotInboard;
                        if (dir == -1) rotInboard = UnityEngine.Random.value < 0.5f;
                        else if (dir == 1) rotInboard = true;
                        else rotInboard = false;

                        //Actual lasering
                        Transform _transform = _rotationData.GetField<Transform>("transform");
                        Quaternion _startRotation = _rotationData.GetField<Quaternion>("startRotation");
                        Vector3 _rotationVector = __instance.GetPrivateField<Vector3>("_rotationVector");
                        if (precisionSpeed == 0) {
                            _rotationData.SetPrivateField("enabled", false);
                            if (!(bool)lockPosition) {
                                _transform.localRotation = _startRotation;
                            }
                        }
                        else if (!(bool)lockPosition) {
                            _transform.localRotation = _startRotation;
                            _transform.Rotate(_rotationVector, UnityEngine.Random.Range(0f, 180f), Space.Self);
                        }
                        _rotationData.SetPrivateField("enabled", true);
                        _rotationData.SetPrivateField("rotationSpeed", (float)precisionSpeed * 20f * (rotInboard ? -1f : 1f));

                        return false;
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            return true;
        }

    }

}
