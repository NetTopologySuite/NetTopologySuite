---
uid: NetTopologySuite.Algorithm
summary: *content
---
Contains classes and interfaces implementing fundamental computational geometry algorithms.

## Robustness
Geometrical algorithms involve a combination of combinatorial and numerical computation. As with all numerical computation using finite-precision numbers, the algorithms chosen are susceptible to problems of robustness. A robustness problem occurs when a numerical calculation produces an incorrect answer for some inputs due to round-off errors. Robustness problems are especially serious in geometric computation, since they can result in errors during topology building.

There are many approaches to dealing with the problem of robustness in geometrical computation. Not surprisingly, most robust algorithms are substantially more complex and less performant than the non-robust versions. Fortunately, NTS is sensitive to robustness problems in only a few key functions (such as line intersection and the point-in-polygon test). There are efficient robust algorithms available for these functions, and these algorithms are implemented in NTS.

## Computational Performance
Runtime performance is an important consideration for a production-quality implementation of geometric algorithms. The most computationally intensive algorithm used in NTS is intersection detection. NTS methods need to determine both all intersection between the line segments in a single Geometry (self-intersection) and all intersections between the line segments of two different Geometries.

The obvious naive algorithm for intersection detection (comparing every segment with every other) has unacceptably slow performance. There is a large literature of faster algorithms for intersection detection. Unfortunately, many of them involve substantial code complexity. NTS tries to balance code simplicity with performance gains. It uses some simple techniques to produce substantial performance gains for common types of input data.

# Package Specification
- Java Topology Suite Technical Specifications
- [OpenGIS Simple Features Specification for SQL](http://www.opengis.org/techno/specs.htm)
