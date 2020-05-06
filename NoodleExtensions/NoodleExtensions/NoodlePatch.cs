using System;
using System.Reflection;

namespace NoodleExtensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class NoodlePatch : Attribute
    {
        internal Type declaringType { get; }
        internal string methodName { get; }

        internal NoodlePatch(Type declaringType)
        {
            this.declaringType = declaringType;
        }

        internal NoodlePatch(string methodName)
        {
            this.methodName = methodName;
        }

        internal NoodlePatch(Type declaringType, string methodName)
        {
            this.declaringType = declaringType;
            this.methodName = methodName;
        }
    }

    internal struct NoodlePatchData
    {
        internal NoodlePatchData(MethodInfo orig, MethodInfo pre, MethodInfo post, MethodInfo tran)
        {
            originalMethod = orig;
            prefix = pre;
            postfix = post;
            transpiler = tran;
        }

        internal MethodInfo originalMethod { get; }
        internal MethodInfo prefix { get; }
        internal MethodInfo postfix { get; }
        internal MethodInfo transpiler { get; }
    }
}