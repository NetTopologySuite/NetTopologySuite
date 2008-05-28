/*
* Arguments class: application arguments interpreter
*
* Authors:		R. LOPES
* Contributors:	R. LOPES
* Created:		25 October 2002
* Modified:		28 October 2002
*
* Version:		1.0
*/

using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System;

namespace GisSharpBlog.NetTopologySuite.Console
{
    public class Arguments
    {

        private static readonly Regex _spliter = new Regex(@"^-{1,2}|^/|=|:",
                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _remover = new Regex(@"^['String.Empty]?(.*?)['String.Empty]?$",
                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Variables
        private readonly StringDictionary Parameters = new StringDictionary();

        // Constructor
        public Arguments(String[] Args)
        {
            String parameter = null;
            String[] parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
            foreach (String arg in Args)
            {
                // Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
                parts = _spliter.Split(arg, 3);

                switch (parts.Length)
                {
                    // Found a value (for the last parameter found (space separator))
                    case 1:
                        if (parameter != null)
                        {
                            if (!Parameters.ContainsKey(parameter))
                            {
                                parts[0] = _remover.Replace(parts[0], "$1");
                                Parameters.Add(parameter, parts[0]);
                            }

                            parameter = null;
                        }

                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (parameter != null && !Parameters.ContainsKey(parameter))
                        {
                            Parameters.Add(parameter, "true");
                        }

                        parameter = parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (parameter != null && !Parameters.ContainsKey(parameter))
                        {
                            Parameters.Add(parameter, "true");
                        }

                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
                        if (!Parameters.ContainsKey(parameter))
                        {
                            parts[2] = _remover.Replace(parts[2], "$1");
                            Parameters.Add(parameter, parts[2]);
                        }

                        parameter = null;
                        break;
                }
            }

            // In case a parameter is still waiting
            if (parameter != null && !Parameters.ContainsKey(parameter))
            {
                Parameters.Add(parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists
        public String this[String Param]
        {
            get { return Parameters[Param]; }
        }
    }
}