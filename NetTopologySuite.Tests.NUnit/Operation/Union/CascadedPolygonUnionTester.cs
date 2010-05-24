using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Match;
using GisSharpBlog.NetTopologySuite.Operation.Union;
using NetTopologySuite.Coordinates.Simple;

namespace NetTopologySuite.Tests.NUnit.Operation
{
    /**
     * Compares the results of CascadedPolygonUnion to Geometry.union()
     * using shape similarity measures.
     * 
     * @author mbdavis
     *
     */
    public class CascadedPolygonUnionTester
    {
        public const double MIN_SIMILARITY_MEAURE = 0.999999;

        public Boolean Test(IList<IGeometry<Coordinate>> geoms, double minimumMeasure)
        {

            Stopwatch sw = new Stopwatch();

            Console.Write("Computing Iterated union");
            IGeometry<Coordinate> union1 = UnionIterated(geoms);
            sw.Start();
            union1 = UnionIterated(geoms);
            sw.Stop();
            Console.WriteLine(string.Format(" ... {0}ms", sw.ElapsedMilliseconds));
            sw.Reset();
            Console.Write("Computing Cascaded union");
            IGeometry<Coordinate> union2 = UnionCascaded(geoms);
            sw.Start();
            union2 = UnionCascaded(geoms);
            sw.Stop();
            Console.WriteLine(string.Format(" ... {0}ms", sw.ElapsedMilliseconds));

            Console.WriteLine("Testing similarity with min measure = " + minimumMeasure);

            double areaMeasure = (new AreaSimilarityMeasure<Coordinate>()).Measure(union1, union2);
            double hausMeasure = (new HausdorffSimilarityMeasure<Coordinate>()).Measure(union1, union2);
            double overallMeasure = SimilarityMeasureCombiner.Combine(areaMeasure, hausMeasure);

            Console.WriteLine(
                    "Area measure = " + areaMeasure
                    + "   Hausdorff measure = " + hausMeasure
                    + "    Overall = " + overallMeasure);

            return overallMeasure > minimumMeasure;
        }

        /*
        private void OLDdoTest(String filename, double distanceTolerance) 
        throws IOException, ParseException
        {
          WKTFileReader fileRdr = new WKTFileReader(filename, wktRdr);
          List geoms = fileRdr.read();
    
          System.out.println("Computing Iterated union");
          Geometry union1 = UnionIterated(geoms);
          System.out.println("Computing Cascaded union");
          Geometry union2 = UnionCascaded(geoms);
    
          System.out.println("Testing similarity with tolerance = " + distanceTolerance);
          boolean isSameWithinTolerance =  SimilarityValidator.isSimilar(union1, union2, distanceTolerance);
    
 	
          assertTrue(isSameWithinTolerance);
        }
      */

        public IGeometry<Coordinate> UnionIterated(IList<IGeometry<Coordinate>> geoms)
        {
            IGeometry<Coordinate> unionAll = null;
            int count = 0;
            foreach (IPolygon<Coordinate> geom in geoms)
            {

                if (unionAll == null)
                {
                    unionAll = (IGeometry<Coordinate>)geom.Clone();
                }
                else
                {
                    unionAll = unionAll.Union(geom);
                }

                count++;
                if (count % 100 == 0)
                {
                    Console.WriteLine(".");
                    //        System.out.println("Adding geom #" + count);
                }
            }
            return unionAll;
        }

        public IGeometry<Coordinate> UnionCascaded(IList<IGeometry<Coordinate>> geoms)
        {
            return CascadedPolygonUnion<Coordinate>.Union(geoms);
        }

    }
}