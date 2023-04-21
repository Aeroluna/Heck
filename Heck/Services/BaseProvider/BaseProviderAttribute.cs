using System;
using JetBrains.Annotations;

namespace Heck.BaseProvider
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Property)]
    public class BaseProviderAttribute : Attribute
    {
        public BaseProviderAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
