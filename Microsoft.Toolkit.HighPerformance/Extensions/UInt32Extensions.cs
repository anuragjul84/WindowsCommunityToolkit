﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Microsoft.Toolkit.HighPerformance.Extensions
{
    /// <summary>
    /// Helpers for working with the <see cref="uint"/> type.
    /// </summary>
    public static class UInt32Extensions
    {
        /// <summary>
        /// Checks whether or not a given bit is set.
        /// </summary>
        /// <param name="value">The input <see cref="uint"/> value.</param>
        /// <param name="n">The position of the bit to check.</param>
        /// <returns>Whether or not the n-th bit is set.</returns>
        /// <remarks>
        /// This method doesn't validate <paramref name="n"/> against the valid range.
        /// If the parameter is not valid, the result will just be inconsistent.
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlag(this uint value, int n)
        {
            // Read the n-th bit, downcast to byte
            byte flag = (byte)((value >> n) & 1);

            /* Reinterpret the byte to avoid the test, setnz and
             * movzx instructions (asm x64). This is because the JIT
             * compiler is able to optimize this reinterpret-cast as
             * a single "and eax, 0x1" instruction, whereas if we had
             * compared the previous operation against 0, the assembly
             * would have had to perform the test, then set the non-zero
             * flag and then extend the (byte) result to eax. */
            return Unsafe.As<byte, bool>(ref flag);
        }

        /// <summary>
        /// Sets a bit to a specified value.
        /// </summary>
        /// <param name="value">The target <see cref="uint"/> value to modify.</param>
        /// <param name="n">The position of the bit to set or clear.</param>
        /// <param name="flag">The value to assign to the target bit.</param>
        /// <returns>An <see cref="uint"/> value equal to <paramref name="value"/> except for the <paramref name="n"/>-th bit.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetFlag(this uint value, int n, bool flag)
        {
            // Reinterpret the flag as 1 or 0, and cast to uint
            uint flag32 = Unsafe.As<bool, byte>(ref flag);

            /* Set the n-th bit to the input flag value.
             * The left operand XORs the input value with a mask of
             * all 1-s or 0-s depending on the input flag. The single
             * target bit is then isolated with the left shift, and
             * the combined value is XORed with the target value, setting
             * the n-th bit to the requested flag. This allows the whole
             * operation to avoid conditional branches in all cases. */
            return value ^ unchecked((uint)((-flag32 ^ value) & (1 << n)));
        }
    }
}