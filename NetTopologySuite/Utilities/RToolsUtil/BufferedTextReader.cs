// BufferedTextReader.cs
// 
// Copyright (C) 2003 Ryan Seghers
//
// This software is provided AS IS. No warranty is granted, 
// neither expressed nor implied. USE THIS SOFTWARE AT YOUR OWN RISK.
// NO REPRESENTATION OF MERCHANTABILITY or FITNESS FOR ANY 
// PURPOSE is given.
//
// License to use this software is limited by the following terms:
// 1) This code may be used in any program, including programs developed
//    for commercial purposes, provided that this notice is included verbatim.
//    
// Also, in return for using this code, please attempt to make your fixes and
// updates available in some way, such as by sending your updates to the
// author.

using System;
using System.IO;

namespace RTools_NTS.Util
{
	/// <summary>
	/// Wraps a TextReader with buffering for speed. This is not finished,
	/// and preliminary testing indicates it isn't faster than FCL implementation.
	/// </summary>
	public class BufferedTextReader
	{
		static readonly int BlockSize = 1024;
		TextReader reader;
		char[] buffer;
		CharBuffer cb;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reader">The TextReader to wrap.</param>
		public BufferedTextReader(TextReader reader)
		{
			this.reader = reader;
			buffer = new char[BlockSize];
			cb = new CharBuffer(0);
		}

		/// <summary>
		/// Read a single character.
		/// </summary>
		/// <returns>The character read.</returns>
		public char Read()
		{
			if (cb.Length == 0)
			{
				// read from underlying reader
				int readCount = reader.Read(buffer, 0, BlockSize);
				if (readCount == 0) throw new ApplicationException("End of stream.");
				cb.SetBuffer(buffer, readCount);
			}

			char c = cb[0]; 
			cb.Remove(0); 
			return(c);
		}

		/// <summary>
		/// Close the underlying reader.
		/// </summary>
		public void Close()
		{
			reader.Close();
			cb = null;
		}
	}
}
