// ParseUtil.cs
// 
// Copyright (C) 2002-2004 Ryan Seghers
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
//
using System;
using System.Collections;

namespace RTools_NTS.Util
{
	/// <summary>
	/// A start on some utility methods for parsing in conjunction with
	/// StreamTokenizer.  These currently use Token[] but could be adapted
	/// for ArrayList.
	/// </summary>
	public class ParseUtil
	{
		/// <summary>
		/// Build an Array of a particular type from a list of tokens.  
		/// The Type must be one that can be built with Convert.ChangeType.
		/// There are various ways to specify how many elements to parse.
		/// WARNING: This will throw an exception if any tokens cannot be
		/// converted.
		/// </summary>
		/// <param name="tokens">The ArrayList of tokens.</param>
		/// <param name="i">The starting (and ending) index.  This is
		/// modified, and left pointing at the last used token.</param>
		/// <param name="type">The Type of the array elements.</param>
		/// <param name="endToken">An optional end Token to look for.
		/// Parsing stops when a token equal to this is found.
		/// If this is null, then it is not used.</param>
		/// <param name="maxLength">The maximum number of array elements
		/// to parse.  If this is negative, then it is not used.</param>
		/// <param name="log">A Logger to use for messages.</param>
		/// <returns>The Array, or null for error.</returns>
		public static Array BuildArray(ArrayList tokens, ref int i, Type type,
			Token endToken, int maxLength, Logger log)
		{
			int len = tokens.Count;
			if (i >= len) 
			{
				log.Error("BuildArray: Input index too large.");
				return(null);
			}

			// put the objects into an array list first, since we don't
			// know length
			ArrayList list = new ArrayList();

			// allow null endToken specified
			if (endToken == null) endToken = new EofToken();

			Token token = null;
			token = (Token)tokens[i++];
			int arrayLength = 0;

			while ((!(token is EofToken)) && (token != endToken) && (i < len)
				&& ((maxLength < 0) || (arrayLength < maxLength)))
			{
				Object o = token.ConvertToType(type);
				list.Add(o);
				arrayLength++;
				token = (Token)tokens[i++];
			}
			i--; // went one past

			return(list.ToArray(type));
		}

		/// <summary>
		/// Given a Token[] and a reference int, skip forward
		/// in the token array until a WordToken is found,
		/// and leave the reference int at that index.
		/// </summary>
		/// <param name="tokens">The token array.</param>
		/// <param name="i">The start index, and the result index.</param>
		/// <returns>bool - true for success, false for 
		/// hit the end of the tokens.</returns>
		public static bool SkipToWord(Token[] tokens, ref int i)
		{
			while (!(tokens[i] is WordToken))
			{
				i++;
				if (i >= tokens.Length) return(false);
			}
			return(true);
		}

		/// <summary>
		/// Given a Token[], a reference int and a string, skip forward
		/// in the token array until a token matches the string
		/// and leave the reference int at that index.
		/// </summary>
		/// <param name="tokens">The token array.</param>
		/// <param name="i">The start index, and the result index.</param>
		/// <param name="s">The string to look for.</param>
		/// <returns>bool - true for success, false for 
		/// hit the end of the tokens.</returns>
		public static bool SkipToStringValue(Token[] tokens, ref int i,
			string s)
		{
			while (tokens[i] != s)
			{ 
				i++; 
				if (i >= tokens.Length) return(false);
			}
			return(true);
		}

		/// <summary>
		/// Given a Token[] and a reference int, skip forward
		/// in the token array until a WordToken is found,
		/// and leave the reference int at that index.
		/// </summary>
		/// <param name="tokens">The token array.</param>
		/// <param name="i">The start index, and the result index.</param>
		/// <param name="c">The char to look for.</param>
		/// <returns>bool - true for success, false for 
		/// hit the end of the tokens.</returns>
		public static bool SkipToChar(Token[] tokens, ref int i,
			char c)
		{
			while(tokens[i] != c)
			{ 
				i++; 
				if (i >= tokens.Length) return(false);
			}
			return(true);
		}

