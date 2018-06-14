using System;

namespace RTools_NTS.Util
{
    /// <summary>
    /// Buffer for characters.  This approximates StringBuilder
    /// but is designed to be faster for specific operations.
    /// This is about 30% faster for the operations I'm interested in
    /// (Append, Clear, Length, ToString).
    /// This trades off memory for speed.
    /// </summary>
    /// <remarks>
    /// <para>To make Remove from the head fast, this is implemented
    /// as a ring buffer.</para>
    /// <para>This uses head and tail indices into a fixed-size
    /// array. This will grow the array as necessary.</para>
    /// </remarks>
    public class CharBuffer
    {
        #region Fields

        int capacity = 128;
        char[] buffer;
        int headIndex;  // index of first char
        int tailIndex;  // index 1 past last char

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets the number of characters in the character buffer.
        /// Increasing the length this way provides indeterminate results.
        /// </summary>
        public int Length
        {
            get => (tailIndex - headIndex);
            set
            {
                tailIndex = headIndex + value;
                if (tailIndex >= capacity) throw new
                    IndexOutOfRangeException("Tail index greater than capacity");
            }
        }

        /// <summary>
        /// Returns the capacity of this character buffer.
        /// </summary>
        public int Capacity => (capacity);

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CharBuffer()
        {
            buffer = new char[capacity];
        }

        /// <summary>
        /// Construct with a specific capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public CharBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new char[capacity];
        }

        #endregion

        #region Non-Public Methods

        /// <summary>
        /// Reallocate the buffer to be larger. For the new size, this
        /// uses the max of the requested length and double the current
        /// capacity.
        /// This does not shift, meaning it does not change the head or
        /// tail indices.
        /// </summary>
        /// <param name="requestedLen">The new requested length.</param>
        protected void Grow(int requestedLen)
        {
            int newLen = Math.Max(capacity*2, requestedLen);
            newLen = Math.Max(newLen, 16);
            char[] newBuffer = new char[newLen];
            Array.Copy(buffer, 0, newBuffer, 0, capacity);
            buffer = newBuffer;
            capacity = newLen;
        }

        /// <summary>
        /// Ensure that we're set for the requested length by
        /// potentially growing or shifting contents.
        /// </summary>
        /// <param name="requestedLength"></param>
        protected void CheckCapacity(int requestedLength)
        {
            if (requestedLength + headIndex >= capacity)
            {
                // have to do something
                if ((requestedLength + headIndex > (capacity >> 1))
                    && (requestedLength < capacity - 1))
                {
                    // we're more than half-way through the buffer, and shifting is enough
                    // so just shift
                    ShiftToZero();
                }
                else
                {
                    // not far into buffer or shift wouldn't be enough anyway
                    Grow(0);
                }
            }
        }

        /// <summary>
        /// Move the buffer contents such that headIndex becomes 0.
        /// </summary>
        protected void ShiftToZero()
        {
            int len = Length;
            for (int i = 0; i < len; i++)
            {
                buffer[i] = buffer[i + headIndex];
            }
            headIndex = 0;
            tailIndex = len;
        }

        #endregion

        #region Public Methods and Indexer

        /// <summary>
        /// Overwrite this object's underlying buffer with the specified
        /// buffer.
        /// </summary>
        /// <param name="b">The character array.</param>
        /// <param name="len">The number of characters to consider filled
        /// in the input buffer.</param>
        public void SetBuffer(char[] b, int len)
        {
            capacity = b.Length;
            buffer = b;
            headIndex = 0;
            tailIndex = len;
        }

        /// <summary>
        /// Append a character to this buffer.
        /// </summary>
        /// <param name="c"></param>
        public void Append(char c)
        {
            if (tailIndex >= capacity) CheckCapacity(Length + 1);
            buffer[tailIndex++] = c;
        }

        /// <summary>
        /// Append a string to this buffer.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public void Append(string s)
        {
            if (s.Length + tailIndex >= capacity) CheckCapacity(Length + s.Length);
            for(int i = 0; i < s.Length; i++)
                buffer[tailIndex++] = s[i];
        }

        /// <summary>
        /// Append a string to this buffer.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public void Append(CharBuffer s)
        {
            if (s.Length + tailIndex >= capacity) CheckCapacity(Length + s.Length);
            for(int i = 0; i < s.Length; i++)
                buffer[tailIndex++] = s[i];
        }

        /// <summary>
        /// Remove a character at the specified index.
        /// </summary>
        /// <param name="i">The index of the character to remove.</param>
        /// <returns></returns>
        public void Remove(int i)
        {
            Remove(i, 1);
        }

        /// <summary>
        /// Remove a specified number of characters at the specified index.
        /// </summary>
        /// <param name="i">The index of the characters to remove.</param>
        /// <param name="n">The number of characters to remove.</param>
        public void Remove(int i, int n)
        {
            n = Math.Min(n, Length);
            if (i == 0)
            {
                headIndex += n;
            }
            else
            {
                Array.Copy(buffer, i + headIndex + n, buffer, i + headIndex,
                    tailIndex - (i + headIndex + n));
            }
        }

        /// <summary>
        /// Find the first instance of a character in the buffer, and
        /// return its index.  This returns -1 if the character is
        /// not found.
        /// </summary>
        /// <param name="c">The character to find.</param>
        /// <returns>The index of the specified character, or -1
        /// for not found.</returns>
        public int IndexOf(char c)
        {
            for (int i = headIndex; i < tailIndex; i++)
            {
                if (buffer[i] == c) return(i - headIndex);
            }
            return(-1);
        }

        /// <summary>
        /// Empty the buffer.
        /// </summary>
        public void Clear()
        {
            headIndex = 0;
            tailIndex = 0;
        }

        /// <summary>
        /// Indexer.
        /// </summary>
        public char this [int index]
        {
            get => (buffer[index + headIndex]);
            set => buffer[index + headIndex] = value;
        }

        /// <summary>
        /// Return the current contents as a string.
        /// </summary>
        /// <returns>The new string.</returns>
        public override string ToString()
        {
            return(new string(buffer, headIndex, tailIndex - headIndex));
        }

        #endregion
    }
}
