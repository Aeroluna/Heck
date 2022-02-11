using System.Reflection;
using JetBrains.Annotations;

namespace Heck
{
    public enum RequirementType
    {
        None,
        Condition,
        Always
    }

    [PublicAPI]
    public enum LevelType
    {
        Mission,
        Multiplayer,
        Standard,
        Tutorial
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

    public class Module
    {
        internal Module(
            string id,
            int priority,
            MethodInfo callback,
            RequirementType requirementType,
            MethodInfo? conditionCallback,
            string[] depends,
            string[] conflict)
        {
            Id = id;
            Priority = priority;
            RequirementType = requirementType;
            Callback = callback;
            ConditionCallback = conditionCallback;
            Depends = depends;
            Conflict = conflict;
        }

        public string Id { get; }

        public int Priority { get; }

        public string[] Depends { get; }

        public string[] Conflict { get; }

        public RequirementType RequirementType { get; }

        public MethodInfo Callback { get; }

        public MethodInfo? ConditionCallback { get; }

        public bool Enabled { get; set; } = true;

        public override string ToString()
        {
            return Id;
        }
    }
}
