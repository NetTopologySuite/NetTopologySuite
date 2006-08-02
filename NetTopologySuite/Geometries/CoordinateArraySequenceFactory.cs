using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Creates CoordinateSequences represented as an array of Coordinates.
    /// </summary>
    [Serializable]
    public sealed class CoordinateArraySequenceFactory : ICoordinateSequenceFactory
    {
        private static readonly CoordinateArraySequenceFactory instance = new CoordinateArraySequenceFactory();

        /// <summary>
        /// 
        /// </summary>
        private CoordinateArraySequenceFactory() { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Object ReadResolve() 
        {
            return CoordinateArraySequenceFactory.Instance;
        }

        /// <summary>
        /// Returns the singleton instance of CoordinateArraySequenceFactory.
        /// </summary>
        /// <returns></returns>
        public static CoordinateArraySequenceFactory Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        /// <returns></returns>
        public ICoordinateSequence Create(Coordinate[] coordinates) 
        {
            return new CoordinateArraySequence(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(ICoordinateSequence coordSeq) 
        {
            return new CoordinateArraySequence(coordSeq);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public ICoordinateSequence Create(int size, int dimension) 
        {
            return new CoordinateArraySequence(size);
        }
    }
}
