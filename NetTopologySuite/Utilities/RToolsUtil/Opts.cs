// Opts.cs
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
//    finish UsageSpec parse behavior
//		- optional and required args
//		- pipe operator
//    encode and report on addition information provided by usage-endCapStyle spec
//        such as mutually exclusive options.
//       - add this capability to parseSpec endCapStyle use also

using System;
using System.Collections;
using System.Text;

namespace RTools_NTS.Util
{
	/// <summary>
	/// A command-line option parser similar to Perl's getopts. 
	/// This is for parsing command-line
	/// options like "-name foo -type theType -v -d".  This parses a string[]
	/// (like main usually takes) and collects argument information
	/// based on a parse specification.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The UsageSpec endCapStyle parse is unfinished.  
	/// Basically the UsageSpec endCapStyle parse is more expressive than the
	/// ParseSpec endCapStyle parse (see below).  But in this current implementation,
	/// none of the additional expressiveness is used.  In other words
	/// this class does not currently do anything with the optional/required
	/// switches (-file [-help]) and does not do anything with the pipe
	/// operator.
	/// </para>
	/// <para>
	/// This takes two types of parse specifications, a perl getopts endCapStyle
	/// such as "file=s, type:s, v, d" (see the ParseSpec property), 
	/// or a unix usage endCapStyle such as
	/// "-file fileName -type [typeName] [-v|-d]" (see the UsageSpec property).
	/// </para>
	/// <para>
	/// For the ParseSpec property, the rules are as follows:
	/// 1) The string is comma-separated like "file=s, type:s, v, d"
	/// 2) = means must have an argument
	/// 3) : means may have an argument
	/// </para>
	/// <para>
	/// For the UsageSpec property, the rules are as follows:
	/// 1) Optional arguments must be enclosed in square brackets [].
	/// 2) Argument names must be all word characters (- is considered to be a 
	///    word character).
	/// </para>
	/// <para>
	/// 
	/// </para>
	/// <para>
	/// Here's an example use:
	/// <code>
	/// Opts opts = new Opts();
	/// opts.ParseSpec = "file=s, type:s, v, d");
	///  - or -
	/// opts.UsageSpec = "-file fileName -type [typeName] [-v|-d]";
	/// if (!opts.Parse(args, out errorMessage)) { // display error message and usage }
	/// if (opts.Options.ContainsKey("v")) // -v was specified
	/// if (!opts.Options.ContainsKey("file")) { error... // need -file specified }
	/// Console.WriteLine("-file specified is {0}", opts.Options["file"]);
	/// </code>
	/// </para>
	/// </remarks>
	public class Opts
	{
		// ----------------------------------------------------------------
		#region Private variables
		// ----------------------------------------------------------------

		/// <summary>
		/// Peer class for logging.
		/// </summary>
		private Logger log;

		/// <summary>
		/// Has all the info after the parse... presence, and arguments.
		/// </summary>
		private Hashtable options;

		/// <summary>
		/// After the parse, has the arguments which were unswitched
		/// (not after an option like -file).
		/// </summary>
		private ArrayList unswitchedArgs;

		/// <summary>
		/// The specification.
		/// </summary>
		private string parseSpec;

		/// <summary>
		/// Set from the parseSpec. This is for options which require arguments.
		/// </summary>
		private ArrayList requireArg;

		/// <summary>
		/// Set from the parseSpec. This is for options which may have arguments.
		/// </summary>
		private ArrayList mayHaveArg;

		/// <summary>
		/// Set from the parseSpec. This is for options which do not have arguments.
		/// </summary>
		private ArrayList noArg;

		#endregion

		// ----------------------------------------------------------------------
		#region Properties
		// ----------------------------------------------------------------------

		/// <summary>
		/// The specification of what options to look for, which
		/// have arguments, etc.  This is the "name=s, type:s, v, d"
		/// endCapStyle.
		/// </summary>
		public string ParseSpec
		{
			get { return(parseSpec); }
			set 
			{ 
				Initialize();
				string[] words = value.Split(',');
				char[] typeChars = {':', '='};
				foreach (string word in words)
				{
					int nameLen = word.IndexOfAny(typeChars);
					if (nameLen < 0) nameLen = word.Length;
					string name = word.Substring(0, nameLen).Trim();
					if (word.IndexOf(':') >= 0)
						mayHaveArg.Add(name);
					else if (word.IndexOf('=') >= 0)
						requireArg.Add(name);
					else noArg.Add(name);
				}
				parseSpec = value; 
			}
		}

