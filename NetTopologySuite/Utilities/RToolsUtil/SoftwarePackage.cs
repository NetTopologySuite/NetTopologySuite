// SoftwarePackage.cs
// 
// Copyright (C) 2003-2004 Ryan Seghers
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
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;

namespace RTools_NTS.Util
{
	/// <summary>
	/// This class represents an installed software package on a Windows
	/// system.  This has some static utility methods that will get the
	/// list of installed software packages, letting you uninstall one.
	/// </summary>
	/// <remarks>
	/// <para>This is motivated by the desire to get the version of an installed
	/// package, and by the fact that I wasn't able to find an easier way to 
	/// uninstall an Msi by product name (msiexec needs the product code).
	/// </para>
	/// <para>
	/// This looks for uninstallable packages in the registry in:
	/// HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall
	/// </para>
	/// </remarks>
	public class SoftwarePackage
	{
		#region Fields

		private string name;
		private string productCode;
		private string uninstallString;
		private string displayVersion;

		#endregion 

		#region Properties

		/// <summary>Product name.</summary>
		public string Name { get { return(name); } set { name = value; } }

		/// <summary>Product Code.</summary>
		public string ProductCode { get { return(productCode); } set { productCode = value; } }

		/// <summary>Uninstall string (a shell command to use to remove this SoftwarePackage).</summary>
		public string UninstallString 
		{ 
			get 
			{ 
				if (uninstallString != string.Empty)
					return(uninstallString); 
				else if (productCode != string.Empty)
				{
					return("msiexec.exe /I" + productCode);
				}
				else return(string.Empty);
			} 
			set { uninstallString = value; } 
		}

		/// <summary>The DisplayVersion for this SoftwarePackage.</summary>
		public string DisplayVersion { get { return(displayVersion); } set { displayVersion = value; } }

		/// <summary>Whether or not this SoftwarePackage is uninstallable (by this class).</summary>
		public bool IsUninstallable
		{
			get
			{
				if ((UninstallString != string.Empty) 
					|| (productCode != string.Empty)) return(true);
				else return(false);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SoftwarePackage()
		{
			name = string.Empty;
			productCode = string.Empty;
			uninstallString = string.Empty;
			displayVersion = string.Empty;
		}

		/// <summary>
		/// Constructor which sets all values.
		/// </summary>
		/// <param name="name">The product name.</param>
		/// <param name="productCode">The ProductCode.</param>
		/// <param name="uninstallString">The uninstall string.</param>
		/// <param name="displayVersion">The display version.</param>
		public SoftwarePackage(string name, string productCode, string uninstallString,
			string displayVersion)
		{
			if (name != null) this.name = name;
			else this.name = string.Empty;

			if (productCode != null) this.productCode = productCode;
			else this.productCode = string.Empty;

			if (uninstallString != null) this.uninstallString = uninstallString;
			else this.uninstallString = string.Empty;

			if (displayVersion != null) this.displayVersion = displayVersion;
			else this.displayVersion = string.Empty;
		}

		#endregion

		#region main Instance Methods

		/// <summary>
		/// Uninstall this setup.
		/// </summary>
		/// <param name="interactive">Whether or not to use the interactive
		/// interface.</param>
		/// <returns>bool - true for success, false for failure</returns>
		public bool Uninstall(bool interactive)
		{
			Process p = null;
			ProcessStartInfo psi = null;
			
			// build process start info
			string cmdLine;

			if (uninstallString == string.Empty) 
			{
				// no specific uninstall string, use mine
				if (productCode == String.Empty) return(false);
				cmdLine = "/x" + ProductCode + (interactive ? " /qf" : " /qb");
				psi = new ProcessStartInfo("msiexec.exe", cmdLine);
			}
			else if (uninstallString.ToLower().IndexOf("msiexec") >= 0)
			{
				// it's an msiexec command, try to enact the interactive spec
				if (productCode == String.Empty) cmdLine = uninstallString;
				else 
				{
					cmdLine = "/x" + ProductCode + (interactive ? " /qf" : " /qb");
					psi = new ProcessStartInfo("msiexec.exe", cmdLine);
				}
			}
			else
			{
				cmdLine = uninstallString;
			}

			// execute
			if (psi != null)
			{
				// Process endCapStyle				
				p = new Process();
				p.StartInfo = psi;
				p.Start();
				int timeoutMs = 2 * 60 * 1000;
				if (interactive) timeoutMs = Int32.MaxValue;
				p.WaitForExit(timeoutMs);
				return(p.ExitCode == 0);
				//return(true);
			}
			else
			{
				// Executor endCapStyle for arbitrary command line
				TempFileCollection tmpFiles = new TempFileCollection(".", false);
				Executor.ExecWait(cmdLine, tmpFiles);
				return(true);
			}
		}

		#endregion

		#region Standard Methods

		/// <summary>
		/// ToString override.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return(name + ", " + displayVersion + ", uninstallable: "
				+ IsUninstallable);
		}

