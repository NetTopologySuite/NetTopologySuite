using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary> 
    /// Indicates an invalid or inconsistent topological situation encountered during processing
    /// </summary>
    public class TopologyException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        private static string MsgWithCoord(string msg, Coordinate pt)
        {
            if (pt != null)
            return msg + " [ " + pt + " ]";
            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public TopologyException(string msg) : base(msg) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="pt"></param>
        public TopologyException(string msg, Coordinate pt) 
            : base (MsgWithCoord(msg, pt))
        {            
            Coordinate = new Coordinate(pt);
        }

        /// <summary>
        /// 
        /// </summary>
        public Coordinate Coordinate { get; }
    }
}
