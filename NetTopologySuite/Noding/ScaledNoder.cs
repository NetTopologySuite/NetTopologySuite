using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Noding
{

    /// <summary>
    /// Wraps a <see cref="INoder" /> and transforms its input into the integer domain.
    /// This is intended for use with Snap-Rounding noders,
    /// which typically are only intended to work in the integer domain.
    /// Offsets can be provided to increase the number of digits of available precision.
    /// </summary>
    public class ScaledNoder : INoder
    {
        private readonly INoder _noder;
        private readonly double _scaleFactor;
        private double _offsetX;
        private double _offsetY;
        private readonly bool _isScaled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledNoder"/> class.
        /// </summary>
        /// <param name="noder"></param>
        /// <param name="scaleFactor"></param>
        public ScaledNoder(INoder noder, double scaleFactor) 
            : this(noder, scaleFactor, 0, 0) { }      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noder"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        [Obsolete("Do not use offsetX and offsetY")]
        public ScaledNoder(INoder noder, double scaleFactor, double offsetX, double offsetY) 
        {
            _noder = noder;
            _scaleFactor = scaleFactor;
            _offsetX = offsetX;
            _offsetY = offsetY;
            // no need to scale if input precision is already integral
            _isScaled = ! IsIntegerPrecision;
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            IList<ISegmentString> splitSS = _noder.GetNodedSubstrings();
            if (_isScaled) 
                Rescale(splitSS);
            return splitSS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSegStrings"></param>
        public void ComputeNodes(IList<ISegmentString> inputSegStrings)
        {
            IList<ISegmentString> intSegStrings = inputSegStrings;
            if(_isScaled)
                intSegStrings = Scale(inputSegStrings);
            _noder.ComputeNodes(intSegStrings);
        }    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        /// <returns></returns>
        private IList<ISegmentString> Scale(IList<ISegmentString> segStrings)
        {
            return CollectionUtil.Transform<ISegmentString, ISegmentString>(segStrings, 
                ss => ((ISegmentString)new NodedSegmentString(Scale(ss.Coordinates), ss.Context)));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private Coordinate[] Scale(Coordinate[] pts)
        {
            Coordinate[] roundPts = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                roundPts[i] = new Coordinate(Math.Round((pts[i].X - _offsetX) * _scaleFactor),
                                             Math.Round((pts[i].Y - _offsetY) * _scaleFactor));
            Coordinate[] roundPtsNoDup = CoordinateArrays.RemoveRepeatedPoints(roundPts);
            return roundPtsNoDup;
        }      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segStrings"></param>
        private void Rescale(IList<ISegmentString> segStrings)
        {
            CollectionUtil.Apply(segStrings,
                ss => { Rescale(ss.Coordinates); return null; } );                                           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        private void Rescale(Coordinate[] pts)
        {
            for (int i = 0; i < pts.Length; i++) 
            {
                pts[i].X = pts[i].X / _scaleFactor + _offsetX;
                pts[i].Y = pts[i].Y / _scaleFactor + _offsetY;
            }
        }
    }
}
