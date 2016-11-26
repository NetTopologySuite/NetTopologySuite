﻿using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// A dynamic list of the vertices in a constructed offset curve.
    /// Automatically removes adjacent vertices
    /// which are closer than a given tolerance.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class OffsetSegmentString
    {
        private readonly List<Coordinate> _ptList;
        private IPrecisionModel _precisionModel;

        /**
         * The distance below which two adjacent points on the curve
         * are considered to be coincident.
         * This is chosen to be a small fraction of the offset distance.
         */
        private double _minimimVertexDistance;

        public OffsetSegmentString()
        {
            _ptList = new List<Coordinate>();
        }

        public IPrecisionModel PrecisionModel
        {
            get { return _precisionModel; }
            set { _precisionModel = value; }
        }

        public double MinimumVertexDistance
        {
            get { return _minimimVertexDistance; }
            set { _minimimVertexDistance = value; }
        }

        public void AddPt(Coordinate pt)
        {
            var bufPt = new Coordinate(pt);
            _precisionModel.MakePrecise(bufPt);
            // don't add duplicate (or near-duplicate) points
            if (IsRedundant(bufPt))
                return;
            _ptList.Add(bufPt);
            //Console.WriteLine(bufPt);
        }

        public void AddPts(Coordinate[] pt, bool isForward)
        {
            if (isForward)
            {
                for (int i = 0; i < pt.Length; i++)
                {
                    AddPt(pt[i]);
                }
            }
            else
            {
                for (int i = pt.Length - 1; i >= 0; i--)
                {
                    AddPt(pt[i]);
                }
            }
        }

        /// <summary>
        /// Tests whether the given point is redundant
        /// relative to the previous
        /// point in the list (up to tolerance).
        /// </summary>
        /// <param name="pt"></param>
        /// <returns>true if the point is redundant</returns>
        private bool IsRedundant(Coordinate pt)
        {
            if (_ptList.Count < 1)
                return false;
            var lastPt = _ptList[_ptList.Count - 1];
            double ptDist = pt.Distance(lastPt);
            if (ptDist < _minimimVertexDistance)
                return true;
            return false;
        }

        public void CloseRing()
        {
            if (_ptList.Count < 1)
                return;

            var startPt = new Coordinate(_ptList[0]);
            var lastPt = _ptList[_ptList.Count - 1];
            /*
            Coordinate last2Pt = null;
            if (ptList.Count >= 2)
                last2Pt = ptList[ptList.Count - 2];
            */
            if (startPt.Equals(lastPt)) return;
            _ptList.Add(startPt);
        }

        public void Reverse()
        {
        }

        public Coordinate[] GetCoordinates()
        {
            /*
            // check that points are a ring - add the startpoint again if they are not
            if (ptList.size() > 1)
            {
                Coordinate start  = (Coordinate) ptList.get(0);
                Coordinate end    = (Coordinate) ptList.get(ptList.size() - 1);
                if (! start.equals(end) ) addPt(start);
            }
            */
            var coord = _ptList.ToArray();
            return coord;
        }

        public override String ToString()
        {
            var fact = new GeometryFactory();
            var line = fact.CreateLineString(GetCoordinates());
            return line.ToString();
        }
    }
}