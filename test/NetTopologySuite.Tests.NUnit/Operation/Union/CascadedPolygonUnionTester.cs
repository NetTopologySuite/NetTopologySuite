using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm.Match;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Union
{
    /// <summary>
    /// Compares the results of CascadedPolygonUnion to Geometry.union()
    /// using shape similarity measures.
    /// </summary>
    /// <author>mbdavis</author>
    public class CascadedPolygonUnionTester
    {
        public static double MinSimilarityMeaure = 0.999999;

        public bool Test(IList<Geometry> geoms, double minimumMeasure)
        {
            //System.TestContext.WriteLine("Computing Iterated union ");
            var union1 = UnionIterated(geoms);
            //System.TestContext.WriteLine("Computing Cascaded union");
            var union2 = UnionCascaded(geoms);

            //TestContext.WriteLine("Testing similarity with min measure = " + minimumMeasure);

            double areaMeasure = (new AreaSimilarityMeasure()).Measure(union1, union2);
            double hausMeasure = (new HausdorffSimilarityMeasure()).Measure(union1, union2);
            double overallMeasure = SimilarityMeasureCombiner.Combine(areaMeasure, hausMeasure);

            //TestContext.WriteLine(
            //        "Area measure = " + areaMeasure
            //        + "   Hausdorff measure = " + hausMeasure
            //        + "    Overall = " + overallMeasure);

            return overallMeasure > minimumMeasure;
        }

        /*
        private void OLDdoTest(String filename, double distanceTolerance)
        throws IOException, ParseException
        {
          WKTFileReader fileRdr = new WKTFileReader(filename, wktRdr);
          List geoms = fileRdr.read();

          System.out.println("Computing Iterated union");
          Geometry union1 = unionIterated(geoms);
          System.out.println("Computing Cascaded union");
          Geometry union2 = unionCascaded(geoms);

          System.out.println("Testing similarity with tolerance = " + distanceTolerance);
          bool isSameWithinTolerance =  SimilarityValidator.isSimilar(union1, union2, distanceTolerance);

          Assert.IsTrue(isSameWithinTolerance);
        }
      */
        public Geometry UnionIterated(IList<Geometry> geoms)
        {
            Geometry unionAll = null;
            int count = 0;
            foreach (var geom in geoms)
            {
                if (unionAll == null)
                {
                    unionAll = (Geometry)geom.Copy();
                }
                else
                {
                    unionAll = unionAll.Union(geom);
                }

                count++;
                if (count % 100 == 0)
                {
                    TestContext.Write(".");
                    //        System.out.println("Adding geom #" + count);
                }
            }

            TestContext.Write("\nDone");

            return unionAll;
        }

        public Geometry UnionCascaded(IList<Geometry> geoms)
        {
            return CascadedPolygonUnion.Union(geoms);
        }
    }
}
