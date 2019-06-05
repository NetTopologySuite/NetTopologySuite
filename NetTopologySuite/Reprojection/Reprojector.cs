using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Reprojection
{
    /// <summary>
    /// A class for reprojection
    /// </summary>
    public class Reprojector
    {
        private static bool NoopReprojectionMessageShown = true;
        private static string _definitionKind = "wkt";

        /// <summary>
        /// Gives access to the current Reprojector instance
        /// </summary>
        public static Reprojector Instance { get; set; } = new Reprojector();

        /// <summary>
        /// Creates an instance of this class using the default <see cref="NetTopologySuite.Geometries.CoordinateSequenceFactory"/>
        /// and <see cref="NetTopologySuite.Geometries.PrecisionModel"/> that are defined in <see cref="NtsGeometryServices.Instance"/>.
        /// </summary>
        public Reprojector()
            : this(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, NtsGeometryServices.Instance.DefaultPrecisionModel)
        {
        }

        public Reprojector(CoordinateSequenceFactory coordinateSequenceFactory)
            :this(coordinateSequenceFactory, NtsGeometryServices.Instance.DefaultPrecisionModel)
        {
        }

        public Reprojector(PrecisionModel precisionModel)
            : this(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, precisionModel)
        {
        }

        public Reprojector(CoordinateSequenceFactory coordinateSequenceFactory, PrecisionModel precisionModel)
        {
            CoordinateSequenceFactory = coordinateSequenceFactory;
            PrecisionModel = precisionModel;
        }

        protected Dictionary<int, SpatialReference> SpatialReferences { get; } = new Dictionary<int, SpatialReference>();

        protected CoordinateSequenceFactory CoordinateSequenceFactory { get; }

        protected PrecisionModel PrecisionModel { get; }


        public Geometry Reproject(Geometry geometry, SpatialReference from, SpatialReference to)
        {
            if (!CheckArguments(geometry, nameof(geometry), from, to, out var e))
                throw e;

            if (geometry is Point pnt)
                return to.Factory.CreatePoint(Reproject(pnt.CoordinateSequence, from, to));

            if (geometry is LineString lnstrng)
                return to.Factory.CreateLineString(Reproject(lnstrng.CoordinateSequence, from, to));

            if (geometry is Polygon plygn)
            {
                var ex = to.Factory.CreateLinearRing(Reproject(plygn.ExteriorRing.CoordinateSequence, from, to));
                var hls = new LinearRing[plygn.NumInteriorRings];
                for (int i = 0; i < hls.Length; i++)
                    hls[i] = to.Factory.CreateLinearRing(Reproject(plygn.InteriorRings[i].CoordinateSequence, to, from));
                return to.Factory.CreatePolygon(ex, hls);
            }

            if (geometry is GeometryCollection)
            {
                var res = new Geometry[geometry.NumGeometries];
                for (int i = 0; i < res.Length; i++)
                    res[i] = Reproject(geometry.GetGeometryN(i), from, to);
                return to.Factory.BuildGeometry(res);
            }

            throw new NotSupportedException($"Reprojecting geometries of '{geometry.OgcGeometryType}' is not supported.");
        }

        public virtual Envelope Reproject(Envelope envelope, SpatialReference from, SpatialReference to)
        {
            if (!CheckArguments(envelope, nameof(envelope), from, to, out var e))
                throw e;

            ShowNoopReprojection();
            return envelope.Copy();
        }

        public virtual Coordinate Reproject(Coordinate coordinate, SpatialReference from, SpatialReference to)
        {
            if (!CheckArguments(coordinate, nameof(coordinate), from, to, out var e))
                throw e;

            ShowNoopReprojection();
            return coordinate.Copy();
        }

        public virtual CoordinateSequence Reproject(CoordinateSequence coordinateSequence, SpatialReference from, SpatialReference to)
        {
            if (!CheckArguments(coordinateSequence, nameof(coordinateSequence), from, to, out var e))
                throw e;

            ShowNoopReprojection();
            return coordinateSequence.Copy();
        }



        protected bool CheckArguments(object instance, string instanceName, SpatialReference from, SpatialReference to, out ArgumentException e)
        {
            e = null;
            if (instance == null)
                e = new ArgumentNullException(instanceName);

            if (CastSpatialReference<SpatialReference>(from) == null)
                e = new ArgumentNullException(nameof(from));

            if (CastSpatialReference<SpatialReference>(from) == null)
                e = new ArgumentNullException(nameof(to));

            return e == null;
        }

        protected virtual T CastSpatialReference<T>(SpatialReference sr) where T:SpatialReference
        {
            return (T)sr;
        }

        public SpatialReferenceLookUp LookUp { get; set; } = new EpsgIoSpatialReferenceLookUp("wkt");

        public SpatialReference GetSpatialReference(int srid)
        {
            if (SpatialReferences.TryGetValue(srid, out var sr))
                return sr;

            string definition = LookUp.GetDefinition(srid).Result;
            if (string.IsNullOrWhiteSpace(definition))
                throw new ArgumentException(nameof(srid));

            sr = Create(definition, srid);
            SpatialReferences[srid] = sr;

            return sr;
        }

        /// <summary>
        /// Method to create a spatial reference 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="srid"></param>
        /// <returns></returns>
        protected virtual SpatialReference Create(string definition, int srid)
        {
            var gf = new GeometryFactory(PrecisionModel, srid, CoordinateSequenceFactory);
            return new SpatialReference(definition, gf);
        }

        private static void ShowNoopReprojection()
        {
            if (NoopReprojectionMessageShown) return;
            System.Diagnostics.Trace.WriteLine("This Reprojector does not perfrom any reprojection!");
            NoopReprojectionMessageShown = true;
        }
    }
}
