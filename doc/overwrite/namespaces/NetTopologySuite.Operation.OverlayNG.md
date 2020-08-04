---
uid: NetTopologySuite.Operation.OverlayNG
summary: *content
---
Contains classes that perform topological overlay to compute boolean spatial functions. Overlay operations are used in spatial analysis for computing set-theoretic operations (boolean combinations) of input {@link Geometry}s.

The {@link OverlayNG} class provides the standard Simple Features boolean set-theoretic overlay operations, including:
* Intersection
* Union
* Difference
* Symmetric Difference
 
These operations are supported for all combinations of the basic geometry types and their homogeneous collections.

Additional operations include:

* {@link UnaryUnion} unions collections of geometries in an efficient way
* {@link CoverageUnion} provides enhanced performance for unioning valid polygonal and lineal coverages
* {@link PrecisionReducer} allows reducing the precision of a geometry in a topologically-valid way

## Semantics
The semantics of operation results are:
* Results are always valid geometries
* Empty results are empty atomic geometries of appropriate dimension
* Duplicate vertices are removed
* Linear results are merged node-to-node (e.g. are of maximial length)
* Polygon edges which collapse completely due to rounding are not output

## Features
Functionality
* **Precision Model** - operations are performed using a defined precision model (finite or floating)
* **Robust Computation** - provides fully robust computation when an appropriate noder is used
* **Performance optimizations** - including:
  * Short-circuiting for disjoint input envelopes
  *Reduction of input segment count via clipping / limiting to overlap envelope
  * Optimizations can be disabled if required (e.g. for testing or performance evaluation)
* **Pluggable Noding** - allows using different noders to change characteristics of performance and accuracy
* **Precision Reduction** - in a topologically correct way. Implemented by unioning a single input with an empty geometry
* **Fast Coverage Union** - of valid polygonal and linear coverages

## Pluggable Noding
The noding phase of overlay uses a {@link Noder} subclass. This is determine automatically based on the precision model of the input. Or it can be provided explicity, which allows changing characteristics of performance and robustness. Examples of relevant noders include:
{@link SegmentExtractingNoder} - requires node-clean input, and provides very fast noding
{@link SnappingNoder} - a very robust full-precision noder
{@link ValidatingNoder} - a wrapper which can be used to verify the noding prior to topology building

## Codebase
* Defines a simpler topology model, with clear semantics.
* Uses a simpler topology graph data structure (based on winged edge pattern).
* Decouples noding and topology-build phases. This makes the code clearer, and makes it possible to allow supplying alternate implementations and semantics for each phase.
* All optimizations are implemented internally, so that clients do not have to add checks such as envelope overlap<./li>

## Algorithm
For non-point inputs the overlay algorithm is:
1. Check for empty input geometries, and return a result appropriate for the specified operation
2. Extract linework and points from input geometries, with topology location information
3. (If optimization enabled) Apply overlap envelope optimizations:
   1. For Intersection, check if the input envelopes are disjoint (using an envelope expansion adjustment to account for the precision grid).
   2. For Intersection and Difference, clip or limit the linework of the input geometries to the overlap envelope.
   3. If the optimized linework is empty, return an empty result of appropriate type.
4. Node the linework. For full robustness snap-rounding noding is used. Other kinds of noder can be used as well (for instance, the full-precision noding algorithm as the original overlay code).
5. Merge noded edges. Coincident edges from the two input geometries are merged, along with their topological labelling. Topology collapses are detected in this step, and are flagged in the labelling so they can be handled appropriately duing result polygon extraction
6. Build a fully-labelled topology graph. This includes:
   1. Create a graph structure on the noded, merged edges
   2. Propagate topology locations around nodes in the graph
   3. Label edges that have incomplete topology locations. These occur when edges from an input geometry are isolated (disjoint from the edges of the other geometry in the graph).
7. If result is empty return an empty geometry of appropriate type
8. Generate the result geometry from the labelled graph:
   1. Build result polygons
      1. Mark edges which should be included in the result areas
      2. Link maximal rings together
      3. Convert maximal rings to minimal (valid) rings
      4. Determine nesting of holes
      5. Construct result polygons
   2. Build result linework
      1. Mark edges to be included in the result lines
      2. Construct node-to-node linework
   3. Build result points
      1. For intersection only, output point occur where the input touch at single points
   4. Collect result elements into the result geometry

## Package Specification
* OpenGIS Simple Features Specification for SQL