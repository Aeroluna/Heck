using System.Linq;
using JetBrains.Annotations;

namespace Heck.Module;

public enum LoadType
{
    Passive,
    Active
}

[PublicAPI]
public enum LevelType
{
    Mission,
    Multiplayer,
    Standard,
    Tutorial
}

public interface IModule;

public readonly struct Capabilities(string[] requirements, string[] suggestions)
{
    public string[] Requirements { get; } = requirements;

    public string[] Suggestions { get; } = suggestions;
}

internal class ModuleData
{
    internal ModuleData(
        IModule module,
        string id,
        int priority,
        LoadType loadType,
        string[] depends,
        string[] conflict,
        IModuleFeature[] features)
    {
        Module = module;
        Id = id;
        Priority = priority;
        LoadType = loadType;
        Depends = depends;
        Conflict = conflict;
        Features = features;
    }

    public string[] Conflict { get; }

    public string[] Depends { get; }

    public IModuleFeature[] Features { get; }

    public string Id { get; }

    public LoadType LoadType { get; }

    public IModule Module { get; }

    public int Priority { get; }

    public override string ToString()
    {
        return Id;
    }

    internal T? GetFeature<T>()
        where T : IModuleFeature
    {
        return (T?)Features.FirstOrDefault(n => n is T);
    }
}
