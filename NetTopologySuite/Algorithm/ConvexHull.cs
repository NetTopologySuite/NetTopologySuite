using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using Wintellect.PowerCollections;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the convex hull of a <see cref="Geometry" />.
    /// The convex hull is the smallest convex Geometry that contains all the
    /// points in the input Geometry.
    /// Uses the Graham Scan algorithm.
    /// </summary>
    public class ConvexHull
    {
        private IGeometryFactory geomFactory = null;
        private Coordinate[] inputPts = null;

        /// <summary> 
        /// Create a new convex hull construction for the input <c>Geometry</c>.
        /// </summary>
        /// <param name="geometry"></param>
        public ConvexHull(IGeometry geometry) 
            : this(ExtractCoordinates(geometry), geometry.Factory) { }

        /// <summary>
        /// Create a new convex hull construction for the input <see cref="Coordinate" /> array.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="geomFactory"></param>   
        public ConvexHull(Coordinate[] pts, IGeometryFactory geomFactory)
        {
            inputPts = pts;
            this.geomFactory = geomFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        private static Coordinate[] ExtractCoordinates(IGeometry geom)
        {
            UniqueCoordinateArrayFilter filter = new UniqueCoordinateArrayFilter();
            geom.Apply(filter);
            return filter.Coordinates;
        }

        /// <summary> 
        /// Returns a <c>Geometry</c> that represents the convex hull of the input point.
        /// The point will contain the minimal number of points needed to
        /// represent the convex hull.  In particular, no more than two consecutive
        /// points will be collinear.
        /// </summary>
        /// <returns> 
        /// If the convex hull contains 3 or more points, a <c>Polygon</c>;
        /// 2 points, a <c>LineString</c>;
        /// 1 point, a <c>Point</c>;
        /// 0 points, an empty <c>GeometryCollection</c>.
        /// </returns>
        public IGeometry GetConvexHull()
        {
            if (inputPts.Length == 0)
                return geomFactory.CreateGeometryCollection(null);
            
            if (inputPts.Length == 1)
                return geomFactory.CreatePoint(inputPts[0]);

            if (inputPts.Length == 2)
                return geomFactory.CreateLineString(inputPts);
            

            Coordinate[] reducedPts = inputPts;
            // use heuristic to reduce points, if large
            if (inputPts.Length > 50)
                reducedPts = Reduce(inputPts);
            
            // sort points for Graham scan.
            Coordinate[] sortedPts = PreSort(reducedPts);

            // Use Graham scan to find convex hull.
            Stack<Coordinate> cHS = GrahamScan(sortedPts);

            // Convert stack to an array.
            Coordinate[] cH = cHS.ToArray();

            // Convert array to appropriate output geometry.
            return LineOrPolygon(cH);
        }
          
        /// <summary>
        /// Uses a heuristic to reduce the number of points scanned to compute the hull.
        /// The heuristic is to find a polygon guaranteed to
        /// be in (or on) the hull, and eliminate all points inside it.
        /// A quadrilateral defined by the extremal points
        /// in the four orthogonal directions
        /// can be used, but even more inclusive is
        /// to use an octilateral defined by the points in the 8 cardinal directions.
        /// Note that even if the method used to determine the polygon vertices
        /// is not 100% robust, this does not affect the robustness of the convex hull.
        /// <para>
        /// To satisfy the requirements of the Graham Scan algorithm, 
        /// the returned array has at least 3 entries.
        /// </para>
        /// </summary>
        /// <param name="pts">The coordinates to reduce</param>
        /// <returns>The reduced array of coordinates</returns>
        private Coordinate[] Reduce(Coordinate[] pts)
        {
            Coordinate[] polyPts = ComputeOctRing(inputPts);
            
            // unable to compute interior polygon for some reason
            if(polyPts == null)
                return inputPts;
            
            // add points defining polygon
            OrderedSet<Coordinate> reducedSet = new OrderedSet<Coordinate>();
            for (int i = 0; i < polyPts.Length; i++)
                reducedSet.Add(polyPts[i]);
            
            /*
             * Add all unique points not in the interior poly.
             * CGAlgorithms.IsPointInRing is not defined for points actually on the ring,
             * but this doesn't matter since the points of the interior polygon
             * are forced to be in the reduced set.
             */
            for (int i = 0; i < inputPts.Length; i++)
                if (!CGAlgorithms.IsPointInRing(inputPts[i], polyPts))                
                    reducedSet.Add(inputPts[i]);

            Coordinate[] reducedPts = CoordinateArrays.ToCoordinateArray((ICollection<Coordinate>)reducedSet);// new Coordinate[reducedSet.Count];

            // ensure that computed array has at least 3 points (not necessarily unique)  
            if (reducedPts.Length < 3)
                return PadArray3(reducedPts);

            return reducedPts;
        }

        private Coordinate[] PadArray3(Coordinate[] pts)
        {
            Coordinate[] pad = new Coordinate[3];
            for (int i = 0; i < pad.Length; i++)
            {
                if (i < pts.Length)
                {
                    pad[i] = pts[i];
                }
                else
                    pad[i] = pts[0];
            }
            return pad;
        }
    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private Coordinate[] PreSort(Coordinate[] pts)
        {
            Coordinate t;

            // find the lowest point in the set. If two or more points have
            // the same minimum y coordinate choose the one with the minimu x.
            // This focal point is put in array location pts[0].
            for (int i = 1; i < pts.Length; i++)
            {
                if ((pts[i].Y < pts[0].Y) || ((pts[i].Y == pts[0].Y) 
                     && (pts[i].X < pts[0].X)))
                {
                    t = pts[0];
                    pts[0] = pts[i];
                    pts[i] = t;
                }
            }

            // sort the points radially around the focal point.
            Array.Sort(pts, 1, pts.Length - 1, new RadialComparator(pts[0]));
            return pts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>        
        private Stack<Coordinate> GrahamScan(Coordinate[] c)
        {
            Coordinate p;
            Stack<Coordinate> ps = new Stack<Coordinate>(c.Length);
            ps.Push(c[0]);
            ps.Push(c[1]);
            if(c.Length > 2)
            ps.Push(c[2]);
            for (int i = 3; i < c.Length; i++)
            {
                p = ps.Pop();
                while (CGAlgorithms.ComputeOrientation(ps.Peek(), p, c[i]) > 0)
                    p = ps.Pop();
                ps.Push(p);
                ps.Push(c[i]);
            }
            ps.Push(c[0]);
            return ps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>    
        private Stack<Coordinate> ReverseStack(Stack<Coordinate> ps) 
        {        
            // Do a manual reverse of the stack
            int size = ps.Count;
            Coordinate[] tempArray = new Coordinate[size];
            for (int i = 0; i < size; i++)
                tempArray[i] = ps.Pop();
            Stack<Coordinate> returnStack = new Stack<Coordinate>(size);
            foreach (Coordinate obj in tempArray)
                returnStack.Push(obj);
            return returnStack;                        
        }               
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <returns>
        /// Whether the three coordinates are collinear 
        /// and c2 lies between c1 and c3 inclusive.
        /// </returns>        
        private bool IsBetween(Coordinate c1, Coordinate c2, Coordinate c3)
        {
            if (CGAlgorithms.ComputeOrientation(c1, c2, c3) != 0)
                return false;
            if (c1.X != c3.X)
            {
                if (c1.X <= c2.X && c2.X <= c3.X)
                    return true;
                if (c3.X <= c2.X && c2.X <= c1.X)
                    return true;
            }
            if (c1.Y != c3.Y)
            {
                if (c1.Y <= c2.Y && c2.Y <= c3.Y)
                    return true;
                if (c3.Y <= c2.Y && c2.Y <= c1.Y)
                    return true;
            }
            return false;
        }              

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPts"></param>
        /// <returns></returns>
        private Coordinate[] ComputeOctRing(Coordinate[] inputPts)
        {
            Coordinate[] octPts = ComputeOctPts(inputPts);
            CoordinateList coordList = new CoordinateList();
            coordList.Add(octPts, false);

            // points must all lie in a line
            if (coordList.Count < 3)
                return null;
            
            coordList.CloseRing();
            return coordList.ToCoordinateArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPts"></param>
        /// <returns></returns>
        private Coordinate[] ComputeOctPts(Coordinate[] inputPts)
        {
            Coordinate[] pts = new Coordinate[8];
            for (int j = 0; j < pts.Length; j++)
                pts[j] = inputPts[0];
            
            for (int i = 1; i < inputPts.Length; i++)
            {
                if (inputPts[i].X < pts[0].X)
                    pts[0] = inputPts[i];
                
                if (inputPts[i].X - inputPts[i].Y < pts[1].X - pts[1].Y)
                    pts[1] = inputPts[i];
                
                if (inputPts[i].Y > pts[2].Y)                
                    pts[2] = inputPts[i];
                
                if (inputPts[i].X + inputPts[i].Y > pts[3].X + pts[3].Y)                
                    pts[3] = inputPts[i];
                
                if (inputPts[i].X > pts[4].X)
                    pts[4] = inputPts[i];
                
                if (inputPts[i].X - inputPts[i].Y > pts[5].X - pts[5].Y)                
                    pts[5] = inputPts[i];
                
                if (inputPts[i].Y < pts[6].Y)
                    pts[6] = inputPts[i];
                
                if (inputPts[i].X + inputPts[i].Y < pts[7].X + pts[7].Y)
                    pts[7] = inputPts[i];                
            }
            return pts;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinates"> The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>A 2-vertex <c>LineString</c> if the vertices are collinear; 
        /// otherwise, a <c>Polygon</c> with unnecessary (collinear) vertices removed. </returns>       
        private IGeometry LineOrPolygon(Coordinate[] coordinates)
        {
            coordinates = CleanRing(coordinates);
            if (coordinates.Length == 3)
                return geomFactory.CreateLineString(new Coordinate[] { coordinates[0], coordinates[1] });
            ILinearRing linearRing = geomFactory.CreateLinearRing(coordinates);
            return geomFactory.CreatePolygon(linearRing, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original">The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>The coordinates with unnecessary (collinear) vertices removed.</returns>
        private Coordinate[] CleanRing(Coordinate[] original)
        {
            Equals(original[0], original[original.Length - 1]);
            List<Coordinate> cleanedRing = new List<Coordinate>();
            Coordinate previousDistinctCoordinate = null;
            for (int i = 0; i <= original.Length - 2; i++)
            {
                Coordinate currentCoordinate = original[i];
                Coordinate nextCoordinate = original[i + 1];
                if (currentCoordinate.Equals(nextCoordinate))
                    continue;
                if (previousDistinctCoordinate != null &&
                    IsBetween(previousDistinctCoordinate, currentCoordinate, nextCoordinate))
                    continue;                
                cleanedRing.Add(currentCoordinate);
                previousDistinctCoordinate = currentCoordinate;
            }
            cleanedRing.Add(original[original.Length - 1]);
            return cleanedRing.ToArray();
        }

        /// <summary>
        /// Compares <see cref="Coordinate" />s for their angle and distance
        /// relative to an origin.
        /// </summary>
        private class RadialComparator : IComparer<Coordinate>
        {
            private Coordinate origin = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="RadialComparator"/> class.
            /// </summary>
            /// <param name="origin"></param>
            public RadialComparator(Coordinate origin)
            {
                this.origin = origin;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="p1"></param>
            /// <param name="p2"></param>
            /// <returns></returns>
            public int Compare(Coordinate p1, Coordinate p2)
            {                
                return PolarCompare(origin, p1, p2);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="o"></param>
            /// <param name="p"></param>
            /// <param name="q"></param>
            /// <returns></returns>
            private static int PolarCompare(Coordinate o, Coordinate p, Coordinate q)
            {
                double dxp = p.X - o.X;
                double dyp = p.Y - o.Y;
                double dxq = q.X - o.X;
                double dyq = q.Y - o.Y;
             
                int orient = CGAlgorithms.ComputeOrientation(o, p, q);

                if(orient == CGAlgorithms.CounterClockwise)
                    return 1;
                if(orient == CGAlgorithms.Clockwise) 
                    return -1;

                // points are collinear - check distance
                double op = dxp * dxp + dyp * dyp;
                double oq = dxq * dxq + dyq * dyq;
                if (op < oq)
                    return -1;                
                if (op > oq)
                    return 1;
                return 0;
            }
        }       
    }
}
