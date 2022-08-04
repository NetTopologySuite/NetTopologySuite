---
uid: NetTopologySuite.Coverage
summary: *content
---
Contains classes that operate on polygonal coverages.
 
A polygonal coverage is a non-overlapping, fully-noded set of polygons.
Specifically, a set of polygons is a valid coverage if:

1. The interiors of all polygons are disjoint. This is the case if no polygon has a boundary which intersects the interior of another polygon.
2. Where polygons are adjacent (their boundaries intersect), the vertices (and thus line segments) of the common boundary match exactly.

Coverage algorithms (such as <xref href="NetTopologySuite.Coverage.CoverageUnion">CoverageUnion</xref>) 
generally require the input coverage to be valid to produce correct results.
Coverages can be validated using {@link CoverageValidator}.