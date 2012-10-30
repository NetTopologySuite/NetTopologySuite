using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Utility
{
    public class FunctionsUtil
    {

        public static readonly Envelope DefaultEnvelope = new Envelope(0, 100, 0, 100);

        public static Envelope getEnvelopeOrDefault(IGeometry g)
        {
            if (g == null) return DefaultEnvelope;
            return g.EnvelopeInternal;
        }

        private static readonly IGeometryFactory Factory = new GeometryFactory();

        public static IGeometryFactory getFactoryOrDefault(IGeometry g)
        {
            if (g == null) return Factory; /*NTSTestBuilder.getGeometryFactory()*/;
            return g.Factory;
        }

        /*
  public static void showIndicator(Geometry geom)
  {
    GeometryEditPanel panel = JTSTestBuilderFrame
    .getInstance().getTestCasePanel()
    .getGeometryEditPanel();
    Graphics2D gr = (Graphics2D) panel.getGraphics();
    GeometryRenderer.paint(geom, panel.getViewport(), gr, 
        AppColors.INDICATOR_LINE_COLOR, 
        AppColors.INDICATOR_FILL_COLOR);
  }
     */
        public static IGeometry BuildGeometry(List<IGeometry> geoms, IGeometry parentGeom)
        {
            if (geoms.Count <= 0)
                return null;
            if (geoms.Count == 1)
                return geoms[0];
            // if parent was a GC, ensure returning a GC
            if (parentGeom.OgcGeometryType == OgcGeometryType.GeometryCollection)
                return parentGeom.Factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geoms));
            // otherwise return MultiGeom
            return parentGeom.Factory.BuildGeometry(geoms);
        }
    }
}