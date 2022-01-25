using System;
using System.Collections.Generic;
using System.Reflection;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using IPA.Loader;

namespace Heck
{
    public class CustomDataDeserializer
    {
        private MethodInfo? _customEventMethod;
        private MethodInfo? _beatmapEventMethod;
        private MethodInfo? _beatmapObjectMethod;
        private MethodInfo? _earlyMethod;

        internal CustomDataDeserializer(object? id)
        {
            Id = id;
        }

        public bool Enabled { get; set; }

        public object? Id { get; }

        public override string ToString()
        {
            return Id?.ToString() ?? "NULL";
        }

        internal void Bind<T>()
        {
            foreach (MethodInfo method in typeof(T).GetMethods(AccessTools.allDeclared))
            {
                void AccessAttribute<TAttribute>(ref MethodInfo? savedMethod)
                    where TAttribute : Attribute
                {
                    if (method.GetCustomAttribute<TAttribute>() == null)
                    {
                        return;
                    }

                    savedMethod = method;
                }

                AccessAttribute<EarlyDeserializer>(ref _earlyMethod);
                AccessAttribute<CustomEventsDeserializer>(ref _customEventMethod);
                AccessAttribute<EventsDeserializer>(ref _beatmapEventMethod);
                AccessAttribute<ObjectsDeserializer>(ref _beatmapObjectMethod);
            }
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
