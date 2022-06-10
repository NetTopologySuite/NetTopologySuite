using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Samples.Technique
{
    /// <summary>
    /// A class that holds output parameters for the SplitPolygonAtDateline routine.
    /// </summary>
    public class SplitDatelineOuput
    {
        /// <summary>
        /// The output geometry type.
        /// </summary>
        public OgcGeometryType OutType { get; set; } = OgcGeometryType.Polygon;

        /// <summary>
        /// The value used to trim the resulting geometry parts so they do not exactly touch the dateline.A positive value will trim only the
        /// parts that are in the Eastern hemisphere and a negative value will trim only the parts that are in the Western hemisphere.This is
        /// useful if the resultant geometries are to be used as multiple parts of a single feature.Double.NaN for no trim gap.If the resulting
        /// geometry will be inserted into a feature class then consider the tolerance for that feature class. "The default value for the x,y tolerance
        /// is 10 times the default x,y resolution, and this is recommended for most cases" and "If coordinates are in latitude-longitude, the default
        /// x,y resolution is 0.000000001 degrees". Thus a default tolerance = 10 * res = 10 * 0.000000001 degrees = 0.00000001 degrees.
        /// https://desktop.arcgis.com/en/arcmap/latest/manage-data/geodatabases/feature-class-basics.htm
        /// </summary>
        public double OutTrimGap { get; set; } = double.NaN;

        /// <summary>
        /// The value used to densify the resulting geometry. Double.NaN for no densification.
        /// </summary>
        public double OutDensifyResolution { get; set; } = double.NaN;

        /// <summary>
        /// The method to use to fix invalid geometry. The defaukt is to not attempt to fix invalid geometry.
        /// </summary>
        public InvalidGeomFixMethod InvGeomFixMethod { get; set; } = InvalidGeomFixMethod.FixNone;
    }
}