		#endregion

		#region Static Utility Methods

		/// <summary>
		/// This returns an array of potentially unstallable SoftwarePackages.
		/// See SoftwarePackage.IsUninstallable to see whether this class
		/// can uninstall a particular SoftwarePackage.
		/// </summary>
		/// <returns>The SoftwarePackage[], null for error.</returns>
		public static SoftwarePackage[] GetList()
		{
			//
			// registry access
			//
			string uninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
			RegistryKey key = Registry.LocalMachine.OpenSubKey(uninstallKey);
			if (key == null)							
				return null;			

			// list them
			ArrayList list = new ArrayList();
			string[] subkeyNames = key.GetSubKeyNames();
			foreach(string subkey in subkeyNames)
			{
				RegistryKey sk = key.OpenSubKey(subkey);
				string prodName;
				string uninstallString = (string)sk.GetValue("UninstallString");
				string displayVersion = (string)sk.GetValue("DisplayVersion");
				string productCode = string.Empty;

				if (subkey[0] == '{')
				{
					// product code entry
					productCode = subkey;
					prodName = (string)sk.GetValue("DisplayName");
					if (prodName == null) prodName = string.Empty;
				}
				else 
				{
					// entry by name
					prodName = subkey;
				}
				list.Add(new SoftwarePackage(prodName, productCode, uninstallString, displayVersion));
			}
			return((SoftwarePackage[])list.ToArray(typeof(SoftwarePackage)));
		}

		/// <summary>
		/// Get a particular software package, by product name. This returns
		/// the first package found with the specified product name.
		/// </summary>
		/// <param name="productName">The product name of the package to 
		/// get.</param>
		/// <returns>SoftwarePackage</returns>
		public static SoftwarePackage GetPackage(string productName)
		{
			SoftwarePackage[] setups = GetList();
			foreach(SoftwarePackage s in setups)
			{
				if (s.Name.Equals(productName)) return(s);
			}
			return(null);
		}

		/// <summary>
		/// This returns the requested SoftwarePackage only if it only appears 
		/// exactly once (as determined solely by name) in the input SoftwarePackage
		/// array.
		/// </summary>
		/// <param name="name">The product name to look for.</param>
		/// <param name="list">The array of SoftwarePackages to search.</param>
		/// <returns>The SoftwarePackage found, or null for none found or more
		/// than one found.</returns>
		public static SoftwarePackage FindOne(string name, SoftwarePackage[] list)
		{
			SoftwarePackage setup = null;

			foreach(SoftwarePackage s in list)
			{
				if (s.Name.Equals(name)) 
				{
					if (setup != null) return(null);
					setup = s;
				}
			}
			return(setup);
		}

		/// <summary>
		/// Uninstall all uninstallable packages with the specified product name.
		/// </summary>
		/// <param name="productName">The product name of the packages to 
		/// uninstall.</param>
		/// <param name="interactive">Whether to run Msiexec in interactive mode
		/// or not.</param>
		/// <returns>bool - true for 1 or more found and uninstalled, false
		/// otherwise.</returns>
		public static bool UninstallMultiple(string productName, bool interactive)
		{
			SoftwarePackage[] setups = GetList();
			foreach(SoftwarePackage s in setups)
			{
				if (s.Name.Equals(productName) && s.IsUninstallable)
				{
					if (!s.Uninstall(interactive))
						return(false);
				}
			}
			return(true);
		}

		#endregion	
	}
}
