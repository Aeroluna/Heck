﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Heck.BaseProvider;

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
        opMul,
        opDiv
    }

#pragma warning restore SA1300 // Element should begin with upper-case letter

    internal abstract class Modifier<T>
        where T : struct
    {
        private readonly T? _rawPoint;
        private readonly BaseProviderData<T>? _baseProvider;

        internal Modifier(T? point, BaseProviderData<T>? baseProvider, Modifier<T>[] modifiers, Operation operation)
        {
            _rawPoint = point;
            _baseProvider = baseProvider;
            Modifiers = modifiers;
            Operation = operation;
            HasBaseProvider = baseProvider != null || modifiers.Any(n => n.HasBaseProvider);
        }

        public abstract T Point { get; }

        public Operation Operation { get; }

        public bool HasBaseProvider { get; }

        protected T OriginalPoint => _rawPoint ?? _baseProvider?.GetValue() ?? throw new InvalidOperationException();

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
