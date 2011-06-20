using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Coordinates;
using NPack.Interfaces;
using GeoAPI.Geometries;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Densify
{
    ///<summary>
    ///Densifies a geometry by inserting extra vertices along the line segments
    ///in the geometry. The densified geometry contains no line segment which
    ///is longer than the given distance tolerance.
    ///Densified polygonal geometries are guaranteed to be topologically correct.
    ///The coordinates created during densification respect the input geometry's
    ///{@link PrecisionModel}.
    ///Note: At some future point this class will offer a variety of densification strategies.
    /// </summary>
    public class Densifier<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
    {
        ///<summary>
        ///Densifies a geometry using a given distance tolerance,
        ///and respecting the input geometry's <see cref="IPrecisionModel{TCoordinate}"/>.
        ///</summary>
        ///<param name="geom">Geometry to densify</param>
        ///<param name="distanceTolerance">Distance tolerance to densify</param>
        ///<returns>The densified geometry</returns>
        public static IGeometry<TCoordinate> Densify(IGeometry<TCoordinate> geom, Double distanceTolerance)
        {
            Densifier<TCoordinate> densifier = new Densifier<TCoordinate>(geom);
            densifier.DistanceTolerance = distanceTolerance;
            return densifier.GetResultGeometry();
        }

        /**
         * Densifies a coordinate sequence.
         * 
         * @param pts
         * @param distanceTolerance
         * @return the densified coordinate sequence
         */
        protected static TCoordinate[] DensifyPoints(TCoordinate[] pts,
                Double distanceTolerance, ICoordinateFactory<TCoordinate> coordinateFactory)
        {
            //LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>();
            CoordinateList<TCoordinate> coordList = new CoordinateList<TCoordinate>();
            for (Int32 i = 0; i < pts.Length - 1; i++)// in (IEnumerable<TCoordinate>)pts)
            {
                LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>(pts[i], pts[i + 1]);
                coordList.Add(seg.P0);
                Double length = seg.Length;
                Int32 densifiedSegCount = (Int32)(length / distanceTolerance) + 1;
                if (densifiedSegCount > 1)
                {
                    Double densifiedSegLength = length / densifiedSegCount;
                    for (Int32 j = 0; j < densifiedSegCount; j++)
                    {
                        Double segFract = (j * densifiedSegLength) / length;
                        coordList.Add(seg.PointAlong(coordinateFactory, segFract), false);
                    }
                }

            }
            coordList.Add(pts[pts.Length - 1], false);
            return coordList.ToArray();
        }

        private IGeometry<TCoordinate> _inputGeom;
        private IGeometryFactory<TCoordinate> _factory;
        private Double _distanceTolerance;

        /**
         * 
         * 
         * @param inputGeom
         */

        ///<summary>
        ///Creates a new densifier instance. Assigns inputGeom's GeometryFactory
        ///</summary>
        ///<param name="inputGeom">geometry to densify</param>
        public Densifier(IGeometry<TCoordinate> inputGeom)
            : this(inputGeom, inputGeom == null ? null : inputGeom.Factory)
        {
        }

        ///<summary>
        ///Creates a new densifier instance. Assigns inputGeom's GeometryFactory
        ///</summary>
        ///<param name="inputGeom">geometry to densify</param>
        ///<param name="factory">geometryfactory to create densified geometry</param>
        public Densifier(IGeometry<TCoordinate> inputGeom, IGeometryFactory<TCoordinate> factory)
        {
            _inputGeom = inputGeom;
            _factory = factory;
        }

        ///<summary>
        ///Sets the distance tolerance for the densification. All line segments
        ///in the densified geometry will be no longer than the distance tolereance.
        ///simplified geometry will be within this distance of the original geometry.
        ///The distance tolerance must be positive.
        ///</summary>	
        public Double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException("Tolerance must be positive");
                _distanceTolerance = value;
            }
        }

        /**
         * Gets the densified geometry.
         * 
         * @return the densified geometry
         */
        public IGeometry<TCoordinate> GetResultGeometry()
        {
            return (new DensifyTransformer(_factory, _distanceTolerance)).Transform(_inputGeom);
        }

        class DensifyTransformer : GeometryTransformer<TCoordinate>
        {
            IGeometryFactory<TCoordinate> _factory;
            Double _distanceTolerance;

            public DensifyTransformer(IGeometryFactory<TCoordinate> factory, Double distanceTolerance)
            {
                _factory = factory;
                _distanceTolerance = distanceTolerance;
            }

            protected ICoordinateSequence<TCoordinate> TransformCoordinates(
                    ICoordinateSequence<TCoordinate> coords, IGeometry<TCoordinate> parent)
            {
                TCoordinate[] inputPts = coords.ToArray();
                TCoordinate[] newPts = Densifier<TCoordinate>.DensifyPoints(inputPts, _distanceTolerance, _factory.CoordinateFactory);
                // prevent creation of invalid linestrings
                if (parent is ILineString<TCoordinate> && newPts.Length == 1)
                {
                    newPts = new TCoordinate[0];
                }
                return _factory.CoordinateSequenceFactory.Create(newPts);
            }

            protected IGeometry<TCoordinate> TransformPolygon(IPolygon<TCoordinate> geom, IGeometry<TCoordinate> parent)
            {
                IGeometry<TCoordinate> roughGeom = base.TransformPolygon(geom, parent);
                // don't try and correct if the parent is going to do this
                if (parent is IMultiPolygon<TCoordinate>)
                    return roughGeom;

                return CreateValidArea(roughGeom);
            }

            protected IGeometry<TCoordinate> TransformMultiPolygon(IMultiPolygon<TCoordinate> geom, IGeometry<TCoordinate> parent)
            {
                IGeometry<TCoordinate> roughGeom = base.TransformMultiPolygon(geom, parent);
                return CreateValidArea(roughGeom);
            }

            /**
             * Creates a valid area geometry from one that possibly has bad topology
             * (i.e. self-intersections). Since buffer can handle invalid topology, but
             * always returns valid geometry, constructing a 0-width buffer "corrects"
             * the topology. Note this only works for area geometries, since buffer
             * always returns areas. This also may return empty geometries, if the input
             * has no actual area.
             * 
             * @param roughAreaGeom
             *          an area geometry possibly containing self-intersections
             * @return a valid area geometry
             */
            private IGeometry<TCoordinate> CreateValidArea(IGeometry<TCoordinate> roughAreaGeom)
            {
                return roughAreaGeom.Buffer(0.0);
            }
        }

    }
}
