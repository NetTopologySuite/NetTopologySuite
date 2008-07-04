// Token.cs
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
// To-do:
//   more Equals methods?

using System;
using System.Globalization;

namespace RTools_NTS.Util
{
	/// <summary>
	/// Token class used by StreamTokenizer.
	/// This represents a single token in the input stream.
	/// This is subclassed to provide specific token types,
	/// such as CharToken, FloatToken, etc.
	/// </summary>
	abstract public class Token
	{
		#region Properties

		/// <summary>
		/// The line number in the input stream where this token originated.
		/// This is base-1.
		/// </summary>
		protected int lineNumber;

		/// <summary>
		/// The line number where this token was found.  This is base-1.
		/// </summary>
		public int LineNumber { get { return(lineNumber); }}

		/// <summary>
		/// A storage object for the data of this token.
		/// </summary>
		protected object obj;

		/// <summary>
		/// The Object stored by this token.  This will be
		/// a primitive C# type.
		/// </summary>
		public object Object { get { return(obj); }}

		/// <summary>
		/// Backer for UntermError.
		/// </summary>
		bool untermError;

		/// <summary>
		/// Whether or not there was an unterminated token problem
		/// when creating this token.  See UntermErrorMessage for
		/// a message associated with the problem.
		/// </summary>
		public bool UntermError 
		{ 
			get { return(untermError); } 
			set { untermError = value; } 
		}

		/// An error message associated with unterm error.
		string untermErrorMsg;

		/// <summary>
		/// The error message if there was an unterminated token error
		/// creating this token.
		/// </summary>
		public string UntermErrorMsg
		{
			get { return(untermErrorMsg); }
			set 
			{ 
				untermError = true;
				untermErrorMsg = value; 
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a Token with the specified line number.
		/// </summary>
		/// <param name="line">The line number where this
		/// token comes from.</param>
		public Token(int line)
		{
			obj = null;
			untermError = false;
			lineNumber = line;
		}

		#endregion

		#region Operator overloads and Equals

		/// <summary>
		/// Equals override.
		/// </summary>
		/// <param name="other">The object to compare to.</param>
		/// <returns>bool - true for equals, false otherwise.</returns>
		public override bool Equals(object other)
		{
			if (other == null) return(false);
			else if (!(other is Token)) return(false);
			else return(obj.Equals(((Token)other).obj));
		}

		/// <summary>
		/// Equals overload.
		/// </summary>
		/// <param name="s">The string to compare to.</param>
		/// <returns>bool</returns>
		public bool Equals(string s)
		{
			if (s == null) return(false);
			else return(StringValue.Equals(s));
		}

		/// <summary>
		/// Equals overload.
		/// </summary>
		/// <param name="c">The char to compare to.</param>
		/// <returns>bool</returns>
		public bool Equals(char c)
		{
			if (!(this is CharToken)) return(false);
			CharToken ct = this as CharToken;
			return(ct.Object.Equals(c));
		}

		/// <summary>
		/// Operator== overload.  Compare a token and an object.
		/// </summary>
		/// <param name="t">The token to compare.</param>
		/// <param name="o">The other object.</param>
		/// <returns>bool</returns>
		public static bool operator == (Token t, Object o)
		{
			if ((object)t == null)
				if (o == null) return(true);
				else return(false);
			else if (o == null) return(false);
			return(t.Equals(o));
		}

		/// <summary>
		/// Operator!= overload.  Compare a token and an object.
		/// </summary>
		/// <param name="t">The token to compare.</param>
		/// <param name="o">The other object.</param>
		/// <returns>bool</returns>
		public static bool operator != (Token t, Object o)
		{
			if ((object)t == null)
				if (o == null) return(false);
				else return(true);
			return(!t.Equals(o));
		}

		/// <summary>
		/// Operator== overload.  Compare a token and a char.
		/// </summary>
		/// <param name="t">The token to compare.</param>
		/// <param name="c">The char.</param>
		/// <returns>bool</returns>
		public static bool operator == (Token t, char c)
		{
			if ((object)t == null) return(false);
			return(t.Equals(c));
		}

		/// <summary>
		/// Operator!= overload.  Compare a token and a char.
		/// </summary>
		/// <param name="t">The token to compare.</param>
		/// <param name="c">The char.</param>
		/// <returns>bool</returns>
		public static bool operator != (Token t, char c)
		{
			if ((object)t == null) return(false);
			return(!t.Equals(c));
		}

		/// <summary>
		/// Operator== overload.  Compare a token and a string.
		/// </summary>
		/// <param name="t">The token to compare.</param>
		/// <param name="s">The string.</param>
		/// <returns>bool</returns>
		public static bool operator == (Token t, string s)
		{
			if ((object)t == null)
				if (s == null) return(true);
				else return(false);
			return(t.Equals(s));
		}

		/// <summary>
		/// Operator!= overload.  Compare a token and a string.
		/// </summary>
		/// <param name="t">The token to compare.</param>
		/// <param name="s">The string.</param>
		/// <returns>bool</returns>
		public static bool operator != (Token t, string s)
		{
			if ((object)t == null)
				if (s == null) return(false);
				else return(true);
			return(!t.Equals(s));
		}

		#endregion

		#region Standard Methods

		/// <summary>
		/// Override.  Returns the ToString().GetHashCode().
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return(ToString().GetHashCode());
		}

		/// <summary>
		/// Return this token's value as a string.
		/// </summary>
		/// <returns>This token's value as a string.</returns>
		public virtual string StringValue
		{
			get { return("unset"); }
		}

		/// <summary>
		/// Produce a string which includes the line number.
		/// </summary>
		/// <returns></returns>
		public string ToLineString() 
		{ 
			return(String.Format("{0}: line {1}", ToDebugString(), lineNumber));
		}

		/// <summary>
		/// Produce a string which includes the token type.
		/// </summary>
		/// <returns></returns>
		public virtual string ToDebugString() 
		{ 
			return(String.Format("{0}: line {1}", ToString(), lineNumber));
		}

		/// <summary>
		/// Create an object of the specified type corresponding to
		/// this token.
		/// </summary>
		/// <param name="t">The type of object to create.</param>
		/// <returns>The new object, or null for error.</returns>
		public Object ConvertToType(Type t)
		{
            return Convert.ChangeType(StringValue, t);			
		}

		#endregion
	}

