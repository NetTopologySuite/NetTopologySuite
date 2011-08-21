using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;
using NetTopologySuite.Geometries.Utilities;
using Wintellect.PowerCollections;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// A utility class which creates Conforming Delaunay Trianglulations
    /// from collections of points and linear constraints, and extract the resulting 
    /// triangulation edges or triangles as geometries. 
    /// </summary>
    /// <author>Martin Davis</author>
    public class ConformingDelaunayTriangulationBuilder
    {
        private ICollection<ICoordinate> siteCoords;
        private IGeometry constraintLines;
        private double tolerance = 0.0;
        private QuadEdgeSubdivision subdiv = null;

        private IDictionary<ICoordinate, Vertex> vertexMap = new OrderedDictionary<ICoordinate, Vertex>();

        public ConformingDelaunayTriangulationBuilder()
        {
        }

        /// <summary>
        /// Sets the sites (point or vertices) which will be triangulated.
        /// All vertices of the given geometry will be used as sites.
        /// </summary>
        /// <remarks>The geometry from which the sites will be extracted.</remarks>
        public IGeometry Sites
        {
            set
            {
                siteCoords = DelaunayTriangulationBuilder.ExtractUniqueCoordinates(value);
            }
        }

        /// <summary>
        /// Sets the linear constraints to be conformed to.
        /// All linear components in the input will be used as constraints.
        /// </summary>
        /// <remarks>The lines to constraint to</remarks>
        ///
        public IGeometry Constraints
        {
            set
            {
                this.constraintLines = value;
            }
        }

        /// <summary>
        /// Sets the snapping tolerance which will be used
        /// to improved the robustness of the triangulation computation.
        /// A tolerance of 0.0 specifies that no snapping will take place.
        /// </summary>
        /// <remarks>The tolerance distance to use</remarks>
        public double Tolerance
        {
            set
            {
                this.tolerance = value;
            }
        }

        private void Create()
        {
            if (subdiv != null) return;

            var siteEnv = DelaunayTriangulationBuilder.Envelope(siteCoords);

            var sites = CreateConstraintVertices(siteCoords);

            IList<Segment> segments = new List<Segment>();
            if (constraintLines != null)
            {
                siteEnv.ExpandToInclude(constraintLines.EnvelopeInternal);
                CreateVertices(constraintLines);
                segments = CreateConstraintSegments(constraintLines);
            }

            ConformingDelaunayTriangulator cdt = new ConformingDelaunayTriangulator(sites, tolerance);

            cdt.SetConstraints(segments, new List<Vertex>(vertexMap.Values));

            cdt.FormInitialDelaunay();
            cdt.EnforceConstraints();
            subdiv = cdt.Subdivision;
        }

        private static IList<Vertex> CreateConstraintVertices(ICollection<ICoordinate> coords)
        {
            var verts = new List<Vertex>();
            foreach (var coord in coords)
            {
                verts.Add(new ConstraintVertex(coord));
            }
            return verts;
        }

        private void CreateVertices(IGeometry geom)
        {
            var coords = geom.Coordinates;
            for (int i = 0; i < coords.Length; i++)
            {
                Vertex v = new ConstraintVertex(coords[i]);
                vertexMap.Add(coords[i], v);
            }
        }

        private static IList<Segment> CreateConstraintSegments(IGeometry geom)
        {
            var lines = LinearComponentExtracter.GetLines(geom);
            var constraintSegs = new List<Segment>();
            foreach (var line in lines)
            {
                CreateConstraintSegments(line, constraintSegs);
            }
            return constraintSegs;
        }

        private static void CreateConstraintSegments(ILineString line, IList<Segment> constraintSegs)
        {
            var coords = line.Coordinates;
            for (int i = 1; i < coords.Length; i++)
            {
                constraintSegs.Add(new Segment(coords[i - 1], coords[i]));
            }
        }

        /// <summary>
        /// Gets the QuadEdgeSubdivision which models the computed triangulation.
        /// </summary>
        /// <returns>The subdivision containing the triangulation</returns>
        public QuadEdgeSubdivision getSubdivision()
        {
            Create();
            return subdiv;
        }

        /// <summary>
        /// Gets the edges of the computed triangulation as a <see cref="IMultiLineString"/>.
        /// </summary>
        /// <param name="geomFact">The geometry factory to use to create the output</param>
        /// <returns>the edges of the triangulation</returns>
        public IMultiLineString GetEdges(IGeometryFactory geomFact)
        {
            Create();
            return subdiv.GetEdges(geomFact);
        }

        /// <summary>
        /// Gets the faces of the computed triangulation as a <see cref="IGeometryCollection"/> 
        /// of <see cref="Polygon"/>.
        /// </summary>
        /// <param name="geomFact">the geometry factory to use to create the output</param>
        /// <returns>the faces of the triangulation</returns>
        public IGeometryCollection GetTriangles(IGeometryFactory geomFact)
        {
            Create();
            return subdiv.GetTriangles(geomFact);
        }

    }
}

