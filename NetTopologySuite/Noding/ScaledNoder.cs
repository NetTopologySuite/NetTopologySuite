using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Wraps a <see cref="INoder{TCoordinate}" /> and transforms its input into the integer domain.
    /// This is intended for use with Snap-Rounding noders,
    /// which typically are only intended to work in the integer domain.
    /// Offsets can be provided to increase the number of digits of available precision.
    /// </summary>
    public class ScaledNoder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly INoder<TCoordinate> _noder = null;
        private readonly Double _scaleFactor = 0;
        private readonly Double _offsetX = 0;
        private readonly Double _offsetY = 0;
        private readonly IAffineTransformMatrix<DoubleComponent> _transform;
        private readonly IAffineTransformMatrix<DoubleComponent> _inverse;
        private readonly Boolean _isScaled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledNoder{TCoordinate}"/> class.
        /// </summary>
        public ScaledNoder(INoder<TCoordinate> noder, Double scaleFactor)
            : this(noder, scaleFactor, 0, 0) {}

        public ScaledNoder(INoder<TCoordinate> noder, Double scaleFactor, Double offsetX, Double offsetY)
        {
            _noder = noder;
            ICoordinateFactory<TCoordinate> coordinateFactory = null;
            Debug.Assert(coordinateFactory != null);
            TCoordinate scaleVector = coordinateFactory.Create(scaleFactor, scaleFactor);
            TCoordinate offsetVector = coordinateFactory.Create(offsetX, offsetY);

            _transform = coordinateFactory.CreateTransform(scaleVector, 0, offsetVector);
            _inverse = _transform.Inverse;

            // no need to scale if input precision is already integral
            _isScaled = ! IsIntegerPrecision;
        }

        public Boolean IsIntegerPrecision
        {
            get { return _scaleFactor == 1.0; }
        }

        public IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings()
        {
            IEnumerable<SegmentString<TCoordinate>> splitSS = _noder.GetNodedSubstrings();

            if (_isScaled)
            {
                rescale(splitSS);
            }

            return splitSS;
        }

        public void ComputeNodes(IEnumerable<SegmentString<TCoordinate>> inputSegStrings)
        {
            IEnumerable<SegmentString<TCoordinate>> intSegStrings = inputSegStrings;

            if (_isScaled)
            {
                intSegStrings = Scale(inputSegStrings);
            }

            _noder.ComputeNodes(intSegStrings);
        }

        private IEnumerable<SegmentString<TCoordinate>> Scale(IEnumerable<SegmentString<TCoordinate>> segStrings)
        {
            return CollectionUtil.Transform(segStrings, delegate(SegmentString<TCoordinate> segmentString)
                                                        {
                                                            return new SegmentString<TCoordinate>(
                                                                scale(segmentString.Coordinates), segmentString.Data);
                                                        });
        }

        private IEnumerable<TCoordinate> scale(IEnumerable<TCoordinate> pts)
        {
            // TODO: figure out how to get rid of boxing...
            IEnumerable<IVector<DoubleComponent>> vectors =
                Enumerable.Upcast<IVector<DoubleComponent>, TCoordinate>(pts);

            IEnumerable<IVector<DoubleComponent>> transformed = _transform.TransformVectors(vectors);

            foreach (IVector<DoubleComponent> vector in transformed)
            {
                yield return Math.Round(new TCoordinate(vector));
            }
        }

        private static IEnumerable<SegmentString<TCoordinate>> rescale(IEnumerable<SegmentString<TCoordinate>> segStrings)
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