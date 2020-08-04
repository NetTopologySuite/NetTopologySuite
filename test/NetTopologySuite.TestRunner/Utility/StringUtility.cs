using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetTopologySuite.Utilities;
using RTools_NTS.Util;

namespace Open.Topology.TestRunner.Utility
{
    /**
     *  Useful string utilities
     *
     *@author     jaquino
     *@created    June 22, 2001
     *
     * @version 1.7
     */

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

        /**
         * Capitalizes the given string.
         *
         * @param s the string to capitalize
         * @return the capitalized string
         */

        public static string Capitalize(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /**
         *  Returns true if s can be converted to an int.
         */

        public static bool IsInteger(string s)
        {
            int val;
            if (int.TryParse(s, out val))
                return true;
            return false;
        }

        /**
         *  Returns an throwable's stack trace
         */

        public static string GetStackTrace(Exception t)
        {
            return t.StackTrace;
        }

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

        /**
         *  Converts the milliseconds value into a String of the form "9d 22h 15m 8s".
         */

        public static string GetTimeString(long milliseconds)
        {
            long remainder = milliseconds;
            long days = remainder/86400000;
            remainder = remainder%86400000;
            long hours = remainder/3600000;
            remainder = remainder%3600000;
            long minutes = remainder/60000;
            remainder = remainder%60000;
            long seconds = remainder/1000;
            return days + "d " + hours + "h " + minutes + "m " + seconds + "s";
        }

        /**
         *  Returns true if substring is indeed a substring of string.
         *  Case-insensitive.
         */

        public static bool ContainsIgnoreCase(string wholeString, string substring)
        {
            return Contains(wholeString.ToLowerInvariant(), substring.ToLowerInvariant());
        }

        /**
         *  Returns true if substring is indeed a substring of string.
         */

        public static bool Contains(string wholeString, string substring)
        {
            return wholeString.IndexOf(substring) > -1;
        }

        /**
         *  Returns a string with all occurrences of oldChar replaced by newStr
         */

        public static string Replace(string str, char oldChar, string newStr)
        {
            var buf = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (ch == oldChar)
                {
                    buf.Append(newStr);
                }
                else
                {
                    buf.Append(ch);
                }
            }
            return buf.ToString();
        }

        /**
         *  Returns a String of the given length consisting entirely of the given
         *  character
         */

        public static string StringOfChar(char ch, int count)
        {
            var buf = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                buf.Append(ch);
            }
            return buf.ToString();
        }

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

        /**
         *  Returns the elements of v in uppercase
         */

        public static IEnumerable<string> ToUpperCase(IEnumerable<string> v)
        {
            foreach (string s in v)
                yield return s.ToUpperInvariant();
        }

        /**
         *  Returns the elements of v in lowercase
         */

        public static IEnumerable<string> ToLowerCase(IEnumerable<string> v)
        {
            foreach (string s in v)
                yield return s.ToLowerInvariant();
        }

        /**
         *  Returns the elements of c separated by commas and enclosed in
         *  single-quotes
         */

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

        /**
         *  Returns the elements of c separated by commas. c must not be empty.
         */

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

        /**
         *  Converts the comma-delimited string into a List of trimmed strings.
         */

        public static List<string> FromCommaDelimitedString(string s)
        {
            var result = new List<string>(s.Split(','));
            return result;
        }

        /**
         *  If s is null, returns "null"; otherwise, returns s.
         */

        public static string ToStringNeverNull<T>(T o)
            where T : class
        {
            return o == null ? "null" : o.ToString();
        }

        /**
         *  Replaces all instances of the String o with the String n in the
         *  StringBuffer orig if all is true, or only the first instance if all is
         *  false. Posted by Steve Chapel <schapel@breakthr.com> on UseNet
         */

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

        /**
         *  Returns original with all occurrences of oldSubstring replaced by
         *  newSubstring
         */

        public static string ReplaceAll(string original, string oldSubstring, string newSubstring)
        {
            return Replace(original, oldSubstring, newSubstring, true);
        }

        /**
         *  Returns original with the first occurrenc of oldSubstring replaced by
         *  newSubstring
         */

        public static string ReplaceFirst(string original, string oldSubstring, string newSubstring)
        {
            return Replace(original, oldSubstring, newSubstring, false);
        }

        /**
         *  Pads the String with the given character until it has the given length. If
         *  original is longer than the given length, returns original.
         */

        public static string LeftPad(string original, int length, char padChar)
        {
            return original.PadLeft(length, padChar);
        }

        /**
         *  Pads the String with the given character until it has the given length. If
         *  original is longer than the given length, returns original.
         */

        public static string RightPad(string original, int length, char padChar)
        {
            return original.PadRight(length, padChar);
        }

        /**
         *  Removes the HTML tags from the given String, inserting line breaks at
         *  appropriate places. Needs a little work.
         */

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

        /**
         *  Returns d as a string truncated to the specified number of decimal places
         */

        public static string Format(double d, int decimals)
        {
            double factor = Math.Pow(10, decimals);
            double digits = Math.Round(factor*d);
            return ((int) Math.Floor(digits/factor)) + "." + ((int) (digits%factor));
        }

        /**
         *  Line-wraps s by inserting CR-LF instead of the first space after the nth
         *  column.
         */

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

        /**
         *  Removes vowels from the string. Case-insensitive.
         */

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

        /**
         *  Removes vowels from the string except those that start words.
         *  Case-insensitive.
         */

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

        /**
         *  Replaces consecutive instances of characters with single instances.
         *  Case-insensitive.
         */

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

        /**
         *  Returns the position of the first occurrence of the given character found
         *  in s starting at start. Ignores text within pairs of parentheses. Returns
         *  -1 if no occurrence is found.
         */

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

        /**
         *  Returns original with occurrences of oldSubstring replaced by
         *  newSubstring. Set all to true to replace all occurrences, or false to
         *  replace the first occurrence only.
         */

        public static string Replace(
            string original,
            string oldSubstring,
            string newSubstring,
            bool all)
        {
            var b = new StringBuilder(original);
            Replace(b, oldSubstring, newSubstring, all);
            return b.ToString();
        }

        /**
         *  Replaces vowels that start words with a special code
         */

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

        /**
         *  Decodes strings returned by #encodeStartingVowels
         */

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
        public static string CurrentMethodName()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception e)
            {
                string callStack = e.StackTrace;
                int atPos = callStack.IndexOf("at");
                atPos = callStack.IndexOf("at", atPos + 1);
                int parenthesisPos = callStack.IndexOf("(", atPos);
                return callStack.Substring(atPos + 3, parenthesisPos);
            }
        }

    }
}