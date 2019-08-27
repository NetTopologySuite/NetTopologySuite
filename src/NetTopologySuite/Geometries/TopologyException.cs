using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Indicates an invalid or inconsistent topological situation encountered during processing
    /// </summary>
    public class TopologyException : ApplicationException
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

        private readonly Coordinate pt;

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
            this.pt = pt.Copy();
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate Coordinate => pt;
    }
}
