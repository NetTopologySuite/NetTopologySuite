using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.CoordinateSystems.Transformations;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.CoordinateSystems.Transformations
{
    [TestFixtureAttribute]
    public class GeometryTransformTest
    {
        private class AffineTransform : IMathTransform
        {
            private readonly AffineTransformation _affineTransformation;
            private readonly AffineTransformation _inverseAffineTransformation;
            private AffineTransformation _transformation;

            public AffineTransform()
                :this(new AffineTransformation())
            {
            }

            public AffineTransform(AffineTransformation affineTransformation)
            {
                _affineTransformation = affineTransformation;
                _inverseAffineTransformation = affineTransformation.GetInverse();
                _transformation = affineTransformation;

                DimSource = DimTarget = 2;
                //WKT = "NTS AffineTransformation";
            }

            /// <summary>
            /// Gets the dimension of input points.
            /// </summary>
            public int DimSource { get; private set; }

            /// <summary>
            /// Gets the dimension of output points.
            /// </summary>
            public int DimTarget { get; private set; }

            /// <summary>
            /// Tests whether this transform does not move any points.
            /// </summary>
            /// <returns></returns>
            public bool Identity()
            {
                return _affineTransformation.IsIdentity;
            }

            /// <summary>
            /// Gets a Well-Known text representation of this object.
            /// </summary>
            public string WKT { get; private set; }

            /// <summary>
            /// Gets an XML representation of this object.
            /// </summary>
            public string XML { get; private set; }

            /// <summary>
            /// Gets the derivative of this transform at a point. If the transform does 
            /// not have a well-defined derivative at the point, then this function should 
            /// fail in the usual way for the DCP. The derivative is the matrix of the 
            /// non-translating portion of the approximate affine map at the point. The
            /// matrix will have dimensions corresponding to the source and target 
            /// coordinate systems. If the input dimension is M, and the output dimension 
            /// is N, then the matrix will have size [M][N]. The elements of the matrix 
            /// {elt[n][m] : n=0..(N-1)} form a vector in the output space which is 
            /// parallel to the displacement caused by a small change in the m'th ordinate 
            /// in the input space.
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public double[,] Derivative(double[] point)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Gets transformed convex hull.
            /// </summary>
            /// <remarks>
            /// <para>The supplied ordinates are interpreted as a sequence of points, which generates a convex
            /// hull in the source space. The returned sequence of ordinates represents a convex hull in the 
            /// output space. The number of output points will often be different from the number of input 
            /// points. Each of the input points should be inside the valid domain (this can be checked by 
            /// testing the points' domain flags individually). However, the convex hull of the input points
            /// may go outside the valid domain. The returned convex hull should contain the transformed image
            /// of the intersection of the source convex hull and the source domain.</para>
            /// <para>A convex hull is a shape in a coordinate system, where if two positions A and B are 
            /// inside the shape, then all positions in the straight line between A and B are also inside 
            /// the shape. So in 3D a cube and a sphere are both convex hulls. Other less obvious examples 
            /// of convex hulls are straight lines, and single points. (A single point is a convex hull, 
            /// because the positions A and B must both be the same - i.e. the point itself. So the straight
            /// line between A and B has zero length.)</para>
            /// <para>Some examples of shapes that are NOT convex hulls are donuts, and horseshoes.</para>
            /// </remarks>
            /// <param name="points"></param>
            /// <returns></returns>
            public List<double> GetCodomainConvexHull(List<double> points)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Gets flags classifying domain points within a convex hull.
            /// </summary>
            /// <remarks>
            ///  The supplied ordinates are interpreted as a sequence of points, which 
            /// generates a convex hull in the source space. Conceptually, each of the 
            /// (usually infinite) points inside the convex hull is then tested against
            /// the source domain. The flags of all these tests are then combined. In 
            /// practice, implementations of different transforms will use different 
            /// short-cuts to avoid doing an infinite number of tests.
            /// </remarks>
            /// <param name="points"></param>
            /// <returns></returns>
            public DomainFlags GetDomainFlags(List<double> points)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Creates the inverse transform of this object.
            /// </summary>
            /// <remarks>This method may fail if the transform is not one to one. However, all cartographic projections should succeed.</remarks>
            /// <returns></returns>
            public IMathTransform Inverse()
            {
                if (_inverseAffineTransformation != null)
                    return new AffineTransform(_inverseAffineTransformation);
                throw new System.NotSupportedException();
            }

            /// <summary>
            /// Transforms a coordinate point. The passed parameter point should not be modified.
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public double[] Transform(double[] point)
            {
                var cin = new Coordinate(point[0], point[1]);
                var cout = new Coordinate();
                cout = _affineTransformation.Transform(cin, cout);
                return new double[] { cout.X, cout.Y };
            }

            /// <summary>
            /// Transforms a a coordinate. The input coordinate remains unchanged.
            /// </summary>
            /// <param name="coordinate">The coordinate to transform</param>
            /// <returns>The transformed coordinate</returns>
            public ICoordinate Transform(ICoordinate coordinate)
            {
                var cout = new Coordinate(coordinate);
                return _affineTransformation.Transform(cout, cout);
            }

            /// <summary>
            /// Transforms a a coordinate. The input coordinate remains unchanged.
            /// </summary>
            /// <param name="coordinate">The coordinate to transform</param>
            /// <returns>The transformed coordinate</returns>
            public Coordinate Transform(Coordinate coordinate)
            {
                return _affineTransformation.Transform(coordinate, new Coordinate());
            }

            /// <summary>
            /// Transforms a list of coordinate point ordinal values.
            /// </summary>
            /// <remarks>
            /// This method is provided for efficiently transforming many points. The supplied array 
            /// of ordinal values will contain packed ordinal values. For example, if the source 
            /// dimension is 3, then the ordinals will be packed in this order (x0,y0,z0,x1,y1,z1 ...).
            /// The size of the passed array must be an integer multiple of DimSource. The returned 
            /// ordinal values are packed in a similar way. In some DCPs. the ordinals may be 
            /// transformed in-place, and the returned array may be the same as the passed array.
            /// So any client code should not attempt to reuse the passed ordinal values (although
            /// they can certainly reuse the passed array). If there is any problem then the server
            /// implementation will throw an exception. If this happens then the client should not
            /// make any assumptions about the state of the ordinal values.
            /// </remarks>
            /// <param name="points"></param>
            /// <returns></returns>
            public IList<double[]> TransformList(IList<double[]> points)
            {
                var list = new List<double[]>();
                foreach (var doublese in list)
                {
                    list.Add(Transform(doublese));
                }
                return list;
            }

            /// <summary>
            /// Transforms a list of coordinates.
            /// </summary>
            /// <remarks>
            /// This method is provided for efficiently transforming many points. The supplied array 
            /// of ordinal values will contain packed ordinal values. For example, if the source 
            /// dimension is 3, then the ordinals will be packed in this order (x0,y0,z0,x1,y1,z1 ...).
            /// The size of the passed array must be an integer multiple of DimSource. The returned 
            /// ordinal values are packed in a similar way. In some DCPs. the ordinals may be 
            /// transformed in-place, and the returned array may be the same as the passed array.
            /// So any client code should not attempt to reuse the passed ordinal values (although
            /// they can certainly reuse the passed array). If there is any problem then the server
            /// implementation will throw an exception. If this happens then the client should not
            /// make any assumptions about the state of the ordinal values.
            /// </remarks>
            /// <param name="points"></param>
            /// <returns></returns>
            public IList<Coordinate> TransformList(IList<Coordinate> points)
            {
                var list = new List<Coordinate>();
                foreach (var c in list)
                {
                    list.Add(Transform(c));
                }
                return list;
            }

            private AffineTransform _inverse;
            private bool useInverse;
            /// <summary>
            /// Reverses the transformation
            /// </summary>
            public void Invert()
            {
                if (_transformation == _affineTransformation)
                {
                    if (_inverseAffineTransformation == null)
                        throw new NotSupportedException();
                    _transformation = _inverseAffineTransformation;
                }
                else
                {
                    _transformation = _affineTransformation;
                }
            }

            /// <summary>
            /// Transforms a coordinate sequence. The input coordinate sequence remains unchanged.
            /// </summary>
            /// <param name="coordinateSequence">The coordinate sequence to transform.</param>
            /// <returns>The transformed coordinate sequence.</returns>
            public ICoordinateSequence Transform(ICoordinateSequence coordinateSequence)
            {
                var c = (ICoordinateSequence)coordinateSequence.Clone();
                for (var i = 0; i < c.Count; i++)
                    _affineTransformation.Transform(c, i);
                return c;
            }
        }

        private IMathTransform _mathTransform;
        private IGeometryFactory _factory;
        private WKTReader _reader;

        [SetUpAttribute]
        public void SetUp()
        {
            var a = new AffineTransformation();
            a.SetToTranslation(10, 10);
            _mathTransform = new AffineTransform(a);

            _factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(new PrecisionModel(1000d));
            _reader = new WKTReader(_factory);
        }

        /*
                    "POINT ( 10 20 )",
                    "LINESTRING EMPTY",
                    "LINESTRING(0 0, 10 10)",
                    "MULTILINESTRING ((50 100, 100 200), (100 100, 150 200))",
                    "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                    "MULTIPOLYGON (((100 200, 200 200, 200 100, 100 100, 100 200)), ((300 200, 400 200, 400 100, 300 100, 300 200)))",
                    "GEOMETRYCOLLECTION (POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200)), LINESTRING (250 100, 350 200), POINT (350 150))"
         */

        [TestAttribute]
        public void TestPoint()
        {
            const string wkt = "POINT ( 10 20 )";
            var g1 = (IPoint)_reader.Read(wkt);
            var g2 = (IPoint)GeometryTransform.TransformGeometry(_factory, g1,
                _mathTransform);

            TestGeometry(g1, g2);
        }

        [TestAttribute]
        public void TestLineString()
        {
            const string wkt = "LINESTRING(0 0, 10 10)";
            var g1 = (ILineString)_reader.Read(wkt);
            var g2 = (ILineString)GeometryTransform.TransformGeometry(_factory, g1,
                _mathTransform);

            TestGeometry(g1, g2);
        }

        [TestAttribute]
        public void TestPolygon()
        {
            const string wkt = "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))";
            var g1 = (IPolygon)_reader.Read(wkt);
            var g2 = (IPolygon)GeometryTransform.TransformGeometry(_factory, g1,
                _mathTransform);

            TestGeometry(g1, g2);
        }

        [TestAttribute]
        public void TestGeometryCollection()
        {
            const string wkt = "GEOMETRYCOLLECTION (POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200)), LINESTRING (250 100, 350 200), POINT (350 150))";
            var g1 = (IGeometryCollection)_reader.Read(wkt);
            var g2 = (IGeometryCollection)GeometryTransform.TransformGeometry(_factory, g1,
                _mathTransform);

            TestGeometry(g1, g2);
        }

        private static void TestGeometry(IGeometry g1, IGeometry g2)
        {
            if (g1 is IPoint)
            {
                TestCoordinateSequences(((IPoint)g1).CoordinateSequence, ((IPoint)g2).CoordinateSequence);
                return;
            }

            if (g1 is ILineString)
            {
                TestCoordinateSequences(((ILineString)g1).CoordinateSequence, ((ILineString)g2).CoordinateSequence);
                return;
            }

            if (g1 is IPolygon)
            {
                TestGeometry(((IPolygon)g1).ExteriorRing, ((IPolygon)g2).ExteriorRing);
                for (var i = 0; i < ((IPolygon)g1).NumInteriorRings; i++)
                    TestGeometry(((IPolygon)g1).GetInteriorRingN(i), ((IPolygon)g2).GetInteriorRingN(i));

                return;
            }

            if (g1 is IGeometryCollection)
            {
                for (var i = 0; i < g1.NumGeometries;i++ )
                {
                    TestGeometry(g1.GetGeometryN(i), g2.GetGeometryN(i));
                }
                return;
            }

            Assert.IsTrue(false, "Should never reach here!");
        }

        private static void TestCoordinateSequences(ICoordinateSequence orig, ICoordinateSequence trans)
        {
            Assert.AreNotSame(orig, trans, "Seqences are same");

            Assert.AreEqual(orig.Count, trans.Count, "Sequences have different lengths");
            Assert.AreEqual(orig.Ordinates, trans.Ordinates, "Sequences provide different ordinates");

            var ordinates = OrdinatesUtility.ToOrdinateArray(orig.Ordinates);
            for (var i = 0; i < orig.Count; i++)
            {
                foreach (var ordinate in ordinates)
                {
                    var v1 = orig.GetOrdinate(i, ordinate);
                    var v2 = trans.GetOrdinate(i, ordinate);

                    if (double.IsNaN(v1))
                    {
                        Assert.IsTrue(double.IsNaN(v2));
                        continue;
                    }

                    if (double.IsPositiveInfinity(v1))
                    {
                        Assert.IsTrue(double.IsPositiveInfinity(v2));
                        continue;
                    }
                    if (double.IsNegativeInfinity(v1))
                    {
                        Assert.IsTrue(double.IsNegativeInfinity(v2));
                        continue;
                    }

                    Assert.AreNotEqual(v1, v2, "Sequences provide equal value for '{0}'", ordinate);
                }
            }
        }
    }
}