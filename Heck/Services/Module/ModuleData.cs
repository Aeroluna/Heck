using System.Linq;
using JetBrains.Annotations;

namespace Heck.Module
{
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

    public interface IModule
    {
    }

    public readonly struct Capabilities
    {
        public Capabilities(string[] requirements, string[] suggestions)
        {
            Requirements = requirements;
            Suggestions = suggestions;
        }

        public string[] Requirements { get; }

        public string[] Suggestions { get; }
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

        public IModule Module { get; }

        public string Id { get; }

        public int Priority { get; }

        public string[] Depends { get; }

        public string[] Conflict { get; }

        public LoadType LoadType { get; }

        public IModuleFeature[] Features { get; }

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
}
