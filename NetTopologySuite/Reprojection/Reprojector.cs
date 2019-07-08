using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    /// <summary>
    /// A class for reprojection
    /// </summary>
    public class Reprojector
    {
        /// <summary>
        /// Gives access to the current Reprojector instance
        /// </summary>
        public static Reprojector Instance { get; set; } = new Reprojector();

        /// <summary>
        /// Gets a value indicating the spatial reference factory
        /// </summary>
        public SpatialReferenceFactory SpatialReferenceFactory { get; }

        /// <summary>
        /// Creates an instance of this class using the default <see cref="Geometries.CoordinateSequenceFactory"/>
        /// and <see cref="Geometries.PrecisionModel"/> that are defined in <see cref="NtsGeometryServices.Instance"/>.
        /// </summary>
        public Reprojector()
            : this(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, NtsGeometryServices.Instance.DefaultPrecisionModel)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the given <see cref="Geometries.CoordinateSequenceFactory"/>
        /// and the default <see cref="Geometries.PrecisionModel"/> that is defined in <see cref="NtsGeometryServices.Instance"/>.
        /// </summary>
        /// <param name="coordinateSequenceFactory">The factory used when creating <see cref="SpatialReference"/> objects.</param>
        public Reprojector(CoordinateSequenceFactory coordinateSequenceFactory)
            :this(coordinateSequenceFactory, NtsGeometryServices.Instance.DefaultPrecisionModel)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the given <see cref="Geometries.PrecisionModel"/> and the default
        /// <see cref="Geometries.CoordinateSequenceFactory"/> that is defined in <see cref="NtsGeometryServices.Instance"/>.
        /// </summary>
        /// <param name="precisionModel">The precision model used when creating <see cref="SpatialReference"/> objects.</param>
        public Reprojector(PrecisionModel precisionModel)
            : this(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, precisionModel)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the given <see cref="Geometries.PrecisionModel"/> and the default
        /// <see cref="Geometries.CoordinateSequenceFactory"/> that is defined in <see cref="NtsGeometryServices.Instance"/>.
        /// </summary>
        /// <param name="coordinateSequenceFactory">The factory used when creating <see cref="SpatialReference"/> objects.</param>
        /// <param name="precisionModel">The precision model used when creating <see cref="SpatialReference"/> objects.</param>
        public Reprojector(CoordinateSequenceFactory coordinateSequenceFactory, PrecisionModel precisionModel)
            : this(new EpsgIoSpatialReferenceFactory(coordinateSequenceFactory, precisionModel, "wkt"))
        {

        }

        /// <summary>
        /// Creates an instance of this class using the given <see cref="Geometries.PrecisionModel"/> and the default
        /// <see cref="Geometries.CoordinateSequenceFactory"/> that is defined in <see cref="NtsGeometryServices.Instance"/>.
        /// </summary>
        /// <param name="spatialReferenceFactory">The factory used when creating <see cref="SpatialReference"/> objects.</param>
        /// <param name="reprojectionFactory"></param>
        public Reprojector(SpatialReferenceFactory spatialReferenceFactory, ReprojectionFactory reprojectionFactory = null)
        {
            ReprojectionFactory = reprojectionFactory ?? new ReprojectionFactory();
            SpatialReferenceFactory = spatialReferenceFactory;
        }

        /// <summary>
        /// Gets a value that can create <see cref="Reprojection"/>s for use with this <see cref="Reprojector"/> or for reuse.
        /// </summary>
        public ReprojectionFactory ReprojectionFactory { get; }

        /// <summary>
        /// Method to reproject a <see cref="Geometry"/> from one <see cref="SpatialReference"/> to another.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="toSRID">The id of the target spatial reference system.</param>
        /// <returns></returns>
        public Geometry Reproject(Geometry geometry, int toSRID)
        {
            if (toSRID == 0)
                throw new ArgumentOutOfRangeException(nameof(toSRID));

            var to = SpatialReferenceFactory.GetSpatialReference(toSRID);
            return Reproject(geometry, to);
        }

        /// <summary>
        /// Method to reproject a <see cref="Geometry"/> from one <see cref="SpatialReference"/> to another.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="toSRID">The id of the target spatial reference system.</param>
        /// <returns></returns>
        public async Task<Geometry> ReprojectAsync(Geometry geometry, int toSRID)
        {
            if (toSRID == 0)
                throw new ArgumentOutOfRangeException(nameof(toSRID));

            var to = await SpatialReferenceFactory.GetSpatialReferenceAsync(toSRID);
            return await ReprojectAsync(geometry, to);
        }
        /// <summary>
        /// Method to reproject a <see cref="Geometry"/> from one <see cref="SpatialReference"/> to another.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="to">The target spatial reference system.</param>
        /// <returns>The reprojected geometry</returns>
        public Geometry Reproject(Geometry geometry, SpatialReference to)
        {
            if (!CheckArguments(geometry, nameof(geometry), to, to, out var e))
                throw e;

            if (geometry.SRID == to.Factory.SRID)
                return geometry;

            if (geometry.SRID == 0)
                throw new ArgumentException();

            var from = SpatialReferenceFactory.GetSpatialReference(geometry.SRID);
            var reprojection = ReprojectionFactory.Create(from, to);
            var res = reprojection.Apply(geometry);

            if (reprojection is IDisposable d)
                d.Dispose();

            return res;
        }

        /// <summary>
        /// Method to reproject a <see cref="Geometry"/> from one <see cref="SpatialReference"/> to another.
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <param name="to">The target spatial reference system.</param>
        /// <returns>The reprojected geometry</returns>
        public async Task<Geometry> ReprojectAsync(Geometry geometry, SpatialReference to)
        {
            if (!CheckArguments(geometry, nameof(geometry), to, to, out var e))
                throw e;

            if (geometry.SRID == to.Factory.SRID)
                return geometry;

            if (geometry.SRID == 0)
                throw new ArgumentException();

            var from = await SpatialReferenceFactory.GetSpatialReferenceAsync(geometry.SRID);

            var reprojection = ReprojectionFactory.Create(from, to);
            var res = await reprojection.ApplyAsync(geometry);

            if (reprojection is IDisposable d)
                d.Dispose();

            return res;
        }

        public virtual Envelope Reproject(Envelope envelope, SpatialReference from, SpatialReference to)
        {
            if (!CheckArguments(envelope, nameof(envelope), from, to, out var e))
                throw e;

            var reprojection = ReprojectionFactory.Create(from, to);
            var res = reprojection.Apply(envelope);
            if (reprojection is IDisposable d)
                d.Dispose();

            return res;
        }

        public virtual Coordinate Reproject(Coordinate coordinate, SpatialReference from, SpatialReference to)
        {
            if (!CheckArguments(coordinate, nameof(coordinate), from, to, out var e))
                throw e;

            var reprojection = ReprojectionFactory.Create(from, to);
            var res = reprojection.Apply(coordinate);
            if (reprojection is IDisposable d)
                d.Dispose();

            return res;
        }


        /// <summary>
        /// Utility function to check reprojection arguments for validity
        /// </summary>
        /// <param name="instance">The object that needs to be transformed</param>
        /// <param name="instanceName">The parameter name of the <paramref name="instance"/></param>
        /// <param name="from">The source spatial reference system</param>
        /// <param name="to">The target spatial reference system</param>
        /// <param name="e">An exception that indicates what is wrong with the arguments</param>
        /// <returns><value>true</value> if all checks have passed.</returns>
        protected bool CheckArguments(object instance, string instanceName, SpatialReference from, SpatialReference to, out ArgumentException e)
        {
            e = null;
            if (instance == null)
                e = new ArgumentNullException(instanceName);

            if (string.IsNullOrWhiteSpace(from.DefinitionKind))
                e = new ArgumentException(nameof(from));

            if (string.IsNullOrWhiteSpace(to.DefinitionKind))
                e = new ArgumentException(nameof(to));

            return e == null;
        }
    }
}
