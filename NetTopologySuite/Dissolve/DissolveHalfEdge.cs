using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Dissolve
{
    /// <summary>
    /// A HalfEdge which carries information
    /// required to support <see cref="LineDissolver"/>.
    /// </summary>
    public class DissolveHalfEdge : MarkHalfEdge
    {
        private bool _isStart;

        public DissolveHalfEdge(Coordinate orig)
            : base(orig) { }

        /// <summary>
        /// Tests whether this edge is the starting segment
        /// in a LineString being dissolved.
        /// </summary>
        /// <returns><c>true</c> if this edge is a start segment</returns>
        public bool IsStart => _isStart;

        /// <summary>
        /// Sets this edge to be the start segment of an input LineString.
        /// </summary>
        public void SetStart()
        {
            _isStart = true;
        }
    }
}