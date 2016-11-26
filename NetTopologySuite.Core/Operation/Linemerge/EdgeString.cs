using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

#if PCL
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    ///     A sequence of <c>LineMergeDirectedEdge</c>s forming one of the lines that will
    ///     be output by the line-merging process.
    /// </summary>
    public class EdgeString
    {
        private readonly IList directedEdges = new ArrayList();
        private readonly IGeometryFactory factory;
        private Coordinate[] coordinates;

        /// <summary>
        ///     Constructs an EdgeString with the given factory used to convert this EdgeString
        ///     to a LineString.
        /// </summary>
        /// <param name="factory"></param>
        public EdgeString(IGeometryFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// </summary>
        private Coordinate[] Coordinates
        {
            get
            {
                if (coordinates == null)
                {
                    var forwardDirectedEdges = 0;
                    var reverseDirectedEdges = 0;
                    var coordinateList = new CoordinateList();
                    var i = directedEdges.GetEnumerator();
                    while (i.MoveNext())
                    {
                        var directedEdge = (LineMergeDirectedEdge) i.Current;
                        if (directedEdge.EdgeDirection)
                            forwardDirectedEdges++;
                        else reverseDirectedEdges++;
                        coordinateList.Add(((LineMergeEdge) directedEdge.Edge).Line.Coordinates, false,
                            directedEdge.EdgeDirection);
                    }
                    coordinates = coordinateList.ToCoordinateArray();
                    if (reverseDirectedEdges > forwardDirectedEdges)
                        CoordinateArrays.Reverse(coordinates);
                }
                return coordinates;
            }
        }

        /// <summary>
        ///     Adds a directed edge which is known to form part of this line.
        /// </summary>
        /// <param name="directedEdge"></param>
        public void Add(LineMergeDirectedEdge directedEdge)
        {
            directedEdges.Add(directedEdge);
        }

        /// <summary>
        ///     Converts this EdgeString into a LineString.
        /// </summary>
        public ILineString ToLineString()
        {
            return factory.CreateLineString(Coordinates);
        }
    }
}