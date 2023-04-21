using System.Collections.Generic;
using System.Text;

namespace Heck.Animation
{
    // TODO: allow operations to handle different types
    // TODO: add more operations
#pragma warning disable SA1300 // Element should begin with upper-case letter

    public enum Operation
    {
        opNone = 0,
        opAdd,
        opSub,
        opMult,
        opDivide
    }

#pragma warning restore SA1300 // Element should begin with upper-case letter

    internal abstract class Modifier<T>
    {
        internal Modifier(T point, Modifier<T>[] modifiers, Operation operation)
        {
            OriginalPoint = point;
            Modifiers = modifiers;
            Operation = operation;
        }

        public abstract T Point { get; }

        public Operation Operation { get; }

        protected T OriginalPoint { get; }

        protected Modifier<T>[] Modifiers { get; }

        protected abstract string FormattedValue { get; }

        public override string ToString()
        {
            const string spacer = ", ";
            StringBuilder stringBuilder = new("[" + FormattedValue);
            if (Operation != Operation.opNone)
            {
                stringBuilder.Append(spacer + Operation);
            }

            if (Modifiers.Length > 0)
            {
                stringBuilder.Append(string.Join(spacer, (IEnumerable<object>)Modifiers));
            }

            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }
    }
}
