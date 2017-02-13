using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !PCL
using System.Security.Cryptography.X509Certificates;
#endif
// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die mit einer Assembly verknüpft sind.
[assembly: AssemblyTitle("NetTopologySuite.IO")]
[assembly: AssemblyDescription("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Stable")]
#endif
[assembly: AssemblyCompany("NetTopologySuite - Team")]
[assembly: AssemblyProduct("NetTopologySuite.IO")]
[assembly: AssemblyCopyright("Copyright © 2012-2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar 
// für COM-Komponenten. Wenn Sie auf einen Typ in dieser Assembly von 
// COM zugreifen müssen, legen Sie das ComVisible-Attribut für diesen Typ auf "true" fest.
#if !PCL40
[assembly: ComVisible(false)]
#endif
#if !PCL
// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
[assembly: Guid("676188be-f97b-45da-ab4c-fb0b92284a84")]
[assembly: InternalsVisibleTo("NetTopologySuite.IO.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e5a9697e3d378de4bdd1607b9a6ea7884823d3909f8de55b573416d9adb0ae25eebc39007d71a7228c500d6e846d54dcc2cd839056c38c0a5e86b73096d90504f753ea67c9b5e61ecfdb8edf0f1dfaf0455e9a0f9e124e16777baefcda2af9a5a9e48f0c3502891c79444dc2d75aa50b75d148e16f1401dcb18bc1638cc764a9", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("NetTopologySuite.IO.Streams.CloudStorage.Test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e5a9697e3d378de4bdd1607b9a6ea7884823d3909f8de55b573416d9adb0ae25eebc39007d71a7228c500d6e846d54dcc2cd839056c38c0a5e86b73096d90504f753ea67c9b5e61ecfdb8edf0f1dfaf0455e9a0f9e124e16777baefcda2af9a5a9e48f0c3502891c79444dc2d75aa50b75d148e16f1401dcb18bc1638cc764a9")]

#endif