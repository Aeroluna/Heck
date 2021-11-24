using System;
using HarmonyLib;
using JetBrains.Annotations;

namespace Heck
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HeckPatch : Attribute
    {
        public HeckPatch(Type declaringType)
        {
            DeclaringType = declaringType;
        }

        public HeckPatch(string methodName)
        {
            MethodName = methodName;
        }

        public HeckPatch(Type[] parameters)
        {
            Parameters = parameters;
        }

        [PublicAPI]
        public HeckPatch(MethodType methodType)
        {
            MethodType = methodType;
        }

        [PublicAPI]
        public HeckPatch(int id)
        {
            Id = id;
        }

        public HeckPatch(Type declaringType, string methodName)
        {
            DeclaringType = declaringType;
            MethodName = methodName;
        }

        internal Type? DeclaringType { get; }

        internal string? MethodName { get; }

        internal Type[]? Parameters { get; }

        internal MethodType? MethodType { get; }

        internal int? Id { get; }
    }
}
