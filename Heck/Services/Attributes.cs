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
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleCondition : AttributeWithId
    {
        public ModuleCondition(object? id = null)
            : base(id)
        {
        }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleCallback : AttributeWithId
    {
        public ModuleCallback(object? id = null)
            : base(id)
        {
        }
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
