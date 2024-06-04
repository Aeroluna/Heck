using System;
using JetBrains.Annotations;

namespace Heck
{
    // really wish the annotations could be inherited...
    public abstract class AttributeWithId : Attribute
    {
        protected AttributeWithId(object? id = null)
        {
            Id = id;
        }

        internal object? Id { get; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class HeckPatch : AttributeWithId
    {
        public HeckPatch(object? id = null)
            : base(id)
        {
        }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class Module : Attribute
    {
        public Module(
            string id,
            int priority,
            LoadType loadType,
            string[]? depends = null,
            string[]? conflict = null)
        {
            Id = id;
            Priority = priority;
            LoadType = loadType;
            Depends = depends;
            Conflict = conflict;
        }

        public string Id { get; }

        public int Priority { get; }

        public LoadType LoadType { get; }

        public string[]? Depends { get; }

        public string[]? Conflict { get; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleCallback : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleCondition : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class ModulePatcher : Attribute
    {
        public ModulePatcher(string harmonyId, object? id)
        {
            HarmonyId = harmonyId;
            Id = id;
        }

        public string HarmonyId { get; }

        public object? Id { get; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleDataDeserializer : Attribute
    {
        public ModuleDataDeserializer(string id, Type type)
        {
            Id = id;
            Type = type;
        }

        public string Id { get; }

        public Type Type { get; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class EarlyDeserializer : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomEventsDeserializer : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class EventsDeserializer : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ObjectsDeserializer : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PlayViewControllerSettings : Attribute
    {
        public PlayViewControllerSettings(int priority, string title)
        {
            Priority = priority;
            Title = title;
        }

        internal int Priority { get; }

        internal string Title { get; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomEvent : Attribute
    {
        public CustomEvent(params string[] type)
        {
            Type = type;
        }

        internal string[] Type { get; }
    }
}
