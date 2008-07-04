// Finder.cs
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
//    Change over to use Opts
//

using System;
using System.Collections;
using System.IO;

namespace RTools_NTS.Util
{
	/// <summary>
	/// This class provides some static methods which are useful
	/// for file system "find" types of operations, similar to the *nix find
	/// command.
	/// </summary>
	public class Finder
	{
		/// <summary>
		/// Switch from backslashes to forward slashes.
		/// Zip file format specifies only forward slashes.
		/// </summary>
		/// <param name="s">The input string.</param>
		/// <returns>string - with backslashes replaced by forward.</returns>
		public static string ChangeBackSlashes(string s)
		{
			char[] buffer = new Char[s.Length];
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '\\') buffer[i] = '/';
				else buffer[i] = s[i];
			}
			return(new string(buffer));
		}

		/// <summary>
		/// Find files under a specified directory and matching any of
		/// a set of regular expressions. Return them by putting
		/// (string -> null) entries into the input SortedList.
		/// The regular expressions are Command Prompt dir command 
		/// type (like *.cs), not normal regular expressions.
		/// This optionally puts the directory name itself into the list.
		/// </summary>
		/// <remarks>This uses SortedList to get the insertion sort.</remarks>
		/// <param name="dirName">The directory to find files under. "." works.</param>
		/// <param name="regexps">List of directory regexp strings, for example *.cs
		/// and *.bat</param>
		/// <param name="list">The SortedList to fill.</param>
		/// <param name="addDirItself">Whether to add the dir name into the list.</param>
		/// <returns>bool - true for success, false for failure</returns>
		public static bool FindFiles(string dirName, ArrayList regexps, 
			ref SortedList list, bool addDirItself)
		{
			DirectoryInfo dir = new DirectoryInfo(dirName);
			if (!dir.Exists) return(false);
			if (addDirItself) list.Add(dirName, null);

			// list directories: this can fail via exception
			DirectoryInfo[] dirs = null;
			try
			{
				dirs = dir.GetDirectories();
			}
			catch(UnauthorizedAccessException)
			{				
				return(false);
			}

			foreach (DirectoryInfo d in dirs)
			{
				string subDirName = dirName + Path.DirectorySeparatorChar
					+ d.Name;				
			}

			foreach (string regexp in regexps)
			{
				foreach (FileInfo f in dir.GetFiles(regexp)) 
				{
					string s = dirName + Path.DirectorySeparatorChar + f.Name;
					list.Add(s, null);
				}
				foreach (string d in Directory.GetDirectories(dirName, regexp)) 
				{
					list.Add(d, null);
				}
			}

			return(true);
		}

		/// <summary>
		/// Overload with single regular expression.
		/// </summary>
		public static bool FindFiles(string dirName, string regexp, ref SortedList list,
			bool addDirItself)
		{
			ArrayList regexps = new ArrayList();
			regexps.Add(regexp);
			return(FindFiles(dirName, regexps, ref list, addDirItself));
		}

		/// <summary>
		/// For each entry in the input SortedList (where the keys in the
		/// list entries are strings, the file paths) add all parent 
		/// directories into the list.  So given a list with an entry
		/// with key "a/b/c", put "a/b" and "a" in the list.
		/// </summary>
		/// <remarks>The new entries are string dir name with value null.
		/// </remarks>
		/// <param name="list">The SortedList of DictionaryEntry's, with
		/// string file path keys.</param>
		/// <returns>true</returns>
		public static bool AddParents(SortedList list)
		{
			ArrayList parents = new ArrayList();

			foreach(DictionaryEntry entry in list)
			{
				string dir = Path.GetDirectoryName((string)entry.Key);
				while ((dir != null) && (dir.Length > 1))
				{
					parents.Add(dir);
					dir = Path.GetDirectoryName(dir);
				}
			}

			foreach(string p in parents)
			{
				if (!list.ContainsKey(p)) list.Add(p, null);
			}
			return(true);
		}
	}
}
