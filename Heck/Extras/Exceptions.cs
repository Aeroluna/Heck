using System;

namespace Heck
{
    public class JSONNotDefinedException : Exception
    {
        public JSONNotDefinedException(string fieldName)
            : base($"[{fieldName}] required but was not defined.")
        {
        }
    }
}
