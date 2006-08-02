using System;
using System.Collections;
using System.Text;

using Iesi.Collections;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the convex hull of a <c>Geometry</c>.
    /// The convex hull is the smallest convex Geometry that contains all the
    /// points in the input Geometry.
    /// Uses the Graham Scan algorithm.
    /// </summary>
    public class ConvexHull
    {
        private PointLocator pointLocator = new PointLocator();
        private Geometry geometry;
        private GeometryFactory factory;

        /// <summary> 
        /// Create a new convex hull construction for the input <c>Geometry</c>.
        /// </summary>
        /// <param name="geometry"></param>
        public ConvexHull(Geometry geometry)
        {
            this.geometry = geometry;
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
        public virtual Geometry GetConvexHull()
        {
            factory = geometry.Factory;

            UniqueCoordinateArrayFilter filter = new UniqueCoordinateArrayFilter();
            geometry.Apply(filter);
            Coordinate[] pts = filter.Coordinates;

            if (pts.Length == 0)
                return factory.CreateGeometryCollection(null);            
            if (pts.Length == 1)
                return factory.CreatePoint(pts[0]);            
            if (pts.Length == 2)            
                return factory.CreateLineString(pts);            

            // sort points for Graham scan.
            Coordinate[] pspts;
            if (pts.Length > 10)
            {
                //Probably should be somewhere between 50 and 100?
                Coordinate[] rpts = Reduce(pts);
                pspts = PreSort(rpts);
            }
            else pspts = PreSort(pts);            

            // Use Graham scan to find convex hull.
            Stack cHS = GrahamScan(pspts);

            // Convert stack to an array.
            Coordinate[] cH = ToCoordinateArray(cHS);

            // Convert array to linear ring.
            return LineOrPolygon(cH);
        }
  
        /// <summary>
        /// NOTE: why <c>(Coordinate[])stack.ToArray()</c> not work? Probably because ToArray(Type type) is missing...
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        protected virtual Coordinate[] ToCoordinateArray(Stack stack)
        {            
            object[] temp = stack.ToArray();
            Coordinate[] coords = new Coordinate[temp.Length];
            for(int i = 0; i < temp.Length; i++)
                coords[i] = (Coordinate)temp[i];
            return coords;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private Coordinate[] Reduce(Coordinate[] pts)
        {
            BigQuad(pts);

            // Build a linear ring defining a big poly.
            ArrayList bigPoly = new ArrayList();
            bigPoly.Add(BigQuadCoordinates.Westmost);
            if (!bigPoly.Contains(BigQuadCoordinates.Northmost))
                bigPoly.Add(BigQuadCoordinates.Northmost);
            if (!bigPoly.Contains(BigQuadCoordinates.Eastmost))
                bigPoly.Add(BigQuadCoordinates.Eastmost);
            if (!bigPoly.Contains(BigQuadCoordinates.Southmost))
                bigPoly.Add(BigQuadCoordinates.Southmost);           
            if (bigPoly.Count < 3)
                return pts;
            bigPoly.Add(BigQuadCoordinates.Westmost);
            Coordinate[] bigPolyArray = new Coordinate[bigPoly.Count];
            LinearRing bQ = factory.CreateLinearRing((Coordinate[])bigPoly.ToArray(typeof(Coordinate)));

            // load an array with all points not in the big poly
            // and the defining points.
            SortedSet reducedSet = new SortedSet(bigPoly);  
            for (int i = 0; i < pts.Length; i++)
            {
                if (pointLocator.Locate(pts[i], bQ) == Locations.Exterior)
                    reducedSet.Add(pts[i]);                
            }
            Coordinate[] rP = new Coordinate[reducedSet.Count];
            int index = 0;
            foreach (Coordinate coord in reducedSet)
                rP[index++] = coord;

            // Return this array as the reduced problem.
            return rP;
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
                if ((pts[i].Y < pts[0].Y) || ((pts[i].Y == pts[0].Y) && (pts[i].X < pts[0].X)))
                {
                    t = pts[0];
                    pts[0] = pts[i];
                    pts[i] = t;
                }
            }

            // sort the points radially around the focal point.
            RadialSort(pts);
            return pts;
        }

        /// <summary>
        /// NOTE: Seems strange but .NET Stack insert works in reverse order of Java Stack!
        /// I perform a manual reverse of the stack, but this introduce several delays...
        /// it's really necessary to perform the reverse?
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private Stack GrahamScan(Coordinate[] c)
        {
            return GrahamScan(c, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="doReverse">Reverse the stack if true</param>
        /// <returns></returns>
        private Stack GrahamScan(Coordinate[] c, bool doReverse)
        {
            Coordinate p;
            Stack ps = new Stack(c.Length);
            ps.Push(c[0]);
            ps.Push(c[1]);
            ps.Push(c[2]);
            for (int i = 3; i < c.Length; i++)
            {
                p = (Coordinate)ps.Pop();
                while (CGAlgorithms.ComputeOrientation((Coordinate)ps.Peek(), p, c[i]) > 0)
                    p = (Coordinate)ps.Pop();                
                ps.Push(p);
                ps.Push(c[i]);
            }
            ps.Push(c[0]);

            // perform reverse
            if (doReverse)
            {
                // Do a manual reverse of the stack
                int size = ps.Count;
                object[] tempArray = new object[size];
                for (int i = 0; i < size; i++)
                    tempArray[i] = ps.Pop();
                Stack returnStack = new Stack(size);
                foreach (object obj in tempArray)
                    returnStack.Push(obj);
                return returnStack;
            }
            return ps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        private void RadialSort(Coordinate[] p)
        {

            // A selection sort routine, assumes the pivot point is
            // the first point (i.e., p[0]).
            Coordinate t;
            for (int i = 1; i < (p.Length - 1); i++)
            {
                int min = i;
                for (int j = i + 1; j < p.Length; j++)
                    if (PolarCompare(p[0], p[j], p[min]) < 0)
                        min = j;                    
                t = p[i];
                p[i] = p[min];
                p[min] = t;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private int PolarCompare(Coordinate o, Coordinate p, Coordinate q)
        {

            // Given two points p and q compare them with respect to their radial
            // ordering about point o. -1, 0 or 1 depending on whether p is less than,
            // equal to or greater than q. First checks radial ordering then if both
            // points lie on the same line, check distance to o.
            double dxp = p.X - o.X;
            double dyp = p.Y - o.Y;
            double dxq = q.X - o.X;
            double dyq = q.Y - o.Y;
            double alph = Math.Atan2(dxp, dyp);
            double beta = Math.Atan2(dxq, dyq);
            if (alph < beta)
                return -1;            
            if (alph > beta)
                return 1;
            double op = dxp * dxp + dyp * dyp;
            double oq = dxq * dxq + dyq * dyq;
            if (op < oq)
                return -1;
            if (op > oq)
                return 1;
            return 0;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <returns>Whether the three coordinates are collinear and c2 lies between c1 and c3 inclusive.</returns>        
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
        /// <param name="pts"></param>
        /// <returns></returns>
        private void BigQuad(Coordinate[] pts) 
        {         
            BigQuadCoordinates.Northmost = pts[0];
            BigQuadCoordinates.Southmost = pts[0];
            BigQuadCoordinates.Westmost = pts[0];
            BigQuadCoordinates.Eastmost = pts[0];
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].X < BigQuadCoordinates.Westmost.X)
                    BigQuadCoordinates.Westmost = pts[i];
                if (pts[i].X > BigQuadCoordinates.Eastmost.X)
                    BigQuadCoordinates.Eastmost = pts[i];
                if (pts[i].Y < BigQuadCoordinates.Southmost.Y)
                    BigQuadCoordinates.Southmost = pts[i];
                if (pts[i].Y > BigQuadCoordinates.Northmost.Y)
                    BigQuadCoordinates.Northmost = pts[i];
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinates"> The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>A 2-vertex <c>LineString</c> if the vertices are collinear; 
        /// otherwise, a <c>Polygon</c> with unnecessary (collinear) vertices removed. </returns>       
        private Geometry LineOrPolygon(Coordinate[] coordinates)
        {
            coordinates = CleanRing(coordinates);
            if (coordinates.Length == 3)            
                return factory.CreateLineString(new Coordinate[] { coordinates[0], coordinates[1] });                            
            LinearRing linearRing = factory.CreateLinearRing(coordinates);
            return factory.CreatePolygon(linearRing, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original">The vertices of a linear ring, which may or may not be flattened (i.e. vertices collinear).</param>
        /// <returns>The coordinates with unnecessary (collinear) vertices removed.</returns>
        private Coordinate[] CleanRing(Coordinate[] original)
        {
            Assert.Equals(original[0], original[original.Length - 1]);
            ArrayList cleanedRing = new ArrayList();
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
            return (Coordinate[])cleanedRing.ToArray(typeof(Coordinate));
        }

        // NOTE: modified for "safe" assembly in Sql 2005
        // ReadOnly Property created
        private static readonly BigQuadCoords bigQuadCoordinates = new BigQuadCoords();

        private static BigQuadCoords BigQuadCoordinates
        {
            get 
            { 
                return ConvexHull.bigQuadCoordinates; 
            }
        } 
          
        /// <summary>
        /// 
        /// </summary>
        private class BigQuadCoords
        {                        
            public Coordinate Northmost = null;
            public Coordinate Southmost = null;
            public Coordinate Westmost = null;
            public Coordinate Eastmost = null;
        }
    }
}
