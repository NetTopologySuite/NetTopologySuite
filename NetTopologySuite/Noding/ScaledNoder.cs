using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <para>
    /// Clients should be aware that rescaling can involve loss of precision,
    /// which can cause zero-length line segments to be created.
    /// These in turn can cause problems when used to build a planar graph.
    /// This situation should be checked for and collapsed segments removed if necessary.
    /// </para>
    /// </summary>
    public class ScaledNoder : INoder
    {
        private readonly INoder _noder;
        private readonly double _scaleFactor;
        private readonly double _offsetX;
        private readonly double _offsetY;
        private readonly bool _isScaled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledNoder"/> class.
        /// </summary>
        /// <param name="noder">The noder to use</param>
        /// <param name="scaleFactor">The scale factor to use</param>
        public ScaledNoder(INoder noder, double scaleFactor)
        {
            _noder = noder;
            _scaleFactor = scaleFactor;
            // no need to scale if input precision is already integral
            _isScaled = !IsIntegerPrecision;
        }

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
        public bool IsIntegerPrecision => _scaleFactor == 1.0;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            var splitSS = _noder.GetNodedSubstrings();
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
            var intSegStrings = inputSegStrings;
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
            var nodedSegmentStrings = new List<ISegmentString>(segStrings.Count);
            for (int i = 0; i < segStrings.Count; i++)
            {
                var ss = segStrings[i];
                nodedSegmentStrings.Add(new NodedSegmentString(Scale(ss.Coordinates), ss.Context));
            }

            return nodedSegmentStrings;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private Coordinate[] Scale(Coordinate[] pts)
        {
            var roundPts = new Coordinate[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                roundPts[i] = new Coordinate(Math.Round((pts[i].X - _offsetX) * _scaleFactor),
                                             Math.Round((pts[i].Y - _offsetY) * _scaleFactor),
                                             pts[i].Z);
            var roundPtsNoDup = CoordinateArrays.RemoveRepeatedPoints(roundPts);
            return roundPtsNoDup;
        }

        private void Rescale(IList<ISegmentString> segStrings)
        {
            for (int i = 0; i < segStrings.Count; i++)
                Rescale(segStrings[i].Coordinates);
        }

        private void Rescale(Coordinate[] pts)
        {
            Coordinate p0 = null;
            Coordinate p1 = null;

            if (pts.Length == 2)
            {
                p0 = new Coordinate(pts[0]);
                p1 = new Coordinate(pts[1]);
            }

            for (int i = 0; i < pts.Length; i++)
            {
                pts[i].X = pts[i].X / _scaleFactor + _offsetX;
                pts[i].Y = pts[i].Y / _scaleFactor + _offsetY;
            }

#if DEBUG
            if (pts.Length == 2 && pts[0].Equals2D(pts[1]))
            {
                Debug.WriteLine(pts[0]);
                Debug.WriteLine(pts[1]);
            }
#endif
        }
    }
}
