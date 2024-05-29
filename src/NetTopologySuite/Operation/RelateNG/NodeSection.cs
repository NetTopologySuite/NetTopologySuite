using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Operation.RelateNG
{
    /**
     * Represents a computed node along with the incident edges on either side of
     * it (if they exist).
     * This captures the information about a node in a geometry component
     * required to determine the component's contribution to the node topology.
     * A node in an area geometry always has edges on both sides of the node.
     * A node in a linear geometry may have one or other incident edge missing, if
     * the node occurs at an endpoint of the line.
     * The edges of an area node are assumed to be provided 
     * with CW-shell orientation (as per JTS norm).
     * This must be enforced by the caller.
     * 
     * @author Martin Davis
     *
     */
    internal class NodeSection : IComparable<NodeSection>
    {
        /// <summary>
        /// Compares sections by the angle the entering edge makes with the positive X axis.
        /// </summary>
        public class EdgeAngleComparator : IComparer<NodeSection>
        {
            public static EdgeAngleComparator Instance = new EdgeAngleComparator();
            private EdgeAngleComparator() { }
            public int Compare(NodeSection ns1, NodeSection ns2)
            {
                return PolygonNodeTopology.CompareAngle(ns1.nodePt, ns1.getVertex(0), ns2.getVertex(0));
            }
        }

        public static bool IsAreaArea(NodeSection a, NodeSection b)
        {
            return a.dimension == Dimension.A && b.dimension == Dimension.A;
        }

        public static bool AreProper(NodeSection a, NodeSection b)
        {
            return a.isProper && b.isProper;
        }


        //private bool isA;
        //private int dim;
        //private int id;
        //private int ringId;
        //private bool isNodeAtVertex;
        //private Coordinate nodePt;
        private readonly Coordinate _v0;
        private readonly Coordinate _v1;
        private readonly Geometry _poly;

        public NodeSection(bool isA,
            Dimension dimension, int id, int ringId,
            Geometry poly, bool isNodeAtVertex, Coordinate v0, Coordinate nodePt, Coordinate v1)
        {
            this.isA = isA;
            this.dimension = dimension;
            this.id = id;
            this.ringId = ringId;
            Polygonal = poly;
            this.isNodeAtVertex = isNodeAtVertex;
            this.nodePt = nodePt;
            this._v0 = v0;
            this._v1 = v1;
        }

        public Coordinate getVertex(int i)
        {
            return i == 0 ? _v0 : _v1;
        }

        public Coordinate nodePt { get; }

        public Dimension dimension { get; private set; }

        public int id { get; }

        public int ringId { get; }

        public Geometry Polygonal { get; }

        public bool isShell => ringId == 0;


        public bool isArea => dimension == Dimension.A;


        public bool isA { get; }

        public bool isSameGeometry(NodeSection ns)
        {
            return isA == ns.isA;
        }

        public bool isSamePolygon(NodeSection ns)
        {
            return isA == ns.isA && id == ns.id;
        }

        public bool isNodeAtVertex { get; }

        public bool isProper => !isNodeAtVertex;


        public override string ToString()
        {
            string geomName = RelateGeometry.Name(isA);
            string atVertexInd = isNodeAtVertex ? "-V-" : "---";
            string polyId = id >= 0 ? "[" + id + ":" + ringId + "]" : "";
            return string.Format("%s%d%s: %s %s %s",
                geomName, dimension, polyId, EdgeRep(_v0, nodePt), atVertexInd, EdgeRep(nodePt, _v1));
        }

        private static string EdgeRep(Coordinate p0, Coordinate p1)
        {
            if (p0 == null || p1 == null)
                return "null";
            return WKTWriter.ToLineString(p0, p1);
        }

        /// <summary>
        /// Compare node sections by parent geometry, dimension, element id and ring id,
        /// and edge vertices.
        /// Sections are assumed to be at the same node point.</summary>
        public int CompareTo(NodeSection o)
        {
            // Assert: nodePt.equals2D(o.nodePt())

            // sort A before B
            if (isA != o.isA)
            {
                if (isA) return -1;
                return 1;
            }
            //-- sort on dimensions
            int compDim = dimension.CompareTo(o.dimension);
            if (compDim != 0) return compDim;

            //-- sort on id and ring id
            int compId = id.CompareTo(o.id);
            if (compId != 0) return compId;

            int compRingId = ringId.CompareTo(o.ringId);
            if (compRingId != 0) return compRingId;

            //-- sort on edge coordinates
            int compV0 = CompareWithNull(_v0, o._v0);
            if (compV0 != 0) return compV0;

            return CompareWithNull(_v1, o._v1);
        }

        private static int CompareWithNull(Coordinate v0, Coordinate v1)
        {
            if (v0 == null)
            {
                if (v1 == null)
                    return 0;
                //-- null is lower than non-null
                return -1;
            }
            // v0 is non-null
            if (v1 == null)
                return 1;
            return v0.CompareTo(v1);
        }
    }
}
