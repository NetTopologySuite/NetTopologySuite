using System;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    public class Reprojection
    {
        private static bool _noopReprojectionMessageShown;

        /// <summary>
        /// Gets or sets a value indicating if the envelope that is to be reprojected should be densified to
        /// make the result value more accurate. 
        /// </summary>
        public static bool Densify { get; set; }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public Reprojection(SpatialReference source, SpatialReference target)
        {
            Source = source;
            Target = target;
        }

        /// <summary>
        /// Gets the source spatial reference definition
        /// </summary>
        public SpatialReference Source { get; }

        /// <summary>
        /// Gets the target spatial reference definition
        /// </summary>
        public SpatialReference Target { get; }


        /// <summary>
        /// Applies the the 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public Coordinate Apply(Coordinate coordinate)
        {
            return Apply(Source.Factory.CoordinateSequenceFactory.Create(new[] {coordinate})).GetCoordinate(0);
        }

        /// <summary>
        /// Applies the
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public Envelope Apply(Envelope envelope)
        {
            LineString seqGeom = ((Polygon)Source.Factory.ToGeometry(envelope)).Shell;
            if (Densify)
                seqGeom = (LineString)NetTopologySuite.Densify.Densifier.Densify(seqGeom, envelope.MinExtent / 10.0);
            var applied = ((LineString)Apply(seqGeom)).CoordinateSequence;
            var res = new Envelope();
            for (int i = 0; i < applied.Count - 1; i++)
                res.ExpandToInclude(applied.GetX(i), applied.GetY(i));

            return res;
        }

        public Geometry Apply(Geometry geometry)
        {
            if (geometry is Point pnt)
                return Target.Factory.CreatePoint(Apply(pnt.CoordinateSequence));

            if (geometry is LineString lnstrng)
                return Target.Factory.CreateLineString(Apply(lnstrng.CoordinateSequence));

            if (geometry is Polygon plygn)
            {
                var ex = Target.Factory.CreateLinearRing(Apply(plygn.ExteriorRing.CoordinateSequence));
                var hls = new LinearRing[plygn.NumInteriorRings];
                for (int i = 0; i < hls.Length; i++)
                    hls[i] = Target.Factory.CreateLinearRing(Apply(plygn.InteriorRings[i].CoordinateSequence));
                return Target.Factory.CreatePolygon(ex, hls);
            }

            if (geometry is GeometryCollection)
            {
                var res = new Geometry[geometry.NumGeometries];
                for (int i = 0; i < res.Length; i++)
                    res[i] = Apply(geometry.GetGeometryN(i));
                return Target.Factory.BuildGeometry(res);
            }

            throw new NotSupportedException($"Reprojecting geometries of '{geometry.OgcGeometryType}' is not supported.");
        }

        public async Task<Geometry> ApplyAsync(Geometry geometry)
        {
            if (geometry is Point pnt)
                return Target.Factory.CreatePoint(await ApplyAsync(pnt.CoordinateSequence));

            if (geometry is LineString lnstrng)
                return Target.Factory.CreateLineString(await ApplyAsync(lnstrng.CoordinateSequence));

            if (geometry is Polygon plygn)
            {
                var ex = Target.Factory.CreateLinearRing(await ApplyAsync(plygn.ExteriorRing.CoordinateSequence));
                var hls = new LinearRing[plygn.NumInteriorRings];
                for (int i = 0; i < hls.Length; i++)
                    hls[i] = Target.Factory.CreateLinearRing(await ApplyAsync(plygn.InteriorRings[i].CoordinateSequence));
                return Target.Factory.CreatePolygon(ex, hls);
            }

            if (geometry is GeometryCollection)
            {
                var res = new Geometry[geometry.NumGeometries];
                for (int i = 0; i < res.Length; i++)
                    res[i] = await ApplyAsync(geometry.GetGeometryN(i));
                return Target.Factory.BuildGeometry(res);
            }

            throw new NotSupportedException($"Reprojecting geometries of '{geometry.OgcGeometryType}' is not supported.");
        }

        protected virtual CoordinateSequence Apply(CoordinateSequence coordinateSequence)
        {
            ShowNoopReprojection();
            return Target.Factory.CoordinateSequenceFactory.Create(coordinateSequence);
        }

        protected virtual async Task<CoordinateSequence> ApplyAsync(CoordinateSequence coordinateSequence)
        {
            return await Task.Run(() => Apply(coordinateSequence));
        }

        public override string ToString()
        {
            return $"Reprojection {Source.Factory.SRID} -> {Target.Factory.SRID}";
        }
        private static void ShowNoopReprojection()
        {
            if (_noopReprojectionMessageShown) return;
            System.Diagnostics.Trace.WriteLine("This Reprojection does not perform any reprojection!");
            _noopReprojectionMessageShown = true;
        }
    }
}
