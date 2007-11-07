using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// A sequence of <c>LineMergeDirectedEdge</c>s forming one of the lines that will
    /// be output by the line-merging process.
    /// </summary>
    public class EdgeString
    {
        private IGeometryFactory factory;
        private IList directedEdges = new ArrayList();
        private ICoordinate[] coordinates = null;

        /// <summary>
        /// Constructs an EdgeString with the given factory used to convert this EdgeString
        /// to a LineString.
        /// </summary>
        public EdgeString(IGeometryFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Adds a directed edge which is known to form part of this line.
        /// </summary>
        public void Add(LineMergeDirectedEdge directedEdge)
        {
            directedEdges.Add(directedEdge);
        }

        private ICoordinate[] Coordinates
        {
            get
            {
                if (coordinates == null)
                {
                    Int32 forwardDirectedEdges = 0;
                    Int32 reverseDirectedEdges = 0;
                    CoordinateList coordinateList = new CoordinateList();
                    IEnumerator i = directedEdges.GetEnumerator();
                    while (i.MoveNext())
                    {
                        LineMergeDirectedEdge directedEdge = (LineMergeDirectedEdge) i.Current;
                        if (directedEdge.EdgeDirection)
                        {
                            forwardDirectedEdges++;
                        }
                        else
                        {
                            reverseDirectedEdges++;
                        }
                        coordinateList.Add(((LineMergeEdge) directedEdge.Edge).Line.Coordinates, false,
                                           directedEdge.EdgeDirection);
                    }
                    coordinates = coordinateList.ToCoordinateArray();
                    if (reverseDirectedEdges > forwardDirectedEdges)
                    {
                        CoordinateArrays.Reverse(coordinates);
                    }
                }
                return coordinates;
            }
        }

        /// <summary>
        /// Converts this EdgeString into a LineString.
        /// </summary>
        public ILineString ToLineString()
        {
            return factory.CreateLineString(Coordinates);
        }
    }
}