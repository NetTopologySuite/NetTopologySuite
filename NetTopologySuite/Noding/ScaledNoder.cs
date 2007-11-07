using System;
using System.Collections;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Wraps a <see cref="INoder" /> and transforms its input into the integer domain.
    /// This is intended for use with Snap-Rounding noders,
    /// which typically are only intended to work in the integer domain.
    /// Offsets can be provided to increase the number of digits of available precision.
    /// </summary>
    public class ScaledNoder : INoder
    {
        private INoder noder = null;
        private Double scaleFactor = 0;
        private Double offsetX = 0;
        private Double offsetY = 0;
        private Boolean isScaled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledNoder"/> class.
        /// </summary>
        public ScaledNoder(INoder noder, Double scaleFactor)
            : this(noder, scaleFactor, 0, 0) {}

        public ScaledNoder(INoder noder, Double scaleFactor, Double offsetX, Double offsetY)
        {
            this.noder = noder;
            this.scaleFactor = scaleFactor;
            // no need to scale if input precision is already integral
            isScaled = ! isIntegerPrecision;
        }

        public Boolean isIntegerPrecision
        {
            get { return scaleFactor == 1.0; }
        }

        public IList GetNodedSubstrings()
        {
            IList splitSS = noder.GetNodedSubstrings();
            if (isScaled)
            {
                Rescale(splitSS);
            }
            return splitSS;
        }

        public void ComputeNodes(IList inputSegStrings)
        {
            IList intSegStrings = inputSegStrings;
            if (isScaled)
            {
                intSegStrings = Scale(inputSegStrings);
            }
            noder.ComputeNodes(intSegStrings);
        }

        private IList Scale(IList segStrings)
        {
            return CollectionUtil.Transform(segStrings, delegate(object obj)
                                                        {
                                                            SegmentString ss = (SegmentString) obj;
                                                            return new SegmentString(Scale(ss.Coordinates), ss.Data);
                                                        });
        }

        private ICoordinate[] Scale(ICoordinate[] pts)
        {
            ICoordinate[] roundPts = new ICoordinate[pts.Length];
            for (Int32 i = 0; i < pts.Length; i++)
            {
                roundPts[i] = new Coordinate(Math.Round((pts[i].X - offsetX)*scaleFactor),
                                             Math.Round((pts[i].Y - offsetY)*scaleFactor));
            }
            ICoordinate[] roundPtsNoDup = CoordinateArrays.RemoveRepeatedPoints(roundPts);
            return roundPtsNoDup;
        }

        private void Rescale(IList segStrings)
        {
            CollectionUtil.Apply(segStrings, delegate(object obj)
                                             {
                                                 SegmentString ss = (SegmentString) obj;
                                                 Rescale(ss.Coordinates);
                                                 return null;
                                             });
        }

        private void Rescale(ICoordinate[] pts)
        {
            for (Int32 i = 0; i < pts.Length; i++)
            {
                pts[i].X = pts[i].X/scaleFactor + offsetX;
                pts[i].Y = pts[i].Y/scaleFactor + offsetY;
            }
        }
    }
}