using System;
using Heck.Module;
using JetBrains.Annotations;

namespace Heck;

// really wish the annotations could be inherited...
public abstract class AttributeWithId(object? id = null) : Attribute
{
    internal object? Id { get; } = id;
}

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class HeckPatchAttribute(object? id = null) : AttributeWithId(id);

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleAttribute(
    string id,
    int priority,
    LoadType loadType,
    string[]? depends = null,
    string[]? conflict = null)
    : Attribute
{
    public string[]? Conflict { get; } = conflict;

    public string[]? Depends { get; } = depends;

    public string Id { get; } = id;

    public LoadType LoadType { get; } = loadType;

    public int Priority { get; } = priority;
}

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class ModuleCallbackAttribute : Attribute;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class ModuleConditionAttribute : Attribute;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class ModulePatcherAttribute(string harmonyId, object? id) : Attribute
{
    public string HarmonyId { get; } = harmonyId;

    public object? Id { get; } = id;
}

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class ModuleDataDeserializerAttribute(string id, Type type) : Attribute
{
    public string Id { get; } = id;

    public Type Type { get; } = type;
}

[AttributeUsage(AttributeTargets.Class)]
public class PlayViewControllerSettingsAttribute(int priority, string title) : Attribute
{
    internal int Priority { get; } = priority;

    internal string Title { get; } = title;
}

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class CustomEventAttribute(params string[] type) : Attribute
{
    internal string[] Type { get; } = type;
}
