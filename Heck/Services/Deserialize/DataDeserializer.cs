using System;
using System.Collections.Generic;
using System.Reflection;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace Heck
{
    public class DataDeserializer
    {
        private readonly MethodInfo? _customEventMethod;
        private readonly MethodInfo? _beatmapEventMethod;
        private readonly MethodInfo? _beatmapObjectMethod;
        private readonly MethodInfo? _earlyMethod;

        internal DataDeserializer(object? id, IReflect type)
        {
            Id = id;

            foreach (MethodInfo method in type.GetMethods(AccessTools.allDeclared))
            {
                AccessAttribute<EarlyDeserializer>(ref _earlyMethod);
                AccessAttribute<CustomEventsDeserializer>(ref _customEventMethod);
                AccessAttribute<EventsDeserializer>(ref _beatmapEventMethod);
                AccessAttribute<ObjectsDeserializer>(ref _beatmapObjectMethod);
                continue;

                void AccessAttribute<TAttribute>(ref MethodInfo? savedMethod)
                    where TAttribute : Attribute
                {
                    if (method.GetCustomAttribute<TAttribute>() == null)
                    {
                        return;
                    }

                    savedMethod = method;
                }
            }
        }

        public bool Enabled { get; set; }

        public object? Id { get; }

        public override string ToString()
        {
            return Id?.ToString() ?? "NULL";
        }

        internal void InjectedInvokeEarly(object[] inputs)
        {
            _earlyMethod?.Invoke(null, _earlyMethod.ActualParameters(inputs));
        }

        internal Dictionary<CustomEventData, ICustomEventCustomData> InjectedInvokeCustomEvent(object[] inputs)
        {
            return InjectedInvoke<Dictionary<CustomEventData, ICustomEventCustomData>>(_customEventMethod, inputs);
        }

        internal Dictionary<BeatmapEventData, IEventCustomData> InjectedInvokeEvent(object[] inputs)
        {
            return InjectedInvoke<Dictionary<BeatmapEventData, IEventCustomData>>(_beatmapEventMethod, inputs);
        }

        internal Dictionary<BeatmapObjectData, IObjectCustomData> InjectedInvokeObject(object[] inputs)
        {
            return InjectedInvoke<Dictionary<BeatmapObjectData, IObjectCustomData>>(_beatmapObjectMethod, inputs);
        }

        private static T InjectedInvoke<T>(MethodInfo? method, object[] inputs)
            where T : new()
        {
            return (T?)method?.Invoke(null, method.ActualParameters(inputs)) ?? new T();
        }
    }
}
