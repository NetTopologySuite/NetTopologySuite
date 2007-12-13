using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// A sequence of <c>LineMergeDirectedEdge</c>s forming one of the lines that will
    /// be output by the line-merging process.
    /// </summary>
    public class EdgeString<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _factory;
        private readonly List<LineMergeDirectedEdge<TCoordinate>> _directedEdges 
            = new List<LineMergeDirectedEdge<TCoordinate>>();

        private ICoordinateSequence<TCoordinate> _coordinates;

        /// <summary>
        /// Constructs an EdgeString with the given factory used to convert this EdgeString
        /// to a LineString.
        /// </summary>
        public EdgeString(IGeometryFactory<TCoordinate> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Adds a directed edge which is known to form part of this line.
        /// </summary>
        public void Add(LineMergeDirectedEdge<TCoordinate> directedEdge)
        {
            _directedEdges.Add(directedEdge);
        }

        /// <summary>
        /// Converts this EdgeString into a LineString.
        /// </summary>
        public ILineString<TCoordinate> ToLineString()
        {
            return _factory.CreateLineString(getCoordinates());
        }

        private IEnumerable<TCoordinate> getCoordinates()
        {
            if (_coordinates == null)
            {
                Int32 forwardDirectedEdges = 0;
                Int32 reverseDirectedEdges = 0;

                ICoordinateSequence<TCoordinate> coordinateList =
                    CoordinateSequences.CreateEmpty<TCoordinate>();

                foreach (LineMergeDirectedEdge<TCoordinate> directedEdge in _directedEdges)
                {
                    if (directedEdge.EdgeDirection)
                    {
                        forwardDirectedEdges++;
                    }
                    else
                    {
                        reverseDirectedEdges++;
                    }

                    LineMergeEdge<TCoordinate> edge = directedEdge.Edge as LineMergeEdge<TCoordinate>;
                    Debug.Assert(edge != null);
                    coordinateList.Add(edge.Line.Coordinates, false, directedEdge.EdgeDirection);
                }

                if (reverseDirectedEdges > forwardDirectedEdges)
                {
                    _coordinates = coordinateList.Reversed;
                }
                else
                {
                    _coordinates = coordinateList;
                }
            }

            return _coordinates;
        }
    }
}