	#region EolToken

	/// <summary>
	/// Represents end-of-lines (line separator characters).
	/// </summary>
	public class EolToken : Token
	{
		/// <summary>Default constructor.</summary>
		public EolToken() : base(0) {}
		/// <summary>Constructor that takes line number.</summary>
		public EolToken(int line) : base(line) {}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToDebugString() { return("Eol"); }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToString() { return("\n"); }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override bool Equals(object other)
		{
			if (!(other is EolToken)) return(false);
			else return(true);
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string StringValue { get { return(ToString()); } }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override int GetHashCode()
		{
			return(ToString().GetHashCode());
		}
	}

	#endregion

	#region EofToken

	/// <summary>
	/// Represents end of file/stream.
	/// </summary>
	public class EofToken : Token
	{
		/// <summary>Default constructor.</summary>
		public EofToken() : base(0) {}
		/// <summary>Constructor that takes line number.</summary>
		public EofToken(int line) : base(line) {}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToString() { return(String.Empty); }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToDebugString() { return("Eof"); }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override bool Equals(object other)
		{
			if (!(other is EofToken)) return(false);
			else return(true);
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string StringValue { get { return(ToString()); } }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override int GetHashCode()
		{
			return(ToString().GetHashCode());
		}
	}

	#endregion

	#region StringToken

	/// <summary>
	/// Abstract base class for string tokens.
	/// </summary>
	public abstract class StringToken : Token
	{
		/// <summary>Default constructor.</summary>
		public StringToken(string s) : base(0) { obj = s; }
		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public StringToken(string s, int line) : base(line) { obj = s; }

		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToDebugString() 
		{ 
			return(GetType().Name + ":'" + (string)obj + "'"); 
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToString() { return((string)obj); }

		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string StringValue { get { return((string)obj); } }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override int GetHashCode()
		{
			return(ToString().GetHashCode());
		}
	}

	#endregion

	#region WordToken

	/// <summary>
	/// Token type for words, meaning sequences of word
	/// characters.
	/// </summary>
	public class WordToken : StringToken 
	{ 
		/// <summary>Constructor with the specified value.</summary>
		public WordToken(string s) : base(s) {} 
		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public WordToken(string s, int line) : base(s, line) {} 
	}

	#endregion

	#region QuoteToken

	/// <summary>
	/// Token type for Quotes such as "this is a quote".
	/// </summary>
	public class QuoteToken : StringToken 
	{ 
		/// <summary>Constructor with the specified value.</summary>
		public QuoteToken(string s) : base(s) {} 
		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public QuoteToken(string s, int line) : base(s, line) {} 
	}

	#endregion

	#region CommentToken

