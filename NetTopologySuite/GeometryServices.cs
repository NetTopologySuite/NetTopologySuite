using System;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite
{
    public sealed class GeometryServices : IGeometryServices
    {
        private static IGeometryServices _instance;
        
        private static readonly object LockObject1 = new object();
        private static readonly object LockObject2 = new object();

        public static IGeometryServices Instance
        {
            get
            {
                lock (LockObject1)
                {
                    if (_instance != null)
                        return _instance;

                    lock (LockObject2)
                    {
                        _instance = new GeometryServices();
                    }
                    return _instance;
                }
            }
        }




        public GeometryServices()
        {
            DefaultCoordinateSequenceFactory = CoordinateArraySequenceFactory.Instance;
            DefaultPrecisionModel = new PrecisionModel(PrecisionModels.Floating);
        }

        #region Implementation of IGeometryServices

        /// <summary>
        /// Gets the default spatial reference id
        /// </summary>
        public int DefaultSRID { get; private set; }

        /// <summary>
        /// Gets or sets the coordiate sequence factory to use
        /// </summary>
        public ICoordinateSequenceFactory DefaultCoordinateSequenceFactory { get; private set; }

        /// <summary>
        /// Gets or sets the default precision model
        /// </summary>
        public IPrecisionModel DefaultPrecisionModel { get; private set; }

        /// <summary>
        /// Creates a precision model based on given precision model type
        /// </summary>
        /// <returns>The precision model type</returns>
        public IPrecisionModel CreatePrecisionModel(PrecisionModels modelType)
        {
            return new PrecisionModel(modelType);
        }

        /// <summary>
        /// Creates a precision model based on given precision model.
        /// </summary>
        /// <returns>The precision model</returns>
        public IPrecisionModel CreatePrecisionModel(IPrecisionModel precisionModel)
        {
            if (precisionModel is PrecisionModel)
                return new PrecisionModel((PrecisionModel)precisionModel);
            
            if (!precisionModel.IsFloating)
                return new PrecisionModel(precisionModel.Scale);
            return new PrecisionModel(precisionModel.PrecisionModelType);
        }

        /// <summary>
        /// Creates a precision model based on the given scale factor.
        /// </summary>
        /// <param name="scale">The scale factor</param>
        /// <returns>The precision model.</returns>
        public IPrecisionModel CreatePrecisionModel(double scale)
        {
            return new PrecisionModel(scale);
        }

        public IGeometryFactory CreateGeometryFactory()
        {
            return CreateGeometryFactory(-1);
        }

        public IGeometryFactory CreateGeometryFactory(int srid)
        {
            return new GeometryFactory(DefaultPrecisionModel, srid, DefaultCoordinateSequenceFactory);
        }

        public void ReadConfiguration()
        {
            lock (LockObject1)
            {
                
            }
        }

        public void WriteConfiguration()
        {
            lock (LockObject2)
            {
                
            }
        }

        public IGeometryFactory CreateGeometryFactory(ICoordinateSequenceFactory coordinateSequenceFactory)
        {
            if (coordinateSequenceFactory == null)
                throw new ArgumentNullException("coordinateSequenceFactory");
            return new GeometryFactory(coordinateSequenceFactory);
        }

        public IGeometryFactory CreateGeometryFactory(IPrecisionModel precisionModel)
        {
            return new GeometryFactory(precisionModel);
        }

        public IGeometryFactory CreateGeometryFactory(IPrecisionModel precisionModel, int srid)
        {
            if (precisionModel == null)
                throw new ArgumentNullException("precisionModel");
            return new GeometryFactory(precisionModel, srid);
        }

        public IGeometryFactory CreateGeometryFactory(IPrecisionModel precisionModel, int srid, ICoordinateSequenceFactory coordinateSequenceFactory)
        {
            if (precisionModel == null)
                throw new ArgumentNullException("precisionModel");
            if (coordinateSequenceFactory == null)
                throw new ArgumentNullException("coordinateSequenceFactory");

            return new GeometryFactory(precisionModel, srid, coordinateSequenceFactory);
        }

        #endregion

    //    #region Implementation of ISerializable

    //    public GeometryServices(SerializationInfo info, StreamingContext context)
    //    {
    //        var pmType = (PrecisionModels) info.GetInt32("type");
    //        if (pmType != PrecisionModels.Fixed)
    //        {
    //            DefaultPrecisionModel = new PrecisionModel(pmType);
    //        }
    //        else
    //        {
    //            var scale = info.GetDouble("scale");
    //            DefaultPrecisionModel = new PrecisionModel(scale);
    //        }
    //        if (info.GetBoolean("csfPredefined"))
    //        {
    //            var csfAssembly = info.GetString("csfAssembly");
    //            var csfName = info.GetString("csfName");
    //            DefaultCoordinateSequenceFactory = Find(csfAssembly, csfName);
    //        }
    //        else
    //        {
    //            DefaultCoordinateSequenceFactory = (ICoordinateSequenceFactory)
    //                                        info.GetValue("csf", typeof (ICoordinateSequenceFactory));
    //        }
    //        /*_instance = this;*/
    //    }

    //    private ICoordinateSequenceFactory Find(string csfAssembly, string csfName)
    //    {
    //        switch (csfName)
    //        {
    //            case "NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory":
    //                return CoordinateArraySequenceFactory.Instance;
    //            case "NetTopologySuite.Geometries.Implementation.PackedCoordinateSequenceFactory.Double":
    //                return PackedCoordinateSequenceFactory.DoubleFactory;
    //            case "NetTopologySuite.Geometries.Implementation.PackedCoordinateSequenceFactory.Float":
    //                return PackedCoordinateSequenceFactory.FloatFactory;
    //            case "NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequenceFactory":
    //                return DotSpatialAffineCoordinateSequenceFactory.Instance;
    //        }
    //        Assert.ShouldNeverReachHere("CoordinateSequenceFactory instance not found!");
    //        return null;
    //    }

    //    public void GetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //        info.AddValue("type", (int)DefaultPrecisionModel.PrecisionModelType);
    //        if (DefaultPrecisionModel.PrecisionModelType == PrecisionModels.Fixed)
    //            info.AddValue("scale", (int)DefaultPrecisionModel.Scale);

    //        if (DefaultCoordinateSequenceFactory is CoordinateArraySequenceFactory ||
    //            DefaultCoordinateSequenceFactory is PackedCoordinateSequenceFactory ||
    //            DefaultCoordinateSequenceFactory is DotSpatialAffineCoordinateSequenceFactory)
    //        {
    //            var type = DefaultCoordinateSequenceFactory.GetType();
    //            var name = type.FullName;
    //            if (DefaultCoordinateSequenceFactory is PackedCoordinateSequenceFactory)
    //            {
    //                if (DefaultCoordinateSequenceFactory == PackedCoordinateSequenceFactory.DoubleFactory)
    //                    name += ".Double";
    //                else
    //                    name += ".Float";
    //            }
    //            info.AddValue("csfPredefined", true);
    //            info.AddValue("csfAssembly", type.Assembly.FullName);
    //            info.AddValue("csfName", name);
    //        }
    //        else
    //        {
    //            info.AddValue("csfPredefined", false);
    //            if (!(DefaultCoordinateSequenceFactory is ISerializable))
    //                throw new InvalidOperationException(string.Format("Cannot serialize '{0}'",
    //                                                                  DefaultCoordinateSequenceFactory.GetType().FullName));
    //            info.AddValue("csf", DefaultCoordinateSequenceFactory, typeof(ICoordinateSequenceFactory));
    //        }
    //    }

    //    #endregion
    }
}