		/// <summary>
		/// The specification of what options to look for, which
		/// have arguments, etc.  This is the "-name fileName [-v|-d]"
		/// endCapStyle.
		/// </summary>
		public string UsageSpec
		{
			get { return(parseSpec); }
			set 
			{ 
				Initialize();
				StreamTokenizer tokenizer = new StreamTokenizer();
				tokenizer.Verbosity = VerbosityLevel.Warn;
				tokenizer.Settings.WordChar('-');
				ArrayList tokens = new ArrayList();
				tokenizer.TokenizeString(value, tokens);
				StringBuilder sb = new StringBuilder();

				for (int i = 0; i < tokens.Count; i++)
				{
					Token t = (Token)tokens[i];
					Token nextToken = null;
					if (i + 1 < tokens.Count) nextToken = (Token)tokens[i + 1];

					if ((t is WordToken) && (IsSwitch(t.StringValue)))
					{
						// it's a switch
						string name = t.StringValue.Substring(1); // drop the -
						if ((nextToken != null) && (nextToken is WordToken)
							&& (!IsSwitch(nextToken.StringValue)))
						{
							requireArg.Add(name);
							if (sb.Length > 0) sb.Append(",");
							sb.Append(name + "=s");
						}
						else if ((nextToken == '[') && (i + 2 < tokens.Count))
						{
							Token twoAhead = (Token)tokens[i + 2];
							if ((twoAhead is WordToken) && (!IsSwitch(twoAhead.StringValue)))
							{
								// optional
								mayHaveArg.Add(name);
								if (sb.Length > 0) sb.Append(",");
								sb.Append(name + ":s");
							}
						}
						else
						{
							// switch with no arg
							noArg.Add(name);
							if (sb.Length > 0) sb.Append(",");
							sb.Append(name);
						}
					}
				}
				parseSpec = sb.ToString();
			}
		}

		/// <summary>
		/// This hashtable is built during Parse.  This contains the results
		/// of the parse for switches (options). Options which don't take
		/// arguments will map to bool True.  Options which may have arguments
		/// will map to the argument string if present, and null if not present.
		/// Options which must have arguments will map to the argument string
		/// if the Parse succeeds.
		/// </summary>
		public Hashtable Options 
		{
			get { return(options); }
		}

		/// <summary>
		/// These are the rest of the command line arguments which are
		/// not associated with options (switches like "-file").
		/// </summary>
		public ArrayList UnswitchedArgs
		{
			get { return(unswitchedArgs); }
		}

		/// <summary>
		/// The verbosity level for this object's Logger.
		/// </summary>
		public VerbosityLevel Verbosity 
		{ 
			get { return(log.Verbosity); } 
			set { log.Verbosity = value; } 
		}

		#endregion

		// ---------------------------------------------------------------------
		#region Constructors/Destructor
		// ---------------------------------------------------------------------

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Opts()
		{
			Initialize();
		}

		/// <summary>
		/// Utility function, things common to constructors.
		/// </summary>
		void Initialize()
		{
			log = new Logger("Opts");
			options = new Hashtable();
			unswitchedArgs = new ArrayList();
			requireArg = new ArrayList();
			mayHaveArg = new ArrayList();
			noArg = new ArrayList();
		}

		#endregion

		// ---------------------------------------------------------------------
		#region Standard Methods
		// ---------------------------------------------------------------------

		/// <summary>
		/// Display the state of this object.
		/// </summary>
		public void Display()
		{
			Display(String.Empty);
		}

		/// <summary>
		/// Display the state of this object, with a per-line prefix.
		/// </summary>
		/// <param name="prefix">The pre-line prefix.</param>
		public void Display(string prefix)
		{
			log.WriteLine(prefix + "Opts display:");
			log.WriteLine(prefix + "    parseSpec: {0}", parseSpec);
			log.WriteLine(prefix + "    options: ");
			foreach(DictionaryEntry entry in options)
			{
				log.WriteLine(prefix + "         {0} -> {1}", entry.Key, entry.Value);
			}
			log.WriteLine(prefix + "    unswitched args: ");
			foreach(string s in unswitchedArgs)
			{
				log.WriteLine(prefix + "         {0}", s);
			}
		}

		/// <summary>
		/// Display the information gained from the parseSpec.
		/// </summary>
		public void DisplayParseSpec()
		{
			Display(String.Empty);
		}

		/// <summary>
		/// Display the information gained from the parseSpec.
		/// </summary>
		/// <param name="prefix">A prefix to prepend to each line.</param>
		public void DisplayParseSpec(string prefix)
		{
			log.WriteLine(prefix + "parseSpec: {0}", parseSpec);
			log.WriteLine(prefix + "    mayHaveArg: ");
			foreach(string s in mayHaveArg)
				log.WriteLine(prefix + "         {0}", s);
			log.WriteLine(prefix + "    requireArg: ");
			foreach(string s in requireArg)
				log.WriteLine(prefix + "         {0}", s);
			log.WriteLine(prefix + "    noArg: ");
			foreach(string s in noArg)
				log.WriteLine(prefix + "         {0}", s);
		}

		#endregion

		// ---------------------------------------------------------------------
		#region main Methods
		// ---------------------------------------------------------------------

