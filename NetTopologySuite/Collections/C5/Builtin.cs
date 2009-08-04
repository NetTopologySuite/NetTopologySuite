/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/
using System;
using System.Collections.Generic;
using SCG = System.Collections.Generic;

namespace C5
{

    #region char comparer and equality comparer

    internal class CharComparer : IComparer<char>
    {
        #region IComparer<char> Members

        public int Compare(char item1, char item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type char, also known as System.Char.
    /// </summary>
    public class CharEqualityComparer : IEqualityComparer<char>
    {
        private static CharEqualityComparer cached = new CharEqualityComparer();

        private CharEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static CharEqualityComparer Default
        {
            get { return cached ?? (cached = new CharEqualityComparer()); }
        }

        #region IEqualityComparer<char> Members

        /// <summary>
        /// Get the hash code of this char
        /// </summary>
        /// <param name="item">The char</param>
        /// <returns>The same</returns>
        public int GetHashCode(char item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Check if two chars are equal
        /// </summary>
        /// <param name="item1">first char</param>
        /// <param name="item2">second char</param>
        /// <returns>True if equal</returns>
        public bool Equals(char item1, char item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region sbyte comparer and equality comparer

    [Serializable]
    internal class SByteComparer : IComparer<sbyte>
    {
        #region IComparer<sbyte> Members

        [Tested]
        public int Compare(sbyte item1, sbyte item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type sbyte, also known as System.SByte. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.SByteEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class SByteEqualityComparer : IEqualityComparer<sbyte>
    {
        private static SByteEqualityComparer cached;

        private SByteEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static SByteEqualityComparer Default
        {
            get { return cached ?? (cached = new SByteEqualityComparer()); }
        }

        #region IEqualityComparer<sbyte> Members

        /// <summary>
        /// Get the hash code of this sbyte, that is, itself
        /// </summary>
        /// <param name="item">The sbyte</param>
        /// <returns>The same</returns>
        [Tested]
        public int GetHashCode(sbyte item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Determine whether two sbytes are equal
        /// </summary>
        /// <param name="item1">first sbyte</param>
        /// <param name="item2">second sbyte</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(sbyte item1, sbyte item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region byte comparer and equality comparer

    internal class ByteComparer : IComparer<byte>
    {
        #region IComparer<byte> Members

        public int Compare(byte item1, byte item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type byte, also known as System.Byte.
    /// <para>This class is a singleton and the instance can be accessed
    /// via the <see cref="P:C5.ByteEqualityComparer.Default"/> property</para>
    /// </summary>
    public class ByteEqualityComparer : IEqualityComparer<byte>
    {
        private static ByteEqualityComparer cached = new ByteEqualityComparer();

        private ByteEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static ByteEqualityComparer Default
        {
            get { return cached ?? (cached = new ByteEqualityComparer()); }
        }

        #region IEqualityComparer<byte> Members

        /// <summary>
        /// Get the hash code of this byte, i.e. itself
        /// </summary>
        /// <param name="item">The byte</param>
        /// <returns>The same</returns>
        public int GetHashCode(byte item)
        {
            return item.GetHashCode();
        }

        /// <summary>
        /// Check if two bytes are equal
        /// </summary>
        /// <param name="item1">first byte</param>
        /// <param name="item2">second byte</param>
        /// <returns>True if equal</returns>
        public bool Equals(byte item1, byte item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region short comparer and equality comparer

    [Serializable]
    internal class ShortComparer : IComparer<short>
    {
        #region IComparer<short> Members

        [Tested]
        public int Compare(short item1, short item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type short, also known as System.Int16. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.ShortEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class ShortEqualityComparer : IEqualityComparer<short>
    {
        private static ShortEqualityComparer cached;

        private ShortEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static ShortEqualityComparer Default
        {
            get { return cached ?? (cached = new ShortEqualityComparer()); }
        }

        #region IEqualityComparer<short> Members

        /// <summary>
        /// Get the hash code of this short, that is, itself
        /// </summary>
        /// <param name="item">The short</param>
        /// <returns>The same</returns>
        [Tested]
        public int GetHashCode(short item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Determine whether two shorts are equal
        /// </summary>
        /// <param name="item1">first short</param>
        /// <param name="item2">second short</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(short item1, short item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region ushort comparer and equality comparer

    [Serializable]
    internal class UShortComparer : IComparer<ushort>
    {
        #region IComparer<ushort> Members

        [Tested]
        public int Compare(ushort item1, ushort item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type ushort, also known as System.UInt16. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.UShortEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class UShortEqualityComparer : IEqualityComparer<ushort>
    {
        private static UShortEqualityComparer cached;

        private UShortEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static UShortEqualityComparer Default
        {
            get { return cached ?? (cached = new UShortEqualityComparer()); }
        }

        #region IEqualityComparer<ushort> Members

        /// <summary>
        /// Get the hash code of this ushort, that is, itself
        /// </summary>
        /// <param name="item">The ushort</param>
        /// <returns>The same</returns>
        [Tested]
        public int GetHashCode(ushort item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Determine whether two ushorts are equal
        /// </summary>
        /// <param name="item1">first ushort</param>
        /// <param name="item2">second ushort</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(ushort item1, ushort item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region int comparer and equality comparer

    [Serializable]
    internal class IntComparer : IComparer<int>
    {
        #region IComparer<int> Members

        [Tested]
        public int Compare(int item1, int item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type int, also known as System.Int32. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.IntEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class IntEqualityComparer : IEqualityComparer<int>
    {
        private static IntEqualityComparer cached;

        private IntEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static IntEqualityComparer Default
        {
            get { return cached ?? (cached = new IntEqualityComparer()); }
        }

        #region IEqualityComparer<int> Members

        /// <summary>
        /// Get the hash code of this integer, that is, itself
        /// </summary>
        /// <param name="item">The integer</param>
        /// <returns>The same</returns>
        [Tested]
        public int GetHashCode(int item)
        {
            return item;
        }


        /// <summary>
        /// Determine whether two integers are equal
        /// </summary>
        /// <param name="item1">first integer</param>
        /// <param name="item2">second integer</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(int item1, int item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region uint comparer and equality comparer

    [Serializable]
    internal class UIntComparer : IComparer<uint>
    {
        #region IComparer<uint> Members

        [Tested]
        public int Compare(uint item1, uint item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type uint, also known as System.UInt32. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.UIntEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class UIntEqualityComparer : IEqualityComparer<uint>
    {
        private static UIntEqualityComparer cached;

        private UIntEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static UIntEqualityComparer Default
        {
            get { return cached ?? (cached = new UIntEqualityComparer()); }
        }

        #region IEqualityComparer<uint> Members

        /// <summary>
        /// Get the hash code of this unsigned integer
        /// </summary>
        /// <param name="item">The integer</param>
        /// <returns>The same bit pattern as a signed integer</returns>
        [Tested]
        public int GetHashCode(uint item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Determine whether two unsigned integers are equal
        /// </summary>
        /// <param name="item1">first unsigned integer</param>
        /// <param name="item2">second unsigned integer</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(uint item1, uint item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region long comparer and equality comparer

    [Serializable]
    internal class LongComparer : IComparer<long>
    {
        #region IComparer<long> Members

        [Tested]
        public int Compare(long item1, long item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type long, also known as System.Int64. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.LongEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class LongEqualityComparer : IEqualityComparer<long>
    {
        private static LongEqualityComparer cached;

        private LongEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static LongEqualityComparer Default
        {
            get { return cached ?? (cached = new LongEqualityComparer()); }
        }

        #region IEqualityComparer<long> Members

        /// <summary>
        /// Get the hash code of this long integer
        /// </summary>
        /// <param name="item">The long integer</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(long item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Determine whether two long integers are equal
        /// </summary>
        /// <param name="item1">first long integer</param>
        /// <param name="item2">second long integer</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(long item1, long item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region ulong comparer and equality comparer

    [Serializable]
    internal class ULongComparer : IComparer<ulong>
    {
        #region IComparer<ulong> Members

        [Tested]
        public int Compare(ulong item1, ulong item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type uint, also known as System.UInt64. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.ULongEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class ULongEqualityComparer : IEqualityComparer<ulong>
    {
        private static ULongEqualityComparer cached;

        private ULongEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static ULongEqualityComparer Default
        {
            get { return cached ?? (cached = new ULongEqualityComparer()); }
        }

        #region IEqualityComparer<ulong> Members

        /// <summary>
        /// Get the hash code of this unsigned long integer
        /// </summary>
        /// <param name="item">The unsigned long integer</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(ulong item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Determine whether two unsigned long integers are equal
        /// </summary>
        /// <param name="item1">first unsigned long integer</param>
        /// <param name="item2">second unsigned long integer</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(ulong item1, ulong item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region float comparer and equality comparer

    internal class FloatComparer : IComparer<float>
    {
        #region IComparer<float> Members

        public int Compare(float item1, float item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type float, also known as System.Single. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.FloatEqualityComparer.Default"/> property</para>
    /// </summary>
    public class FloatEqualityComparer : IEqualityComparer<float>
    {
        private static FloatEqualityComparer cached;

        private FloatEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static FloatEqualityComparer Default
        {
            get { return cached ?? (cached = new FloatEqualityComparer()); }
        }

        #region IEqualityComparer<float> Members

        /// <summary>
        /// Get the hash code of this float
        /// </summary>
        /// <param name="item">The float</param>
        /// <returns>The same</returns>
        [Tested]
        public int GetHashCode(float item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Check if two floats are equal
        /// </summary>
        /// <param name="item1">first float</param>
        /// <param name="item2">second float</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(float item1, float item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region double comparer and equality comparer

    internal class DoubleComparer : IComparer<double>
    {
        #region IComparer<double> Members

        public int Compare(double item1, double item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type double, also known as System.Double.
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.DoubleEqualityComparer.Default"/> property</para>
    /// </summary>
    public class DoubleEqualityComparer : IEqualityComparer<double>
    {
        private static DoubleEqualityComparer cached;

        private DoubleEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static DoubleEqualityComparer Default
        {
            get { return cached ?? (cached = new DoubleEqualityComparer()); }
        }

        #region IEqualityComparer<double> Members

        /// <summary>
        /// Get the hash code of this double
        /// </summary>
        /// <param name="item">The double</param>
        /// <returns>The same</returns>
        [Tested]
        public int GetHashCode(double item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Check if two doubles are equal
        /// </summary>
        /// <param name="item1">first double</param>
        /// <param name="item2">second double</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(double item1, double item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion

    #region decimal comparer and equality comparer

    [Serializable]
    internal class DecimalComparer : IComparer<decimal>
    {
        #region IComparer<decimal> Members

        [Tested]
        public int Compare(decimal item1, decimal item2)
        {
            return item1 > item2 ? 1 : item1 < item2 ? -1 : 0;
        }

        #endregion
    }

    /// <summary>
    /// An equality comparer for type decimal, also known as System.Decimal. 
    /// <para>This class is a singleton and the instance can be accessed
    /// via the static <see cref="P:C5.DecimalEqualityComparer.Default"/> property</para>
    /// </summary>
    [Serializable]
    public class DecimalEqualityComparer : IEqualityComparer<decimal>
    {
        private static DecimalEqualityComparer cached;

        private DecimalEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [Tested]
        public static DecimalEqualityComparer Default
        {
            get { return cached ?? (cached = new DecimalEqualityComparer()); }
        }

        #region IEqualityComparer<decimal> Members

        /// <summary>
        /// Get the hash code of this decimal.
        /// </summary>
        /// <param name="item">The decimal</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(decimal item)
        {
            return item.GetHashCode();
        }


        /// <summary>
        /// Determine whether two decimals are equal
        /// </summary>
        /// <param name="item1">first decimal</param>
        /// <param name="item2">second decimal</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(decimal item1, decimal item2)
        {
            return item1 == item2;
        }

        #endregion
    }

    #endregion
}