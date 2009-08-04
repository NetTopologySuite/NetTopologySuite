using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("NetTopologySuite")]
[assembly: AssemblyDescription("A CLR library for geometry and spatial operations, " +
                               "derived from JTS - the Java Topology Suite v1.7.2 through v1.9.")]
#if NETCF
#if DEBUG
#if UNSAFE
[assembly: AssemblyConfiguration("CompactFramework Performance Debug")]
#else
[assembly: AssemblyConfiguration("CompactFramework Safe Debug")]
#endif
#else
#if UNSAFE
[assembly: AssemblyConfiguration("CompactFramework Performance Release")]
#else
[assembly: AssemblyConfiguration("CompactFramework Safe Release")]
#endif
#endif
#else
#if DEBUG
#if UNSAFE
[assembly: AssemblyConfiguration("Performance Debug")]
#else

[assembly: AssemblyConfiguration("Safe Debug")]
#endif
#else
#if UNSAFE
[assembly: AssemblyConfiguration("Performance Release")]
#else
[assembly: AssemblyConfiguration("Safe Release")]
#endif
#endif
#endif

[assembly: AssemblyCopyright("Copyright © 2006-2008 Diego Guidi, Rory Plaire")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyDelaySign(false)]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: Guid("6B7EB658-792E-4178-B853-8AEB851513A9")]