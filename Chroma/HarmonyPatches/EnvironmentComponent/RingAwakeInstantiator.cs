using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BSIPA_Utilities;
using HarmonyLib;
using Heck;
using UnityEngine;
using Zenject;
using Zenject.Internal;
using Object = UnityEngine.Object;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    [HeckPatch(PatchType.Environment)]
    internal static class RingAwakeInstantiator
    {
        private static readonly FieldInfo _trackLaneRingPrefab = AccessTools.Field(typeof(TrackLaneRingsManager), "_trackLaneRingPrefab");
        private static readonly MethodInfo _queueInject = AccessTools.Method(typeof(RingAwakeInstantiator), nameof(QueueInject));
        private static readonly FieldAccessor<TrackLaneRingsManager, DiContainer>.Accessor _containerAccessor =
            FieldAccessor<TrackLaneRingsManager, DiContainer>.GetAccessor("_container");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneDecoratorContext), nameof(SceneDecoratorContext.Initialize))]
        private static void InitManagers(SceneDecoratorContext __instance, DiContainer container)
        {
            if (__instance.DecoratedContractName != "Environment")
            {
                return;
            }

            List<TrackLaneRingsManager> managers = new(1);
            __instance.gameObject.scene.GetRootGameObjects().Do(n => FindTrackLaneRingManager(n.transform, managers));
            foreach (TrackLaneRingsManager manager in managers)
            {
                if (manager == null)
                {
                    continue;
                }

                TrackLaneRingsManager managerref = manager;
                _containerAccessor(ref managerref) = container;
                manager.Start();
            }
        }

        private static void FindTrackLaneRingManager(Transform transform, List<TrackLaneRingsManager> managers)
        {
            managers.Add(transform.GetComponent<TrackLaneRingsManager>());

            foreach (Transform child in transform)
            {
                FindTrackLaneRingManager(child, managers);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TrackLaneRingsManager), nameof(TrackLaneRingsManager.Start))]
        private static bool InitOnce(TrackLaneRing[]? ____rings)
        {
            return ____rings == null;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(TrackLaneRingsManager), nameof(TrackLaneRingsManager.Start))]
        private static IEnumerable<CodeInstruction> QueueInjectTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
             * -- this._rings[i] = this._container.InstantiatePrefabForComponent<TrackLaneRing>(this._trackLaneRingPrefab);
             * ++ this._rings[i] = QueueInject(this._container, this._trackLaneRingPrefab);
             */
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _trackLaneRingPrefab))
                .Repeat(n => n
                    .Advance(1)
                    .Set(OpCodes.Call, _queueInject))
                .InstructionEnumeration();
        }

        private static TrackLaneRing QueueInject(DiContainer container, TrackLaneRing prefab)
        {
            TrackLaneRing trackLaneRing = Object.Instantiate(prefab);
            List<MonoBehaviour> injectables = new();
            ZenUtilInternal.GetInjectableMonoBehavioursUnderGameObject(trackLaneRing.gameObject, injectables);
            injectables.ForEach(container.QueueForInject);
            return trackLaneRing;
        }
    }
}