		/// <summary>
		/// Given a Token[] and a reference int, skip forward
		/// in the token array until a WordToken is found,
		/// and leave the reference int at that index.
		/// </summary>
		/// <param name="tokens">The token array.</param>
		/// <param name="i">The start index, and the result index.</param>
		/// <returns>bool - true for success, false for 
		/// hit the end of the tokens.</returns>
		public static bool SkipWs(Token[] tokens, ref int i)
		{
			while (tokens[i] is WhitespaceToken) 
			{ 
				i++; 
				if (i >= tokens.Length) return(false);
			}
			return(true);
		}

		/// <summary>
		/// Given a Token[] and a reference int, skip forward
		/// in the token array until a WordToken is found,
		/// and leave the reference int at that index.
		/// </summary>
		/// <param name="tokens">The token array.</param>
		/// <param name="i">The start index, and the result index.</param>
		/// <returns>bool - true for success, false for 
		/// hit the end of the tokens.</returns>
		public static bool SkipToEol(Token[] tokens, ref int i)
		{
			if ((i < 0) || (i >= tokens.Length)) return(false);
			while (!(tokens[i] is EolToken))
			{ 
				i++; 
				if (i >= tokens.Length) return(false);
			}
			return(true);
		}

		/// <summary>
		/// Given a Token[] and a reference int, skip forward
		/// in the token array until a WordToken is found,
		/// and leave the reference int at that index.
		/// </summary>
		/// <param name="tokens">The token array.</param>
		/// <param name="dropTokens">The tokens to drop.</param>
		/// <returns>bool - true for success, false for 
		/// hit the end of the tokens.</returns>
		public static Token[] DropTokens(Token[] tokens, 
			Token[] dropTokens)
		{
			ArrayList outputList = new ArrayList();
			
			int i = 0;
			for (i = 0; i < tokens.Length; i++)
			{
				bool dropIt = false;
				for (int j = 0; j < dropTokens.Length; j++)
				{
					if (tokens[i].Equals(dropTokens[j])) dropIt = true;
				}
				if (!dropIt) outputList.Add(tokens[i]);
			}

			// copy to array
			Token[] outputTokens = new Token[outputList.Count];
			i = 0;
			foreach(Token t in outputList) outputTokens[i++] = t;

			return(outputTokens);
		}

		/// <summary>
		/// Find matching closing character.
		/// The matchable pairs of characters are parenthesis (), 
		/// square brackets [], and curly braces {}.
		/// Given a Token[] and a reference int containing the index
		/// in the Token[] of a matchable? char, skip forward
		/// in the token array until the matching character is found.
		/// </summary>
		/// <remarks>
		/// This implicitly skips matching characters in quotes and
		/// comments if they are hidden in the tokens.  So if you grab
		/// comments and quotes when you tokenize, the characters in those
		/// tokens are not looked at by this function.
		/// </remarks>
		/// <param name="tokens">The token array.</param>
		/// <param name="i">The start index, and the result index.</param>
		/// <param name="c">The start character whose match is to be found.</param>
		/// <returns>bool - true for success, false for 
		/// hit the end of the tokens.</returns>
		public static bool FindMatch(Token[] tokens, ref int i,
			char c)
		{
			char endChar;
			if (c == '(') endChar = ')';
			else if (c == '{') endChar = '}';
			else if (c == '[') endChar = ']';
			else return(false);

			int nestLevel = 1; // count first one

			// i'th token must be the start character
			if (tokens[i] != c)
			{
				return(false);
			}
			i++;

			// terminate when we hit an end char and that takes us to
			// nest level 0
			while (nestLevel > 0)
			{
				if (tokens[i] == c) nestLevel++;
				else if (tokens[i] == endChar) nestLevel--;
				i++;
				if (i >= tokens.Length) return(false);
			}
			i--; // went one past

			return(true);
		}

