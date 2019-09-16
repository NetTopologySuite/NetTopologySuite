---
uid: NetTopologySuite.Operation.Overlay
summary: *content
---
Contains classes that perform a topological overlay to compute boolean spatial functions.

The Overlay Algorithm is used in spatial analysis methods for computing set-theoretic operations (boolean combinations) of input <xref href="NetTopologySuite.Geometries.Geometry">Geometries</xref>. The algorithm for computing the overlay uses the intersection operations supported by topology graphs. To compute an overlay it is necessary to explicitly compute the resultant graph formed by the computed intersections.

The algorithm to compute a set-theoretic spatial analysis method has the following steps:

- Build topology graphs of the two input geometries. For each geometry all self-intersection nodes are computed and added to the graph.
- Compute nodes for all intersections between edges and nodes of the graphs.
- Compute the labeling for the computed nodes by merging the labels from the input graphs.
- Compute new edges between the compute intersection nodes. Label the edges appropriately.
- Build the resultant graph from the new nodes and edges.
- Compute the labeling for isolated components of the graph. Add the isolated components to the resultant graph.
- Compute the result of the boolean combination by selecting the node and edges with the appropriate labels. Polygonize areas and sew linear geometries together.

# Package Specification
- Java Topology Suite Technical Specifications
- [OpenGIS Simple Features Specification for SQL](http://www.opengis.org/techno/specs.htm)
