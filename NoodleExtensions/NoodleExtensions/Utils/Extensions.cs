﻿using System;

namespace NoodleExtensions
{
    internal static class Extensions
    {
        /// <summary>
        ///     An object extension method that converts the @this to a nullable float.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a float?</returns>
        internal static float? ToNullableFloat(this object @this)
        {
            if (@this == null || @this == DBNull.Value) return null;
            return Convert.ToSingle(@this);
        }
    }
}