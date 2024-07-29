using System.Reflection;
using Heck.Deserialize;
using Heck.Patcher;

namespace Heck.Module
{
    internal interface IModuleFeature
    {
    }

    internal class CallbackModuleFeature : IModuleFeature
    {
        internal CallbackModuleFeature(MethodInfo method)
        {
            Method = method;
        }

        internal MethodInfo Method { get; }
    }

    internal class ConditionModuleFeature : IModuleFeature
    {
        internal ConditionModuleFeature(MethodInfo method)
        {
            Method = method;
        }

        internal MethodInfo Method { get; }
    }

    internal class PatcherModuleFeature : IModuleFeature
    {
        internal PatcherModuleFeature(HeckPatcher patcher)
        {
            Patcher = patcher;
        }

        internal HeckPatcher Patcher { get; }
    }

    internal class DeserializerModuleFeature : IModuleFeature
    {
        internal DeserializerModuleFeature(DataDeserializer dataDeserializer)
        {
            DataDeserializer = dataDeserializer;
        }

        internal DataDeserializer DataDeserializer { get; }
    }
}
