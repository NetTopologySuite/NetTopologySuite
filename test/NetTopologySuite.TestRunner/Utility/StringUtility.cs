using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetTopologySuite.Utilities;
using RTools_NTS.Util;

namespace Open.Topology.TestRunner.Utility
{

    /// <summary>Useful string utilities</summary>
    /// <author>jaquino</author>
    public class StringUtil
    {
        public static readonly string NewLine = Environment.NewLine;

        public static string RemoveFromEnd(string s, string strToRemove)
        {
            if (s == null || strToRemove == null) return s;
            if (s.Length < strToRemove.Length) return s;
            int subLoc = s.Length - strToRemove.Length;
            if (s.Substring(subLoc).Equals(strToRemove, StringComparison.CurrentCultureIgnoreCase))
                return s.Substring(0, subLoc);
            return s;
        }

        /// <summary>
        /// Capitalizes the given string.
        /// </summary>
        /// <param name="s">The string to capitalize</param>
        /// <returns>The capitalized string</returns>
        public static string Capitalize(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="s"/> can be converted to an <see cref="int"/>.
        /// </summary>
        /// <param name="s">The string to test</param>
        public static bool IsInteger(string s)
        {
            if (int.TryParse(s, out _))
                return true;
            return false;
        }

        /// <summary>
        /// Returns <paramref name="t"/>'s stack trace
        /// </summary>
        /// <param name="t">The exception</param>
        public static string GetStackTrace(Exception t)
        {
            return t.StackTrace;
        }

        /// <summary>
        /// Returns <paramref name="t"/>'s stack trace up to <paramref name="depth"/>.
        /// </summary>
        /// <param name="t">The exception</param>
        /// <param name="depth">The depth</param>
        public static string GetStackTrace(Exception t, int depth)
        {
            string stackTrace = "";
            var stringReader = new StringReader(GetStackTrace(t));
            //LineNumberReader lineNumberReader = new LineNumberReader(stringReader);
            int lineNumber = 0;
            for (int i = 0; i < depth; i++)
            {
                try
                {
                    stackTrace += ++lineNumber +": " + stringReader.ReadLine() + NewLine;
                }
                catch (IOException)
                {
                    Assert.ShouldNeverReachHere();
                }
            }
            return stackTrace;
        }

        /// <summary>
        /// Converts the milliseconds value into a String of the form "9d 22h 15m 8s".
        /// </summary>
        /// <param name="milliseconds">A milliseconds value</param>
        public static string GetTimeString(long milliseconds)
        {
            long remainder = milliseconds;
            long days = remainder/86400000;
            remainder %= 86400000;
            long hours = remainder/3600000;
            remainder %= 3600000;
            long minutes = remainder/60000;
            remainder %= 60000;
            long seconds = remainder/1000;
            return days + "d " + hours + "h " + minutes + "m " + seconds + "s";
        }

        /// <summary>
        /// Returns true if substring is indeed a substring of string.<br/>
        /// Case-insensitive.
        /// </summary>
        /// <param name="wholeString">A string</param>
        /// <param name="subString">A string</param>
        public static bool ContainsIgnoreCase(string wholeString, string subString)
        {
            return Contains(wholeString.ToLowerInvariant(), subString.ToLowerInvariant());
        }

        /// <summary>
        /// Returns true if substring is indeed a substring of string.
        /// </summary>
        /// <param name="wholeString">A string</param>
        /// <param name="subString">A string</param>
        public static bool Contains(string wholeString, string subString)
        {
            return wholeString.IndexOf(subString, StringComparison.InvariantCulture) > -1;
        }

        /// <summary>
        /// Returns <paramref name="original"/> with all occurrences of <paramref name="oldChar"/> replaced by
        /// <paramref name="newString"/>.
        /// </summary>
        /// <param name="original">The original string</param>
        /// <param name="oldChar">The char to replace</param>
        /// <param name="newString">The replace string</param>
        public static string Replace(string original, char oldChar, string newString)
        {
            var buf = new StringBuilder();
            for (int i = 0; i < original.Length; i++)
            {
                char ch = original[i];
                if (ch == oldChar)
                {
                    buf.Append(newString);
                }
                else
                {
                    buf.Append(ch);
                }
            }
            return buf.ToString();
        }

        /// <summary>
        /// Returns a String of the given length consisting entirely of the given
        /// character
        /// </summary>
        /// <param name="ch">The character</param>
        /// <param name="count">The number of repetitions of <paramref name="ch"/></param>
        public static string StringOfChar(char ch, int count)
        {
            return new string(ch, count);
        }

        /// <summary>
        /// Indents a (multiline) string
        /// </summary>
        /// <param name="original">The original string</param>
        /// <param name="spaces">The number of spaces to indent</param>
        /// <returns>The indented string</returns>
        public static string Indent(string original, int spaces)
        {
            string indent = StringOfChar(' ', spaces);
            string indented = indent + original;

            indented = ReplaceAll(indented, "\r\n", "<<<<.CRLF.>>>>");
            indented = ReplaceAll(indented, "\r", "<<<<.CR.>>>>");
            indented = ReplaceAll(indented, "\n", "<<<<.LF.>>>>");

            indented = ReplaceAll(indented, "<<<<.CRLF.>>>>", "\r\n" + indent);
            indented = ReplaceAll(indented, "<<<<.CR.>>>>", "\r" + indent);
            indented = ReplaceAll(indented, "<<<<.LF.>>>>", "\n" + indent);
            return indented;
        }

        /// <summary>
        /// Returns the elements of <paramref name="v"/> in uppercase
        /// </summary>
        public static IEnumerable<string> ToUpperCase(IEnumerable<string> v)
        {
            foreach (string s in v)
                yield return s.ToUpperInvariant();
        }

        /// <summary>
        /// Returns the elements of <paramref name="v"/> in lowercase
        /// </summary>
        public static IEnumerable<string> ToLowerCase(IEnumerable<string> v)
        {
            foreach (string s in v)
                yield return s.ToLowerInvariant();
        }


        /// <summary>
        /// Returns the elements of c separated by commas and enclosed in
        /// single-quotes
        /// </summary>
        public static string ToCommaDelimitedStringInQuotes<T>(ICollection<T> c)
        {
            if (c == null || c.Count == 0)
                throw new ArgumentException("c");

            var result = new StringBuilder();
            foreach (var o in c)
            {
                result.Append(",'" + o + "'");
            }
            result.Remove(0, 1);
            return result.ToString();
        }

        /// <summary>
        /// Returns the elements of c separated by commas. c must not be empty.
        /// </summary>
        public static string ToCommaDelimitedString<T>(ICollection<T> c)
        {
            if (c == null || c.Count == 0)
                throw new ArgumentException("c");

            var result = new StringBuilder();
            foreach (var o in c)
            {
                result.Append("," + o);
            }
            result.Remove(0, 1);
            return result.ToString();
        }

        /// <summary>
        /// Converts the comma-delimited string into a List of trimmed strings.
        /// </summary>
        public static List<string> FromCommaDelimitedString(string s)
        {
            var result = new List<string>(s.Split(','));
            return result;
        }

 
        /// <summary>If <paramref name="s"/> is <c>null</c>, returns &quot;null&quot;;<br/>Otherwise <c>false</c></summary>
        public static string ToStringNeverNull<T>(T o)
            where T : class
        {
            return o == null ? "null" : o.ToString();
        }

        /// <summary>
        /// Replaces all instances of the String <paramref name="o"/> with the String <paramref name="n"/> in the
        /// StringBuffer <paramref name="orig"/> if <paramref name="all"/> is <c>true</c>, or only the first instance if <paramref name="all"/> is
        /// <c>false</c>.<br/>Posted by Steve Chapel &lt;schapel@breakthr.com&gt; on UseNet
        /// </summary>
        public static void Replace(StringBuilder orig, string o, string n, bool all)
        {
            if (orig == null || o == null || o.Length == 0 || n == null)
            {
                throw new ArgumentException("Null or zero-length String");
            }
            int i = 0;
            while (i + o.Length <= orig.Length)
            {
                if (orig.ToString().Substring(i, i + o.Length).Equals(o))
                {
                    orig.Replace(o, n);// (i, i + o.Length, n);
                    if (!all)
                    {
                        break;
                    }
                    i += n.Length;
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Returns <paramref name="original"/> with all occurrences of <paramref name="oldSubstring"/> replaced by
        /// <paramref name="newSubstring"/>.
        /// </summary>
        public static string ReplaceAll(string original, string oldSubstring, string newSubstring)
        {
            return Replace(original, oldSubstring, newSubstring, true);
        }

        /// <summary>
        /// Returns <paramref name="original"/> with the first occurrence of <paramref name="oldSubstring"/> replaced by
        /// <paramref name="newSubstring"/>.
        /// </summary>
        public static string ReplaceFirst(string original, string oldSubstring, string newSubstring)
        {
            return Replace(original, oldSubstring, newSubstring, false);
        }

        /// <summary>
        /// Pads the String with the given character until it has the given length. <br/>
        /// If <paramref name="original"/> is longer than the given <paramref name="length"/>, returns original.
        /// </summary>
        /// <param name="original">The original string</param>
        /// <param name="length">The pad length</param>
        /// <param name="padChar">The pad character</param>
        public static string LeftPad(string original, int length, char padChar)
        {
            return original.PadLeft(length, padChar);
        }

        /// <summary>
        /// Pads the String with the given character until it has the given length. <br/>
        /// If <paramref name="original"/> is longer than the given <paramref name="length"/>, returns original.
        /// </summary>
        /// <param name="original">The original string</param>
        /// <param name="length">The pad length</param>
        /// <param name="padChar">The pad character</param>
        public static string RightPad(string original, int length, char padChar)
        {
            return original.PadRight(length, padChar);
        }


        /// <summary>
        /// Removes the HTML tags from the given String, inserting line breaks at
        /// appropriate places. 
        /// </summary>
        /// <remarks>Needs a little work.</remarks>
        /// <param name="original">The original string</param>
        public static string StripHTMLTags(string original)
        {
            //Strip the tags from the HTML description
            bool skipping = false;
            bool writing = false;
            var buffer = new StringBuilder();
            var tokenizer = new StreamTokenizer();
            tokenizer.Settings.WordChars("<>");
            var tokens = new List<Token>();
            if (tokenizer.Tokenize(tokens))
            {
                int i = -1;
                while (i < tokens.Count)
                {
                    i++;
                    string token = tokens[i].StringValue;
                    if (token.Equals("<", StringComparison.InvariantCultureIgnoreCase))
                    {
                        skipping = true;
                        writing = false;
                        continue;
                    }
                    if (token.Equals(">", StringComparison.InvariantCultureIgnoreCase))
                    {
                        skipping = false;
                        continue;
                    }
                    if (skipping) continue;

                    if (token.Trim().Length == 0)
                    {
                        continue;
                    }
                    if (!writing)
                    {
                        buffer.Append("\n");
                    }
                    writing = true;
                    buffer.Append(token.Trim());
                }
            }
            return buffer.ToString();
        }

        /// <summary>
        /// Returns <paramref name="d"/> as a string truncated to the specified number of decimal places
        /// </summary>
        public static string Format(double d, int decimals)
        {
            double factor = Math.Pow(10, decimals);
            double digits = Math.Round(factor*d);
            return ((int) Math.Floor(digits/factor)) + "." + ((int) (digits%factor));
        }

        /// <summary>
        /// Line-wraps <paramref name="s"/> by inserting &quot;\n&quot; instead of the first space after the <paramref name="n"/>th column.
        /// </summary>
        public static string Split(string s, int n)
        {
            var b = new StringBuilder();
            bool wrapPending = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (i%n == 0 && i > 0)
                {
                    wrapPending = true;
                }
                char c = s[i];
                if (wrapPending && c == ' ')
                {
                    b.Append("\n");
                    wrapPending = false;
                }
                else
                {
                    b.Append(c);
                }
            }
            return b.ToString();
        }

        /// <summary>
        /// Removes vowels from the string.<br/>
        /// Case-insensitive.</summary>
        public static string RemoveVowels(string s)
        {
            string result = s;
            result = ReplaceAll(result, "a", "");
            result = ReplaceAll(result, "e", "");
            result = ReplaceAll(result, "i", "");
            result = ReplaceAll(result, "o", "");
            result = ReplaceAll(result, "u", "");
            result = ReplaceAll(result, "A", "");
            result = ReplaceAll(result, "E", "");
            result = ReplaceAll(result, "I", "");
            result = ReplaceAll(result, "O", "");
            result = ReplaceAll(result, "U", "");
            return result;
        }

        /// <summary>
        /// Removes vowels from the string except those that start words.<br/>
        /// Case-insensitive.</summary>
        public static string RemoveVowelsSkipStarts(string s)
        {
            string result = s;
            if (!s.StartsWith(" "))
            {
                result = result.Substring(1);
            }
            result = EncodeStartingVowels(result);
            result = RemoveVowels(result);
            result = DecodeStartingVowels(result);
            if (!s.StartsWith(" "))
            {
                result = s[0] + result;
            }
            return result;
        }

        /// <summary>
        /// Replaces consecutive instances of characters with single instances.
        /// Case-insensitive.
        /// </summary>
        public static string RemoveConsecutiveDuplicates(string s)
        {
            string previous = "??";
            var result = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                string c = s[i] + "";
                if (!previous.Equals(c, StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Append(c);
                }
                previous = c;
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns the position of the first occurrence of the given character found
        /// in s starting at start. Ignores text within pairs of parentheses. Returns
        /// -1 if no occurrence is found.
        /// </summary>
        /// <param name="c">The char to find</param>
        /// <param name="s">The string to search</param>
        /// <param name="start">The position to start at.</param>
        public static int IndexOfIgnoreParentheses(char c, string s, int start)
        {
            int level = 0;
            for (int i = start; i < s.Length; i++)
            {
                char other = s[i];
                if (other == '(')
                {
                    level++;
                }
                else if (other == ')')
                {
                    level--;
                }
                else if (other == c && level == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns <paramref name="original"/> with occurrences of <paramref name="oldSubString"/> replaced by
        /// <paramref name="newSubString"/>. Set <paramref name="all"/> to <c>true</c> to replace all occurrences, or <c>false</c> to
        /// replace the first occurrence only.
        /// </summary>
        /// <param name="original">The original string</param>
        /// <param name="oldSubString">The string to replace</param>
        /// <param name="newSubString">The replace string</param>
        /// <param name="all">A flag indicating if all occurrences should be replaced</param>
        public static string Replace(
            string original,
            string oldSubString,
            string newSubString,
            bool all)
        {
            var b = new StringBuilder(original);
            Replace(b, oldSubString, newSubString, all);
            return b.ToString();
        }

        /// <summary>
        /// Replaces vowels that start words with a special code
        /// </summary>
        private static string EncodeStartingVowels(string s)
        {
            string result = s;
            result = ReplaceAll(result, " a", "!~b");
            result = ReplaceAll(result, " e", "!~f");
            result = ReplaceAll(result, " i", "!~j");
            result = ReplaceAll(result, " o", "!~p");
            result = ReplaceAll(result, " u", "!~v");
            result = ReplaceAll(result, " A", "!~B");
            result = ReplaceAll(result, " E", "!~F");
            result = ReplaceAll(result, " I", "!~J");
            result = ReplaceAll(result, " O", "!~P");
            result = ReplaceAll(result, " U", "!~V");
            return result;
        }

        /// <summary>
        /// Decodes strings returned by <see cref="EncodeStartingVowels"/>
        /// </summary>
        private static string DecodeStartingVowels(string s)
        {
            string result = s;
            result = ReplaceAll(result, "!~b", " a");
            result = ReplaceAll(result, "!~f", " e");
            result = ReplaceAll(result, "!~j", " i");
            result = ReplaceAll(result, "!~p", " o");
            result = ReplaceAll(result, "!~v", " u");
            result = ReplaceAll(result, "!~B", " A");
            result = ReplaceAll(result, "!~F", " E");
            result = ReplaceAll(result, "!~J", " I");
            result = ReplaceAll(result, "!~P", " O");
            result = ReplaceAll(result, "!~V", " U");
            return result;
        }

        //From: Phil Hanna (pehanna@my-deja.com)
        //Subject: Re: special html characters and java???
        //Newsgroups: comp.lang.java.help
        //Date: 2000/09/16
        /// <summary>
        /// Escape special characters in <paramref name="s"/> for HTML
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>The escaped HTML string</returns>
        public static string EscapeHTML(string s)
        {
            Replace(s, "\r\n", "\n", true);
            Replace(s, "\n\r", "\n", true);
            Replace(s, "\r", "\n", true);
            var sb = new StringBuilder();
            int n = s.Length;
            for (int i = 0; i < n; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    case '\n':
                        sb.Append("<BR>");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        //Based on code from http://developer.java.sun.com/developer/qow/archive/104/index.html
        /// <summary>
        /// Gets the name of the current method/function
        /// </summary>
        /// <returns>The name of the method/function</returns>
        public static string CurrentMethodName()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception e)
            {
                string callStack = e.StackTrace;
                if (string.IsNullOrWhiteSpace(callStack))
                    return string.Empty;

                int atPos = callStack.IndexOf("at", StringComparison.CurrentCulture);
                atPos = callStack.IndexOf("at", atPos + 1, StringComparison.InvariantCulture);
                int parenthesisPos = callStack.IndexOf("(", atPos, StringComparison.InvariantCulture);
                return callStack.Substring(atPos + 3, parenthesisPos);
            }
        }

    }
}
