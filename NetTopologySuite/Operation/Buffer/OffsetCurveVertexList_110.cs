using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Buffer
{
    ///<summary>
    /// A list of the vertices in a constructed offset curve. Automatically removes close adjacent vertices.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class OffsetCurveVertexList_110<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geomFact;
        private List<TCoordinate> _ptList;
        private IPrecisionModel<TCoordinate> _precisionModel = null;

        ///<summary>
        /// The distance below which two adjacent points on the curve 
        /// are considered to be coincident.
        /// This is chosen to be a small fraction of the offset distance.
        ///</summary>
        private Double _minimimVertexDistance = 0.0;

        public OffsetCurveVertexList_110(IGeometryFactory<TCoordinate> factory)
        {
            _geomFact = factory;
            _ptList = new List<TCoordinate>();
        }

        public IPrecisionModel<TCoordinate> PrecisionModel
        {
            get { return _precisionModel; }
            set { _precisionModel = value; }
        }

        public Double MinimumVertexDistance
        {
            get { return _minimimVertexDistance; }
            set { _minimimVertexDistance = value; }
        }

        public void Add(TCoordinate pt)
        {
            TCoordinate bufPt = pt.Clone();
            //_precisionModel.MakePrecise(bufPt);
            // don't add duplicate (or near-duplicate) points
            if (IsDuplicate(bufPt))
                return;
            _ptList.Add(bufPt);
        }

        /**
        * Tests whether the given point duplicates the previous
        * point in the list (up to tolerance)
        * 
        * @param pt
        * @return true if the point duplicates the previous point
        */
        private Boolean IsDuplicate(TCoordinate pt)
        {
            if (_ptList.Count < 1)
                return false;
            TCoordinate lastPt = _ptList[_ptList.Count - 1];
            Double ptDist = pt.Distance(lastPt);
            if (ptDist < _minimimVertexDistance)
                return true;
            return false;
        }

        ///<summary>
        ///</summary>
        public void CloseRing()
        {
            if (_ptList.Count < 1)
                return;
            TCoordinate startPt = _ptList[0].Clone();
            TCoordinate lastPt = _ptList[_ptList.Count - 1];
            //TCoordinate last2Pt = default(TCoordinate);
            //if (_ptList.Count >= 2)
            //    last2Pt = _ptList[_ptList.Count - 2];
            if (startPt.Equals(lastPt))
                return;
            _ptList.Add(startPt);
        }

        public void AddRange(IEnumerable<TCoordinate> pts, Boolean isForward)
        {
            if (isForward)
            {
                foreach(TCoordinate pt in pts) 
                    Add(pt);
            }
            else
            {
                foreach (TCoordinate pt in new Stack<TCoordinate>(pts))
                    Add(pt);
            }
        }
  


        public TCoordinate[] GetCoordinates()
        {
            /*
            // check that points are a ring - add the startpoint again if they are not
            if (_ptList.Count > 1)
            {
                TCoordinate start = _ptList[0];
                TCoordinate end = _ptList[_ptList.Count - 1];
                if (!start.Equals(end)) Add(start);
            }
            */
            return _ptList.ToArray();
        }

        public override String ToString()
        {
            ICoordinateSequence<TCoordinate> tmp = _geomFact.CoordinateSequenceFactory.Create(_ptList);
            tmp.CloseRing();
            ILineString<TCoordinate> line = _geomFact.CreateLineString(tmp);
            return line.ToString();
        }
    }
}
