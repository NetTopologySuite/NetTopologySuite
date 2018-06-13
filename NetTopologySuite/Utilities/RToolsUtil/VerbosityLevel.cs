// VerbosityLevel.cs
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
//    for commercial purposes, provided that this copyright and license
//    notice is included verbatim.
//
// Also, in return for using this code, please attempt to make your fixes and
// updates available in some way, such as by sending your updates to the
// author.

namespace RTools_NTS.Util
{
    /// <summary>
    /// This enumerates verbosity levels.
    /// </summary>
    public enum VerbosityLevel
    {
        /// <summary>For error messages.</summary>
        Error,
        /// <summary>For warn messages.</summary>
        Warn,
        /// <summary>For info messages.</summary>
        Info,
        /// <summary>For debug messages.</summary>
        Debug,
    }
}
