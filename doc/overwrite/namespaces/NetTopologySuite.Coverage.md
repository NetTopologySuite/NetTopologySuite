---
uid: NetTopologySuite.Coverage
summary: *content
---
Contains classes that operate on polygonal coverages.
 
A polygonal coverage is a set of polygonal geometries which is non-overlapping and edge-matched
(<xref href="NetTopologySuite.Geometries.Polygon">Polygon</xref> or <xref href="NetTopologySuite.Geometries.MultiPolygon">MultiPolygon</xref>)
A set of polygonal geometries is a valid coverage if:

1. Each geometry is valid.
2. The interiors of all polygons are disjoint (they are not overlapping). 
   This is the case if no polygon has a boundary which intersects the interior of another polygon.
3. Where polygons are adjacent (i.e. their boundaries intersect), 
   they are <b>edge matched</b>: the vertices 
   (and thus line segments) of the common boundary match exactly.

A coverage may contain holes and disjoint regions.

Coverage algorithms (such as <xref href="NetTopologySuite.Coverage.CoverageUnion">CoverageUnion</xref>) 
generally require the input coverage to be valid to produce correct results.
Coverages can be validated using {@link CoverageValidator}.