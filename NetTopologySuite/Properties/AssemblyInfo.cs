using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("NetTopologySuite")]
[assembly: AssemblyDescription("A .NET library for GIS operations, direct porting of JTS Topology Suite 1.13 library")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Stable")]
#endif
[assembly: AssemblyCompany("NetTopologySuite Team")]
[assembly: AssemblyProduct("NTS Topology Suite")]
[assembly: AssemblyCopyright("Copyright © 2006 - 2013 Diego Guidi, John Diss (www.newgrove.com), Felix Obermaier (www.ivv-aachen.de), Todd Jackson")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyDelaySign(false)]
[assembly: CLSCompliant(true)]
#if HAS_SYSTEM_RUNTIME_INTEROPSERVICES_COMVISIBLEATTRIBUTE
[assembly: ComVisible(false)]
#endif
#if HAS_SYSTEM_RUNTIME_INTEROPSERVICES_GUIDATTRIBUTE
[assembly: Guid("6B7EB658-792E-4178-B853-8AEB851513A9")]
#endif
//[assembly: InternalsVisibleTo("NetTopologySuite.Silverlight.Test", AllInternalsVisible = true)]

[assembly: InternalsVisibleTo("NetTopologySuite.Tests.NUnit, PublicKey=" + Consts.PublicKeyToken)]
