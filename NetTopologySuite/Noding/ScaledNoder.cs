using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using NPack;
using NPack.Interfaces;
#if DOTNET35
using System.Linq;
#endif

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Wraps a <see cref="INoder{TCoordinate}" /> and transforms its input into 
    /// the integer domain.
    /// This is intended for use with Snap-Rounding noders,
    /// which typically are only intended to work in the integer domain.
    /// Offsets can be provided to increase the number of digits of available precision.
    /// </summary>
    public class ScaledNoder<TCoordinate, TMatrix> : INoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
        where TMatrix : ITransformMatrix<DoubleComponent, TCoordinate, TMatrix>
    {
        private readonly INoder<TCoordinate> _noder;
        //private readonly Double _scaleFactor = 0;
        //private readonly Double _offsetX = 0;
        //private readonly Double _offsetY = 0;
        private readonly TMatrix _transform;
        private readonly TMatrix _inverse;
        private readonly Boolean _isScaled;
        private readonly ICoordinateSequenceFactory<TCoordinate> _sequenceFactory;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ScaledNoder{TCoordinate, TMatrix}"/> class.
        /// </summary>
        public ScaledNoder(INoder<TCoordinate> noder, 
                           Double scaleFactor, 
                           ICoordinateSequenceFactory<TCoordinate> factory)
            : this(noder, scaleFactor, 0, 0, factory) {}

        public ScaledNoder(INoder<TCoordinate> noder, 
                           Double scaleFactor, 
                           Double offsetX, 
                           Double offsetY, 
                           ICoordinateSequenceFactory<TCoordinate> factory)
        {
            _noder = noder;
            _sequenceFactory = factory;
            ICoordinateFactory<TCoordinate> coordinateFactory = factory.CoordinateFactory;
            Debug.Assert(coordinateFactory != null);
            TCoordinate scaleVector = coordinateFactory.Create(scaleFactor, scaleFactor);
            TCoordinate offsetVector = coordinateFactory.Create(offsetX, offsetY);

            throw new NotImplementedException();
            
            // Need to inject an IMatrixFactory here...
            //_transform = coordinateFactory.CreateTransform<TMatrix>(scaleVector, 
            //                                                        0, 
            //                                                        offsetVector);
            _inverse = _transform.Inverse;

            // no need to scale if input precision is already integral
            _isScaled = !IsIntegerPrecision;
        }

        public Boolean IsIntegerPrecision
        {
            get { return _transform[0, 0].Equals(1.0); }
        }

        public IEnumerable<NodedSegmentString<TCoordinate>> Node(
            IEnumerable<NodedSegmentString<TCoordinate>> inputSegStrings)
        {
            IEnumerable<NodedSegmentString<TCoordinate>> intSegStrings 
                = inputSegStrings;

            if (_isScaled)
            {
                intSegStrings = scale(inputSegStrings);
            }

            IEnumerable<NodedSegmentString<TCoordinate>> splitSS 
                = _noder.Node(intSegStrings);

            if (_isScaled)
            {
                rescale(splitSS);
            }

            return splitSS;
        }

        private IEnumerable<NodedSegmentString<TCoordinate>> scale(
            IEnumerable<NodedSegmentString<TCoordinate>> segStrings)
        {
            Func<NodedSegmentString<TCoordinate>, NodedSegmentString<TCoordinate>>
                componentTransform = delegate(NodedSegmentString<TCoordinate> segmentString)
                                     {
                                         return new NodedSegmentString<TCoordinate>(
                                             scale(segmentString.Coordinates), 
                                             segmentString.Context);
                                     };
            return Enumerable.Transform<NodedSegmentString<TCoordinate>>(segStrings, componentTransform);
        }

        private ICoordinateSequence<TCoordinate> scale(ICoordinateSequence<TCoordinate> pts)
        {
            IEnumerable<TCoordinate> transformed = _transform.TransformVectors(pts);

            return _sequenceFactory.Create(Math.Round, transformed);
        }

        private static IEnumerable<NodedSegmentString<TCoordinate>> rescale(
            IEnumerable<NodedSegmentString<TCoordinate>> segStrings)
        {
            yield break;

            // TODO rescale the cooridnates...

            //CollectionUtil.Apply(segStrings, delegate(object obj)
            //                                 {
            //                                     SegmentString ss = (SegmentString) obj;
            //                                     rescale(ss.Coordinates);
            //                                     return null;
            //                                 });
        }

        private IEnumerable<TCoordinate> rescale(IEnumerable<TCoordinate> pts)
        {
            yield break;

            // TODO compute inverse of cooridnates
            // _inverse.Transform(pts)...
            //
            //for (Int32 i = 0; i < pts.Length; i++)
            //{
            //    pts[i].X = pts[i].X/_scaleFactor + _offsetX;
            //    pts[i].Y = pts[i].Y/_scaleFactor + _offsetY;
            //}
        }
    }
}