		/// <summary>
		/// Simple test of some ParseUtil methods.
		/// </summary>
		/// <returns>bool - true for all passed, false otherwise</returns>
		public static bool TestSelf()
		{
			Logger log = new Logger("ParseUtil: TestSelf");
			log.Info("Starting...");

			StreamTokenizer tokenizer = new StreamTokenizer();
			tokenizer.Verbosity = VerbosityLevel.Warn;

			// FindMatch
			ArrayList alist = new ArrayList();
			tokenizer.TokenizeString("{ [ ] '}' }", alist);
			foreach(Token t in alist) log.Debug("Token = {0}", t);

			Token[] tarray = (Token[])alist.ToArray(typeof(Token));
			int i = 0;
			if (!FindMatch(tarray, ref i, '{'))
			{
				log.Error("FindMatch failed to match { char");
				return(false);
			}

			if (i != 4)
			{
				log.Error("FindMatch got the wrong answer {0}", i);
				return(false);
			}
			else log.Info("FindMatch worked.");

			//
			// try BuildArray
			//
			ArrayList tokens = new ArrayList();
			tokenizer.TokenizeString("1 2 3 4 5", tokens);
			foreach(Token t in tokens) log.Debug("Token = {0}", t);

			i = 0;
			Int16[] shorts = (short[])BuildArray(tokens, ref i, typeof(Int16), null,
				-1, log);
		
			if (shorts == null) 
			{
				log.Error("Unable to BuildArray of shorts.");
				return(false);
			}

			log.Info("Parsed shorts:");
			foreach(Int16 s in shorts)
			{
				log.Write("{0}, ", s);
			}
			log.WriteLine(String.Empty);

			//
			// try BuildArray of floats, char terminated
			//
			tokens.Clear();
			tokenizer.TokenizeString("1 2 ; 3 4", tokens);
			foreach(Token t in tokens) log.Debug("Token = {0}", t);

			i = 0;
			Single[] floats = (float[])BuildArray(tokens, ref i, typeof(Single), 
				new CharToken(';'), -1, log);
		
			if (floats == null) 
			{
				log.Error("Unable to BuildArray of floats.");
				return(false);
			}

			log.Info("Parsed floats:");
			foreach(float f in floats)
			{
				log.Write("{0}, ", f);
			}
			log.WriteLine(String.Empty);

			if (i != 2)
			{
				log.Error("BuildArray left i = {0} which is incorrect");
				return(false);
			}

			//
			// try BuildArray on high-precision floats
			//
			tokens.Clear();
			float f1 = 1.23456f;
			float f2 = 2.34567f;
			tokenizer.TokenizeString(String.Format("{0:f6} {1:f6}", f1,f2), tokens);
			foreach(Token t in tokens) log.Debug("Token = {0}", t);

			i = 0;
			floats = (float[])BuildArray(tokens, ref i, typeof(Single), 
				null, -1, log);
		
			if (floats == null) 
			{
				log.Error("Unable to BuildArray of floats.");
				return(false);
			}

			log.Info("Parsed floats:");
			foreach(float f in floats)
			{
				log.Write("{0}, ", f);
			}
			log.WriteLine(String.Empty);

			if (floats[0] != f1)
			{
				log.Error("BuildArray produced float {0:f6} instead of {1:f6}",
					floats[0], f1);
				return(false);
			}

			//
			// try BuildArray of chars, maxLength terminated
			//
			log.Info("Chars, terminated by maxLength");
			tokens.Clear();
			tokenizer.TokenizeString("12 2 ; 3 4", tokens);
			foreach(Token t in tokens) log.Debug("Token = {0}", t);

			i = 0;
			char[] chars = (char[])BuildArray(tokens, ref i, typeof(Char), 
				null, 3, log);
		
			if (chars == null) 
			{
				log.Error("Unable to BuildArray of chars.");
				return(false);
			}

			log.Info("Parsed chars:");
			foreach(char f in chars)
			{
				log.Write("{0}, ", f);
			}
			log.WriteLine(String.Empty);

			if (i != 4)
			{
				log.Error("BuildArray left i = {0} which is incorrect", i);
				return(false);
			}

			//
			// try BuildArray of hex numbers
			//
			log.Info("Hex numbers");
			tokens.Clear();
			tokenizer.Settings.ParseHexNumbers = true;
			tokenizer.TokenizeString("0xfff, 0xffe", tokens);
			foreach(Token t in tokens) log.Debug("Token = {0}", t);

			i = 0;
			ushort[] ushorts = (ushort[])BuildArray(tokens, ref i, typeof(ushort), 
				null, 3, log);
		
			if (ushorts == null) 
			{
				log.Error("Unable to BuildArray of ushorts.");
				return(false);
			}

			log.Info("Parsed ushorts:");
			foreach(ushort us in ushorts)
			{
				log.Write("{0}, ", us);
			}
			log.WriteLine(String.Empty);

//			if (i != 4)
//			{
//				log.Error("BuildArray left i = {0} which is incorrect", i);
//				return(false);
//			}

			log.Info("All PASSED");
			return(true);
		}
	}
}
