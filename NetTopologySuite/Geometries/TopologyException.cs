using System;
using GeoAPI.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary> 
    /// Indicates an invalid or inconsistent topological 
    /// situation encountered during processing.
    /// </summary>
    public class TopologyException : NtsException
    {
        private readonly ICoordinate _coordinate = null;

        public TopologyException(string msg) : base(msg) {}

        public TopologyException(string msg, ICoordinate pt)
            : base(formatMessageAndCoordinate(msg, pt))
        {
            _coordinate = pt.Clone() as ICoordinate;
        }

        public ICoordinate Coordinate
        {
            get { return _coordinate; }
        }

        private static string formatMessageAndCoordinate(string msg, ICoordinate pt)
        {
            if (pt != null && !pt.IsEmpty)
            {
                return String.Format("{0} [{1}]", msg, pt);
            }

            return msg;
        }
    }
}