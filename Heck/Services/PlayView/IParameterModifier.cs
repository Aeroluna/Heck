using System;

namespace Heck.PlayView
{
    public interface IParameterModifier
    {
        public event Action<StartStandardLevelParameters>? ParametersModified;
    }
}
