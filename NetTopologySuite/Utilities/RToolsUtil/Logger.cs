// Logger.cs
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

using System;

namespace RTools_NTS.Util
{
	/// <summary>
	/// This is a simple implementation of a Logger class.
	/// The purpose is to threshold output based on a verbosity setting,
	/// format messages similarly, and collect all message writes such that
	/// they can be redirected easily.  You (and I) should probably use
	/// the some other library for this, but I haven't found one that suits
	/// my needs.
	/// </summary>
	/// <remarks>
	/// This doesn't implement much of the functionality possible
	/// with this interface.  This could redirect messages to other
	/// text writers, forward messages to subscribers, etc.
	/// </remarks>
	public class Logger
	{
        // NOTE: modified for "safe" assembly in Sql 2005
        // Readonly added!

		///<summary>Backer for Log.</summary>
		protected static readonly Logger log = new Logger(String.Empty);

		///<summary>
		///A static instance you can use without creating your own.
		///</summary>
		public static Logger Log { get { return(log); } }

		///<summary>The name is prepended to all messages. </summary>
		protected string name;

		/// <summary>
		/// The verbosity of this logger.  Messages are filtered
		/// based on this setting.
		/// </summary>
		protected VerbosityLevel verbosity;

		/// <summary>
		/// The verbosity of this logger.  Messages are filtered
		/// based on this setting.
		/// </summary>
		public VerbosityLevel Verbosity
		{
			get { return(verbosity); }
			set { verbosity = value; }
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Logger()
		{
			name = String.Empty;
		}

		/// <summary>
		/// Construct and set name.
		/// </summary>
		/// <param name="name">The name for this logger.  This name
		/// is prepended to output messages (except Out messages).</param>
		public Logger(string name)
		{
			this.name = name;
			this.verbosity = VerbosityLevel.Debug;
		}

		/// <summary>
		/// Write a string with no verbosity checking and no formatting.
		/// </summary>
		/// <param name="msg">The format string.</param>
		/// <param name="args">The arguments.</param>
		public void Write(string msg, params object[] args)
		{			
		}

		/// <summary>
		/// Write a line with no verbosity checking and no formatting.
		/// </summary>
		/// <param name="msg">The format string.</param>
		/// <param name="args">The arguments.</param>
		public void WriteLine(string msg, params object[] args)
		{		
		}

		/// <summary>
		/// Write a string of this particular verbosity.
		/// This will not output the string unless the verbosity is
		/// greater than or equal to this object's threshold.
		/// This prepends the level of the message and
		/// the name of this Logger.
		/// </summary>
		/// <param name="msg">The format string.</param>
		/// <param name="args">The arguments.</param>
		public void Debug(string msg, params object[] args)
		{
			if (verbosity >= VerbosityLevel.Debug)
			{				
			}
		}

		/// <summary>
		/// Write a string of this particular verbosity.
		/// This will not output the string unless the verbosity is
		/// greater than or equal to this object's threshold.
		/// This prepends the level of the message and
		/// the name of this Logger.
		/// </summary>
		/// <param name="msg">The format string.</param>
		/// <param name="args">The arguments.</param>
		public void Info(string msg, params object[] args)
		{
			if (verbosity >= VerbosityLevel.Info)
			{				
			}
		}

		/// <summary>
		/// Write a string of this particular verbosity.
		/// This will not output the string unless the verbosity is
		/// greater than or equal to this object's threshold.
		/// This prepends the level of the message and
		/// the name of this Logger.
		/// </summary>
		/// <param name="msg">The format string.</param>
		/// <param name="args">The arguments.</param>
		public void Warn(string msg, params object[] args)
		{
			if (verbosity >= VerbosityLevel.Warn)
			{				
			}
		}

		/// <summary>
		/// Write a string of this particular verbosity.
		/// This will not output the string unless the verbosity is
		/// greater than or equal to this object's threshold.
		/// This prepends the level of the message and
		/// the name of this Logger.
		/// </summary>
		/// <param name="msg">The format string.</param>
		/// <param name="args">The arguments.</param>
		public void Error(string msg, params object[] args)
		{
			if (verbosity >= VerbosityLevel.Error)
			{				
			}
		}

		/// <summary>
		/// A simple static self test method.
		/// </summary>
		/// <returns>bool - currently always true</returns>
		public static bool TestSelf()
		{
			Logger log = new Logger("Logger: TestSelf");
			log.Debug("A debug message.");
			log.Info("An info message.");
			log.Warn("A warn message.");
			log.Error("An error message.");

			log.Write("Some writes: ");
			for (int i = 0; i < 5; i++) log.Write("{0},", i);
			log.WriteLine(String.Empty);

			log.WriteLine("This WriteLine message will always be displayed.");
			return(true);
		}
	}
}
