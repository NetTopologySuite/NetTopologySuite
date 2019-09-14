---
uid: NetTopologySuite.Operation.Relate
summary: *content
---
Contains classes to implement the computation of the spatial relationships of Geometrys.

The relate algorithm computes the IntersectionMatrix describing the relationship of two Geometrys. The algorithm for computing relate uses the intersection operations supported by topology graphs. Although the relate result depends on the resultant graph formed by the computed intersections, there is no need to explicitly compute the entire graph. It is sufficient to compute the local structure of the graph at each intersection node.

The algorithm to compute relate has the following steps:
- Build topology graphs of the two input geometries. For each geometry all self-intersection nodes are computed and added to the graph.
- Compute nodes for all intersections between edges and nodes of the graphs.
- Compute the labeling for the computed nodes by merging the labels from the input graphs.
- Compute the labeling for isolated components of the graph (see below)
- Compute the IntersectionMatrix from the labels on the nodes and edges.
- Labeling isolated components

Isolated components are components (edges or nodes) of an input Geometry which do not contain any intersections with the other input Geometry. The topological relationship of these components to the other input Geometry must be computed in order to determine the complete labeling of the component. This can be done by testing whether the component lies in the interior or exterior of the other Geometry. If the other Geometry is 1-dimensional, the isolated component must lie in the exterior (since otherwise it would have an intersection with an edge of the Geometry). If the other Geometry is 2-dimensional, a Point-In-Polygon test can be used to determine whether the isolated component is in the interior or exterior.

# Package Specification
- Java Topology Suite Technical Specifications
- [OpenGIS Simple Features Specification for SQL](http://www.opengis.org/techno/specs.htm)