	/// <summary>
	/// Token type for comments, including line and block
	/// comments.
	/// </summary>
	public class CommentToken : StringToken 
	{ 
		/// <summary>Constructor with the specified value.</summary>
		public CommentToken(string s) : base(s) {} 
		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public CommentToken(string s, int line) : base(s, line) {} 
	}

	#endregion

	#region WhitespaceToken

	/// <summary>
	/// Token type for whitespace such as spaces and tabs.
	/// </summary>
	public class WhitespaceToken : StringToken 
	{ 
		/// <summary>Constructor with the specified value.</summary>
		public WhitespaceToken(string s) : base(s) {} 
		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public WhitespaceToken(string s, int line) : base(s, line) {} 
	}

	#endregion

	#region CharToken

	/// <summary>
	/// Token type for characters, meaning non-word characters.
	/// </summary>
	public class CharToken : Token
	{
		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public CharToken(string s, int line) : base(line)
		{
			if (s.Length > 0) obj = s[0];
		}

		/// <summary>Constructor with the specified value.</summary>
		public CharToken(char c) : base(0) { obj = c; }

		/// <summary>Constructor with the specified value.</summary>
		public CharToken(char c, int line) : base(line)
		{
			obj = c;
		}

		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToDebugString() { return(String.Format("CharToken: {0}", (Char)obj)); }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToString() { return(String.Format("{0}", (Char)obj)); }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override int GetHashCode()
		{
			return(ToString().GetHashCode());
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string StringValue { get { return(String.Format("{0}", (Char)obj)); } }
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override bool Equals(object other)
		{
			if ((object)other == null) return(false);
			if (!GetType().Equals(other.GetType())) return(false);
			if ((obj == null) || (((CharToken)other).obj == null)) return(false);
			if (((Char)obj).Equals((Char)((CharToken)other).Object)) return(true);
			else return(false);
		}
	}

	#endregion

	#region FloatToken   

	/// <summary>
	/// Token type for floating point numbers, stored internally as a Double.
	/// </summary>
	public class FloatToken : Token
	{
        // NOTE: modified for "safe" assembly in Sql 2005
        // Static field now is an instance field!
        private NumberFormatInfo numberFormatInfo = null;

        // Static method now is an instance method!
        private NumberFormatInfo GetNumberFormatInfo()
        {
            if (numberFormatInfo == null)
            {
                numberFormatInfo = new NumberFormatInfo();
                numberFormatInfo.NumberDecimalSeparator = ".";
            }
            return numberFormatInfo;
        }

		/// <summary>
        /// Constructor with the specified value.
        /// </summary>
		public FloatToken(string s) : base(0)
		{                        
			try
			{
				obj = Double.Parse(s, GetNumberFormatInfo());
			}
			catch(Exception) { obj = null; }
		}

		/// <summary>
        /// Constructor with the specified value.
        /// </summary>
		public FloatToken(float f) : base(0)
		{
			try
			{
				obj = (double)f;
			}
            catch (Exception) { obj = null; }
		}

		/// <summary>
        /// Constructor with the specified value.
        /// </summary>
		public FloatToken(double d) : base(0)
		{
			try
			{
				obj = d;
			}
            catch (Exception) { obj = null; }
		}

		/// <summary>
        /// Constructor with the specified value and line number.
        /// </summary>
		public FloatToken(string s, int line) : base(line)
		{
			try
			{
                obj = Double.Parse(s, GetNumberFormatInfo());
			}
            catch (Exception) { obj = null; }
		}

		/// <summary>
        /// Constructor with the specified value and line number.
        /// </summary>
		public FloatToken(double f, int line) : base(line)
		{
			try
			{
				obj = (double) f;
			}
            catch (Exception) { obj = null; }
		}

		/// <summary>
        /// Override, see base <see cref="Token"/>
        /// </summary>
		public override string ToDebugString() 
		{ 
			if (obj != null)
				 return(String.Format("FloatToken: {0:R}", (Double) obj)); 
			else return(String.Format("FloatToken: null")); 
		}

		/// <summary>
        /// Override, see base <see cref="Token"/>
        /// </summary>
		public override string ToString() 
		{ 
			if (obj != null)
				 return(String.Format("{0:R}", (Double) obj)); 
			else return(String.Format("null")); 
		}

		/// <summary>
        /// Override, see base <see cref="Token"/>
        /// </summary>
		public override int GetHashCode()
		{
			return(ToString().GetHashCode());
		}

