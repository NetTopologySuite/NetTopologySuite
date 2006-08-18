This is NetTopologySuite, a C# porting of JTS 1.7.1 for java
I have used VS 2005 and 2.0 Framework, and from this version the code is 2.0 compliant only.

At this version (1.7.1) i've inserted this features:
JTS 1.7.1 porting of all algorithms and features.
GeoTools.NET shapefiles read/write capabilities.
SharpMap ( http://www.codeplex.com/Wiki/View.aspx?ProjectName=SharpMap ) CoordinateSystems management

JTS 1.7.1 ships with a internal WKB Reader/Writer, that is different that native implementation already present in NTS.
I've decided to maintain my implementation, that is well tested and works well, but if anyone desire JTS implementation...

TODO: GML Reader/Writer

Please send me any corrections and all suggestions to diego.guidi@gmail.com

Bye :)