		/// <summary>
		/// Utility method to determine whether a string is a switch or not.
		/// This currently just checks if it starts with a - which is not
		/// followed by a digit.
		/// </summary>
		/// <param name="s">The string to test.</param>
		/// <returns>bool - true for yes it's a switch</returns>
		private bool IsSwitch(string s)
		{
			if ((s == null) || (s.Length < 2)) return(false);
			if ((s[0] == '-') && (!Char.IsDigit(s[1]))) return(true);
			return(false);
		}

		/// <summary>
		/// Parse the options string[], determine if the parse spec
		/// requirements are met, and provide an error message string
		/// if not.
		/// </summary>
		/// <param name="args">The string[] to parse.</param>
		/// <param name="errorMessage">Output error message. This
		/// is set if the input options don't have all the pieces
		/// required by the parseSpec string.</param>
		/// <returns>bool - true if parseSpec requirements are met, 
		/// false otherwise.</returns>
		public bool Parse(string[] args, out string errorMessage)
		{
			errorMessage = null;
			for (int i = 0; i < args.Length; i++)
			{
				string s = args[i].Trim();
				if (IsSwitch(s))
				{
					// it's a switch
					string name = s.Substring(1);
					if (requireArg.Contains(name))
					{
						if (i + 1 < args.Length)
						{
							if (!IsSwitch(args[i + 1]))
							{
								options[name] = args[i + 1].Trim();
								i++;
							}
							else
							{
								errorMessage = String.Format("Option '{0}' requires an argument, but next word is {1}",
									name, args[i + 1]);
								return(false);
							}
						}
						else
						{
							errorMessage = String.Format("Option '{0}' requires an argument, but "
								+ "there are no words after it", name);
							return(false);
						}
					}
					else if (mayHaveArg.Contains(name))
					{
						options[name] = null;
						if (i + 1 < args.Length)
						{
							if (args[i + 1].Trim()[0] != '-')
							{
								options[name] = args[i + 1].Trim();
								i++;
							}
						}
					}
					else if (noArg.Contains(name))
					{
						options[name] = true;
					}
					else
					{
						// it's a switch, but wasn't in parse spec
						errorMessage = String.Format("Option '{0}' isn't a recognized switch.", name);
						return(false);
					}
				}
				else
				{
					// not a switch
					unswitchedArgs.Add(s);
				}
			}
			return(true);
		}

		#endregion

		// ---------------------------------------------------------------------
		#region TestSelf
		// ---------------------------------------------------------------------
		/// <summary>
		/// Simple self test.
		/// </summary>
		/// <returns>bool - true for success, false for failure.</returns>
		public static bool TestSelf()
		{
			Logger log = new Logger("testSelf");
			log.Verbosity = VerbosityLevel.Debug;
			log.Info("Starting...");

			//
			// try parseSpec endCapStyle
			//
			string parseSpec = "file=s, type:s, v, d, number=s";
			string[] testArgs = {"-file", "fileName", "-type", "-v", "hello", 
				"-number", "-3.2"};
			string errorMessage = null;

			Opts opts = new Opts();
			opts.ParseSpec = parseSpec;
			log.Info("ParseSpec:");
			opts.DisplayParseSpec("    ");

			if (!opts.Parse(testArgs, out errorMessage))
			{
				log.Error("Unable to parse test string.");
				log.Error("Error message is '{0}'", errorMessage);
				return(false);
			}

			log.Info("After parse:");
			opts.Display("    ");

			// check results
			if (opts.Options["file"] == null)
			{
				log.Error("Parse didn't get -file option");
				return(false);
			}
			log.Info("-file argument was {0}", opts.Options["file"]);

			if (opts.Options.ContainsKey("v")) log.Info("-v option was found");
			else
			{
				log.Error("Parse didn't find -v option");
				return(false);
			}
			
			if (opts.Options.ContainsKey("type"))
			{
				log.Info("-type arg was '{0}'", opts.Options["type"]);
			}
			else
			{
				log.Error("Parse didn't find -type option");
				return(false);
			}

			//
			// try usageSpec endCapStyle
			//
			log.Info("----------------------------------------------------------");
			log.Info("UsageSpec style of use.");
			string usageSpec = "cmd -file fileName -type [typeName] [-v|-d] [-number num] file ...";
			log.Info("Input usage: {0}", usageSpec);
			opts.UsageSpec = usageSpec;
			log.Info("UsageSpec:");
			opts.DisplayParseSpec("    ");

			if (!opts.Parse(testArgs, out errorMessage))
			{
				log.Error("Unable to parse test string.");
				log.Error("Error message is '{0}'", errorMessage);
				return(false);
			}

			if (opts.Options.ContainsKey("v"))
			{
				log.Info("-v was specified");
			}

			log.Info("After parse:");
			opts.Display("    ");

			// done
			log.Info("Done.");
			return(true);
		}
		#endregion
	}
}


