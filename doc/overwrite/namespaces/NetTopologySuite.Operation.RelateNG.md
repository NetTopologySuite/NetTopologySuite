---
uid: NetTopologySuite.Operation.RelateNG
summary: *content
---
Provides classes to implement the RelateNG algorithm
computes topological relationships of <xref href="NetTopologySuite.Geometries.Geometry">Geometry</xref>s.   
Topology is evaluated based on the 
<a href="https://en.wikipedia.org/wiki/DE-9IM">Dimensionally-Extended 9-Intersection Model</a> (DE-9IM).  
The <xref href="NetTopologySuite.Operation.RelateNG.RelateNG">RelateNG</xref> class supports computing the value of boolean topological predicates
Standard OGC named predicates are provided by the <xref href="NetTopologySuite.Operation.RelateNG.RelatePredicateFactory">RelatePredicateFactory</xref> functions. 
Custom relationships can be specified via testing against DE-9IM matrix patterns
(see <xref href="NetTopologySuite.Operation.RelateNG.IntersectionMatrixPattern">IntersectionMatrixPattern</xref> for examples).
The full DE-9IM <xref href="NetTopologySuite.Operation.RelateNG.IntersectionMatrix">IntersectionMatrix</xref> can also be computed.

The algorithm has the following capabilities:
* Efficient short-circuited evaluation of topological predicates
  (including matching custom DE-9IM patterns)
* Optimized repeated evaluation of predicates against a single geometry 
  via cached spatial indexes (AKA "prepared mode")
* Robust computation (since only point-local topology is required,
  so that invalid geometry topology cannot cause failures)
* Support for mixed-type and overlapping {@link GeometryCollection} inputs
  (using _union semantics_)
* Support for <xref href="NetTopologySuite.Algorithm.IBoundaryNodeRule">IBoundaryNodeRule</xref>

RelateNG operates in 2D only; it ignores any Z ordinates.

### Optimized Short-Circuited Evaluation
The RelateNG algorithm uses strategies to optimize the evaluation of
topological predicates, including matching DE-9IM matrix patterns.
These include fast tests of dimensions and envelopes, and short-circuited evaluation 
once the predicate value is known
(either satisfied or failed) based on the value of matrix entries.
Named predicates used explicit strategy code.
DE-9IM matrix pattern matching are short-circuited where possible 
based on analysis of the pattern matrix entries.
Spatial indexes are used to optimize topological computations
(such as locating points in geometry elements, 
and analyzing the topological relationship between geometry edges). 

### Execution Modes
RelateNG provides two execution modes for evaluating predicates:
* **Single-shot** mode evaluates a predicate for a single case of two geometries.
  It is provided by the <xref href="NetTopologySuite.Operation.RelateNG.RelateNG">RelateNG</xref> static functions which take two input geometries.
* **Prepared** mode optimizes repeated evaluation of predicates 
  against a fixed geometry. 
  It is used by creating an instance of <xref href="NetTopologySuite.Operation.RelateNG.RelateNG">RelateNG</xref> 
  on the required geometry with the `Prepare` functions, 
  and then using the `Evaluate` methods.
  It provides much faster performance for repeated operations against a single geometry.

### Robustness
RelateNG provides robust evaluation of topological relationships,
up to the precision of double-precision computation.
It computes topological relationships in the locality of discrete points, 
without constructing a full topology graph of the inputs.
This means that invalid input geometries or numerical round-off do not cause exceptions
(although they may return incorrect answers).
However, it is necessary to node some inputs together (in particular, linear elements)
in order to provide consistent evaluation of the topological structure.

### GeometryCollection Handling
<xref href="NetTopologySuite.Geometries.GeometryCollection">GeometryCollection</xref>s may contain geometries of different dimensions, nested to any level.
The element geometries may overlap in any combination.
The OGC specification did not provide a definition for the topology
of GeometryCollections, or how they behave under the DE-9IM model.
RelateNG defines the topology for arbitrary collections of geometries
using "union semantics".
This is specified as:
* GeometryCollections are evaluated as if they were replaced by the topological union
  of their elements.
* The topological location at a point is equal to its location in the geometry of highest
  dimension which contains it.  For example, a point located in the interior of a Polygon
  and the boundary of a LineString has location `Interior`.

### Zero-length LineString Handling
Zero-length LineStrings are handled as topologically identical to a Point at the same coordinate. 
 
## Package Specification
* <a href="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features Specification for SQL</a>
