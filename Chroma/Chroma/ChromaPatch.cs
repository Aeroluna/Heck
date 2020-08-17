namespace Chroma
{
    using System;
    using System.Reflection;

    internal struct ChromaPatchData
    {
        internal ChromaPatchData(MethodInfo orig, MethodInfo pre, MethodInfo post, MethodInfo tran)
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
    internal class ChromaPatch : Attribute
    {
        internal ChromaPatch(Type declaringType)
        {
            DeclaringType = declaringType;
        }

        internal ChromaPatch(string methodName)
        {
            MethodName = methodName;
        }

        internal ChromaPatch(Type declaringType, string methodName)
        {
            DeclaringType = declaringType;
            MethodName = methodName;
        }

        internal Type DeclaringType { get; }

        internal string MethodName { get; }
    }
}
