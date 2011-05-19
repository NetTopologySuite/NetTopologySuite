using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    ///<summary>
    /// A list of the vertices in a constructed offset curve.
    ///</summary>
    /// <remarks>Automatically removes close adjacent vertices.</remarks>
    /// <author>Martin Davis</author>
    public class OffsetCurveVertexList
    {
        private readonly List<ICoordinate> _ptList;
        private IPrecisionModel _precisionModel;

        private double _minimimVertexDistance;

        public OffsetCurveVertexList()
        {
            _ptList = new List<ICoordinate>();
        }

        /// <summary>
        /// Gets/Sets the precision model to use when adding new points.
        /// </summary>
        public IPrecisionModel PrecisionModel { get { return _precisionModel; } set { _precisionModel = value; } }

        /// <summary>
        /// The distance below which two adjacent points on the curve are considered to be coincident.
        /// </summary>
        /// <remarks>This is chosen to be a small fraction of the offset distance.</remarks>
        public double MinimumVertexDistance { get { return _minimimVertexDistance; } set { _minimimVertexDistance = value; } }

        /// <summary>
        /// Function to add a point
        /// </summary>
        /// <remarks>
        /// The point is only added if <see cref="IsDuplicate(ICoordinate)"/> evaluates to false.
        /// </remarks>
        /// <param name="pt">The point to add.</param>
        public void AddPt(ICoordinate pt)
        {
            ICoordinate bufPt = new Coordinate(pt);
            _precisionModel.MakePrecise(bufPt);
            // don't add duplicate (or near-duplicate) points
            if (IsDuplicate(bufPt))
                return;
            _ptList.Add(bufPt);
            //System.out.println(bufPt);
        }

        ///<summary>
        /// Tests whether the given point duplicates the previous point in the list (up to tolerance)
        ///</summary>
        /// <param name="pt">The point to test</param>
        /// <returns>true if the point duplicates the previous point</returns>
        private bool IsDuplicate(ICoordinate pt)
        {
            if (_ptList.Count < 1)
                return false;
            ICoordinate lastPt = _ptList[_ptList.Count - 1];
            double ptDist = pt.Distance(lastPt);
            if (ptDist < _minimimVertexDistance)
                return true;
            return false;
        }

        /// <summary>
        /// Automatically closes the ring (if it not alread is).
        /// </summary>
        public void CloseRing()
        {
            if (_ptList.Count < 1) return;
            ICoordinate startPt = new Coordinate(_ptList[0]);
            ICoordinate lastPt = _ptList[_ptList.Count - 1];
            /*ICoordinate last2Pt = null;
              if (ptList.Count >= 2)
                  last2Pt = (Coordinate)ptList[ptList.Count - 2];*/
            if (startPt.Equals(lastPt)) return;
            _ptList.Add(startPt);
        }

        /// <summary>
        /// Gets the Coordinates for the curve.
        /// </summary>
        public ICoordinate[] Coordinates
        {
            get
            {
                // check that points are a ring - add the startpoint again if they are not
                if (_ptList.Count > 1)
                {
                    ICoordinate start = _ptList[0];
                    ICoordinate end = _ptList[_ptList.Count - 1];
                    if (!start.Equals(end)) AddPt(start);
                }
                ICoordinate[] coord = _ptList.ToArray();
                return coord;
            }
        }

        public override String ToString()
        {
            GeometryFactory fact = new GeometryFactory();
            ILineString line = fact.CreateLineString(Coordinates);
            return line.ToString();
        }
    }
}