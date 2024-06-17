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
    public class HeckPatchAttribute : AttributeWithId
    {
        public HeckPatchAttribute(object? id = null)
            : base(id)
        {
        }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleAttribute : Attribute
    {
        public ModuleAttribute(
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
    public class ModuleCallbackAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public class ModuleConditionAttribute : Attribute
    {
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class ModulePatcherAttribute : Attribute
    {
        public ModulePatcherAttribute(string harmonyId, object? id)
        {
            HarmonyId = harmonyId;
            Id = id;
        }

        public string HarmonyId { get; }

        public object? Id { get; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleDataDeserializerAttribute : Attribute
    {
        public ModuleDataDeserializerAttribute(string id, Type type)
        {
            Id = id;
            Type = type;
        }

        public string Id { get; }

        public Type Type { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PlayViewControllerSettingsAttribute : Attribute
    {
        public PlayViewControllerSettingsAttribute(int priority, string title)
        {
            Priority = priority;
            Title = title;
        }

        internal int Priority { get; }

        internal string Title { get; }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomEventAttribute : Attribute
    {
        public CustomEventAttribute(params string[] type)
        {
            Type = type;
        }

        internal string[] Type { get; }
    }
}