		/// <summary>
        /// Override, see base <see cref="Token"/>
        /// </summary>
		public override string StringValue 
		{ 
			get
			{
				if (obj != null)
					 // return(String.Format("{0:f9}", (Double)obj)); 
                     return (String.Format("{0:R}", (Double) obj)); 
				else return(String.Format("null")); 
			}
		}

		/// <summary>
        /// Override, see base <see cref="Token"/>
        /// </summary>
		public override bool Equals(object other)
		{
			if ((object)other == null) 
                return false;
			if (!GetType().Equals(other.GetType())) 
                return false;
			if ((obj == null) || (((FloatToken)other).obj == null)) 
                return false ;
			if (((Double) obj).Equals((Double) ((FloatToken) other).Object)) 
                return true;
			else return false;
		}
	}

	#endregion

	#region IntToken

	/// <summary>
	/// Token type for integer tokens. This handles both Int32 and Int64.
	/// </summary>
	public class IntToken : Token
	{
		/// <summary>Constructor with the specified value.</summary>
		public IntToken(int i) : base(0)
		{
			obj = i;
		}

		/// <summary>Constructor with the specified value.</summary>
		public IntToken(long i) : base(0)
		{
			obj = i;
		}

		/// <summary>Constructor with the specified value.</summary>
		public IntToken(string s) : base(0)
		{
			Parse(s);
		}

		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public IntToken(string s, int line) : base(line)
		{
			Parse(s);
		}

		/// <summary>Constructor with the specified value
		/// and line number.</summary>
		public IntToken(int i, int line) : base(line)
		{
			obj = i;
		}

		/// <summary> 
		/// Constructor for a 64 bit int 
		/// </summary> 
		public IntToken(long l, int line) : base(line)
		{
			obj = l;
		}

		/// <summary>
		/// Parse a string known to be a hex string.  This is faster
		/// than Parse which doesn't assume the number is Hex.  This will
		/// throw an exception if the input number isn't hex.
		/// </summary>
		/// <param name="s">The hex number as a string.</param>
		/// <param name="lineNumber">The line where this token was found.</param>
		/// <returns>A new IntToken set to the value in the input string.</returns>
		public static IntToken ParseHex(string s, int lineNumber)
		{
			IntToken it = null;
			try
			{
				it = new IntToken(Convert.ToInt32(s, 16), lineNumber);
			}
			catch
			{
				it = new IntToken(Convert.ToInt64(s, 16), lineNumber);
			}

			return(it);
		}

		/// <summary>
		/// Convert the input string to an integer, if possible
		/// </summary>
		/// <param name="s">The string to parse.</param>
		private void Parse(string s)
		{
			// try base 10 separately since it will be the most
			// common case
			try
			{
				obj = Int32.Parse(s);
				return;
			}
			catch(Exception) 
			{
				// try 64 bit base 10
				try
				{
					obj = Int64.Parse(s);
					return;
				}
				catch(Exception)
				{ 					
				}  // don't give up yet
			}

			// not a normal int, try other bases
			int[] bases = {16, 2, 8};
			foreach(int b in bases)
			{
				try
				{
					obj = Convert.ToInt32(s, b);
					return;
				}
				catch
				{
					// try 64 bit base 10
					try
					{
						obj = Convert.ToInt64(s, b);
						return;
					}
					catch { } // don't give up yet
				}
			}

			obj = null;
		}

		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToDebugString() 
		{ 
			if (obj != null)
				return(String.Format("IntToken: {0}", obj)); 
			else
				return(String.Format("IntToken: null")); 
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string ToString() 
		{ 
			if (obj != null)
				return(String.Format("{0}", obj)); 
			else
				return(String.Format("null")); 
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override int GetHashCode()
		{
			return(ToString().GetHashCode());
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override string StringValue 
		{ 
			get 
			{
				if (obj != null)
					return(String.Format("{0}", obj)); 
				else
					return(String.Format("null")); 
			}
		}
		/// <summary>Override, see base <see cref="Token"/></summary>
		public override bool Equals(object other)
		{
			if ((object)other == null) return(false);
			if (!GetType().Equals(other.GetType())) return(false);
			if ((obj == null) || (((IntToken)other).obj == null)) return(false);
			if (!obj.GetType().Equals(((IntToken)other).obj.GetType())) return(false);
			if (obj is Int32)
			{
				if (((Int32)obj).Equals((Int32)((IntToken)other).Object)) return(true);
			}
			else
			{
				if (((Int64)obj).Equals((Int64)((IntToken)other).Object)) return(true);
			}
			return(false);
		}
	}

	#endregion

}
