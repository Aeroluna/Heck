namespace NoodleExtensions
{
    using System;
    using System.Reflection;
    using HarmonyLib;

    internal struct NoodlePatchData
    {
        internal NoodlePatchData(MethodBase orig, MethodInfo pre, MethodInfo post, MethodInfo tran)
        {
            OriginalMethod = orig;
            Prefix = pre;
            Postfix = post;
            Transpiler = tran;
        }

        internal MethodBase OriginalMethod { get; }

        internal MethodInfo Prefix { get; }

        internal MethodInfo Postfix { get; }

        internal MethodInfo Transpiler { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class NoodlePatch : Attribute
    {
        internal NoodlePatch(Type declaringType)
        {
            DeclaringType = declaringType;
        }

        internal NoodlePatch(string methodName)
        {
            MethodName = methodName;
        }

        internal NoodlePatch(Type[] parameters)
        {
            Parameters = parameters;
        }

        internal NoodlePatch(MethodType methodType)
        {
            MethodType = methodType;
        }

        internal NoodlePatch(Type declaringType, string methodName)
        {
            DeclaringType = declaringType;
            MethodName = methodName;
        }

        internal Type DeclaringType { get; }

        internal string MethodName { get; }

        internal Type[] Parameters { get; }

        internal MethodType? MethodType { get; }
    }
}
