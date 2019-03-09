﻿// TODO: Use some suggestions from https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c and also parallelize if bandwidth is not used up by
//       a single core. Hope this works for ushort too.
// TODO: Can we allocate the count array on the stack to make sorting small arrays faster?
// TODO: Benchmark Fill() for different data types from byte (8-bits) to ulong (64-bits) to see if all of memory bandwidth can be used up. If not then go parallel.
// TODO: If Fill() benchmarks well for larger data types, then reading the array 64-bits at a time will most likely pay off as well. Yes it does! Great direction to go in.
// TODO: Consider using SIMD instructions to read and write even more bits per iteration - e.g. 256-bits is 32 bytes and 512-bits is an entire cache line.
//       https://stackoverflow.com/questions/31999479/using-simd-operation-from-c-sharp-in-net-framework-4-6-is-slower
// TODO: Make sure to mention that .NET core has a Fill method implemented already. Modify my version to have the same interface, and provide a parallel version that's even faster.
// TODO: Benchmark my Fill version on a quad-memory channel system to see how much bandwidth it provides - the fill rate! ;-) kinda like graphics.
// TODO: Implement serial Fill of byte, ushort and uint using ulong to accelerate it.
// TODO: Figure out how to make accelerated Fill generic, such as done for .NET core 2.X
// TODO: Change SSE Fill to look at alignment of the buffer first and do scalar up to 32-byte alignment and then do SSE - otherwise performance is abysmal.
// TODO: Something strange is going on with performance of BlockCopy for ushort when the offset is not zero. I'm guessing that Microsoft messed up the implementation and
//       if they use SSE instructions, then forgot to align these on 32-byte/256-bit boundary, and when SSE is not aligned then performance is abysmal.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static void Fill<T>(this T[] arrayToFill, T value)
        {
            for (int i = 0; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

        public static void Fill<T>(this T[] arrayToFill, T value, int startIndex, int length)
        {
            int index = startIndex;
            int endIndex = startIndex + length;
            for (int i = startIndex; i < endIndex; i++)
                arrayToFill[i] = value;
        }

        public static void FillUsingArrayCopy<T>(this T[] array, T value) where T : struct
        {
            int numBytesInItem = 0;
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                numBytesInItem = 1;
            else if (typeof(T) == typeof(ushort) || typeof(T) == typeof(short))
                numBytesInItem = 2;
            else if (typeof(T) == typeof(uint) || typeof(T) == typeof(int))
                numBytesInItem = 4;
            else if (typeof(T) == typeof(ulong) || typeof(T) == typeof(long))
                numBytesInItem = 8;
            else
                throw new ArgumentException(string.Format("Type '{0}' is unsupported.", typeof(T).ToString()));

            int block = 32, index = 0;
            int endIndex = Math.Min(block, array.Length);

            while (index < endIndex)          // Fill the initial block
                array[index++] = value;

            endIndex = array.Length;
            for (; index < endIndex; index += block, block *= 2)
            {
                int actualBlockSize = Math.Min(block, endIndex - index);
                Array.Copy(array, 0, array, index, actualBlockSize);
            }
        }

        public static void FillUsingArrayCopy2<T>(this T[] destinationArray, T value) where T : struct
        {
            //int FindMostSignificantBit(int length)
            //{
            //    int mostSignificantBit = 0;
            //    while ((length >>= 1) > 0)
            //    {
            //        mostSignificantBit++;
            //    }

            //    return mostSignificantBit;
            //}

            //int bit = 5;
            //int index = 0;
            //int endIndex = Math.Min(1 << bit, array.Length);

            //while (index < endIndex)
            //    array[index++] = value;

            //if (endIndex < array.Length)
            //{
            //    int msb = FindMostSignificantBit(array.Length);
            //    int blockSize = 1 << bit;
            //    for (; bit <= msb; bit++)
            //    {
            //        Array.Copy(array, 0, array, index, blockSize);
            //        index += blockSize;
            //        blockSize *= 2;
            //    }

            //    // Fill leftovers;
            //    int leftover = array.Length - blockSize;
            //    Array.Copy(array, 0, array, index, leftover);
            //}

            if (destinationArray == null)
            {
                throw new ArgumentNullException("destinationArray");
            }

             // set the initial array value
             destinationArray[0] = value;
             
            int arrayToFillHalfLength = destinationArray.Length / 2;
            int copyLength;

            for (copyLength = 1; copyLength < arrayToFillHalfLength; copyLength *= 2)
            {
                Array.Copy(destinationArray, 0, destinationArray, copyLength, copyLength);
            }

            Array.Copy(destinationArray, 0, destinationArray, copyLength, destinationArray.Length - copyLength);
        }

        // From StackOverflow fast fill question https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c
        public static void FillUsingBlockCopy<T>(this T[] array, T value) where T : struct
        {
            int numBytesInItem = 0;
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                numBytesInItem = 1;
            else if (typeof(T) == typeof(ushort) || typeof(T) == typeof(short))
                numBytesInItem = 2;
            else if (typeof(T) == typeof(uint) || typeof(T) == typeof(int))
                numBytesInItem = 4;
            else if (typeof(T) == typeof(ulong) || typeof(T) == typeof(long))
                numBytesInItem = 8;
            else
                throw new ArgumentException($"Type '{typeof(T)}' is unsupported.");

            int block = 32, index = 0;
            int endIndex = Math.Min(block, array.Length);

            while (index < endIndex)          // Fill the initial block
                array[index++] = value;

            endIndex = array.Length;
            for (; index < endIndex; index += block, block *= 2)
            {
                int actualBlockSize = Math.Min(block, endIndex - index);
                Buffer.BlockCopy(array, 0, array, index * numBytesInItem, actualBlockSize * numBytesInItem);
            }
        }

        public static void FillUsingBlockCopy<T>(this T[] array, T value, int startIndex, int count) where T : struct
        {
            int numBytesInItem = 0;
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                numBytesInItem = 1;
            else if (typeof(T) == typeof(ushort) || typeof(T) == typeof(short))
                numBytesInItem = 2;
            else if (typeof(T) == typeof(uint) || typeof(T) == typeof(int))
                numBytesInItem = 4;
            else if (typeof(T) == typeof(ulong) || typeof(T) == typeof(long))
                numBytesInItem = 8;
            else
                throw new ArgumentException(string.Format("Type '{0}' is unsupported.", typeof(T).ToString()));

            int block = 32, index = startIndex;
            int endIndex = startIndex + Math.Min(block, count);

            while (index < endIndex)          // Fill the initial block
                array[index++] = value;

            endIndex = startIndex + count;
            for (; index < endIndex; index += block, block *= 2)
            {
                int actualBlockSize = Math.Min(block, endIndex - index);
                Buffer.BlockCopy(array, startIndex * numBytesInItem, array, index * numBytesInItem, actualBlockSize * numBytesInItem);
            }
        }
    }
}
