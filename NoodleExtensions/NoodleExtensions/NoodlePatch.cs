namespace NoodleExtensions
{
    using System;
    using System.Reflection;

    internal struct NoodlePatchData
    {
        internal NoodlePatchData(MethodInfo orig, MethodInfo pre, MethodInfo post, MethodInfo tran)
        {
            OriginalMethod = orig;
            Prefix = pre;
            Postfix = post;
            Transpiler = tran;
        }

        internal MethodInfo OriginalMethod { get; }

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

        internal NoodlePatch(Type declaringType, string methodName)
        {
            DeclaringType = declaringType;
            MethodName = methodName;
        }

        internal Type DeclaringType { get; }

        internal string MethodName { get; }

        internal Type[] Parameters { get; }
    }
}
