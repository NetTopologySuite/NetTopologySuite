using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Wraps a <see cref="INoder" /> and transforms its input into the integer domain.
    /// This is intended for use with Snap-Rounding noders,
    /// which typically are only intended to work in the integer domain.
    /// Offsets can be provided to increase the number of digits of available precision.
    /// </summary>
    public class ScaledNoder<TCoordinate> : INoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private INoder<TCoordinate> _noder = null;
        private Double _scaleFactor = 0;
        private Double _offsetX = 0;
        private Double _offsetY = 0;
        private Boolean _isScaled = false;
        private ICoordinateSequenceFactory<TCoordinate> _sequenceFactory;
        private ICoordinateFactory<TCoordinate> _coordFactory;
        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledNoder{TCoordinate}"/> class.
        /// </summary>
        /// <param name="noder"></param>
        /// <param name="scaleFactor"></param>
        public ScaledNoder(ICoordinateSequenceFactory<TCoordinate> sequenceFactory, INoder<TCoordinate> noder, Double scaleFactor)
            : this(sequenceFactory, noder, scaleFactor, 0, 0) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noder"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public ScaledNoder(ICoordinateSequenceFactory<TCoordinate> sequenceFactory, INoder<TCoordinate> noder, Double scaleFactor, Double offsetX, Double offsetY)
        {
            _sequenceFactory = sequenceFactory;
            _coordFactory = sequenceFactory.CoordinateFactory;
            _noder = noder;
            _scaleFactor = scaleFactor;
            // no need to scale if input precision is already integral
            _isScaled = !IsIntegerPrecision;
            _offsetX = offsetX;
            _offsetY = offsetY;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIntegerPrecision
        {
            get
            {
                return _scaleFactor == 1.0;
            }
            set
            {
                _scaleFactor = 1.0d;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <returns></returns>
        private IEnumerable<ISegmentString<TCoordinate>> Scale(
            IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            Func<ISegmentString<TCoordinate>, ISegmentString<TCoordinate>>
                componentTransform = delegate(ISegmentString<TCoordinate> segmentString)
                {
                    return new NodedSegmentString<TCoordinate>(
                        Scale(segmentString.Coordinates),
                        segmentString.Context);
                };
            return Processor.Transform<ISegmentString<TCoordinate>>(segStrings, componentTransform);
        }

        private ICoordinateSequence<TCoordinate> Scale(ICoordinateSequence<TCoordinate> pts)
        {
            return _sequenceFactory.Create(Math.Round, Scale((IEnumerable<TCoordinate>)pts));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private IEnumerable<TCoordinate> Scale(IEnumerable<TCoordinate> coords)
        {
            TCoordinate last = default(TCoordinate);
            foreach (TCoordinate coord in coords)
            {
                TCoordinate current = _coordFactory.Create(
                    Math.Round((coord[Ordinates.X] - _offsetX)*_scaleFactor),
                    Math.Round((coord[Ordinates.Y] - _offsetY)*_scaleFactor));
                if (!current.Equals(last))
                    yield return current;
                last = current;
            }
        }
        private ICoordinateSequence<TCoordinate> Rescale(ICoordinateSequence<TCoordinate> pts)
        {
            return _sequenceFactory.Create(Math.Round, Rescale((IEnumerable<TCoordinate>)pts));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private IEnumerable<TCoordinate> Rescale(IEnumerable<TCoordinate> coords)
        {
            foreach (TCoordinate coord in coords)
            {
                yield return _coordFactory.Create(
                    coord[Ordinates.X] / _scaleFactor + _offsetX,
                    coord[Ordinates.Y] / _scaleFactor + _offsetY);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <returns></returns>
        private IEnumerable<ISegmentString<TCoordinate>> Rescale(
            IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            Func<ISegmentString<TCoordinate>, ISegmentString<TCoordinate>>
                componentTransform = delegate(ISegmentString<TCoordinate> segmentString)
                {
                    return new NodedSegmentString<TCoordinate>(
                        Rescale(segmentString.Coordinates),
                        segmentString.Context);
                };
            return Processor.Transform<ISegmentString<TCoordinate>>(segStrings, componentTransform);
        }

        #region INoder<TCoordinate> Member

        public IEnumerable<ISegmentString<TCoordinate>> Node(IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            IEnumerable<ISegmentString<TCoordinate>> intSegStrings = IsIntegerPrecision
                                                                             ?
                                                                                 segStrings
                                                                             :
                                                                                 Scale(segStrings);

            IEnumerable<ISegmentString<TCoordinate>> splitSS
                = _noder.Node(intSegStrings);

            if (_isScaled)
            {
                Rescale(splitSS);
            }

            return splitSS;
        }

        public void ComputeNodes(IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            IEnumerable<ISegmentString<TCoordinate>> intSegStrings = IsIntegerPrecision
                                                                             ?
                                                                                 segStrings
                                                                             :
                                                                                 Scale(segStrings);

            _noder.ComputeNodes(intSegStrings);
        }

        #endregion
    }
}