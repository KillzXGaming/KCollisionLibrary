using System;
using System.Numerics;

namespace KclLibrary
{
    /// <summary>
    /// Represents a collection of mathematical functions.
    /// </summary>
    internal static class Maths
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the next power of 2 which results in a value bigger than or equal to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to which the next power of 2 will be determined.</param>
        /// <returns>The next power of resulting in a value bigger than or equal to the given value.</returns>
        internal static int GetNext2Exponent(float value)
        {
            if (value <= 1) return 0;
            return (int)Math.Ceiling(Math.Log(value, 2));
        }
    }
}
