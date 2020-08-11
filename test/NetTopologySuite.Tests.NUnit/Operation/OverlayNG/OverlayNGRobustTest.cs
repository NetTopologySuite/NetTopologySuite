using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    /**
     * Tests robustness cases for OverlayNG.
     * 
     * @author Martin Davis
     *
     */
    public class OverlayNGRobustTest : GeometryTestCase
    {

        /**
         * Tests a case where ring clipping causes an incorrect result.
         * <p>
         * The incorrect result occurs because:
         * <ol>
         * <li>Ring Clipping causes a clipped A line segment to move slightly.
         * <li>This causes the clipped A and B edges to become disjoint
         * (whereas in the original geometry they intersected).  
         * <li>Both edge rings are thus determined to be disconnected during overlay labeling.
         * <li>For the overlay labeling for the disconnected edge in geometry B,
         * the chosen edge coordinate has its location computed as inside the original A polygon.
         * This is because the chosen coordinate happens to be the one that the 
         * clipped edge crossed over.
         * <li>This causes the (clipped) B edge ring to be labelled as Interior to the A polygon. 
         * <li>The B edge ring thus is computed as being in the intersection, 
         * and the entire ring is output, producing a much larger polygon than is correct.
         * </ol>
         * The test check here is a heuristic that detects the presence of a large
         * polygon in the output.
         * <p>
         * There are several possible fixes:
         * <ul>
         * <li>Improve clipping to avoid clipping line segments which may intersect
         * other geometry (by computing a large enough clipping envelope)</li>
         * <li>Improve choosing a point for disconnected edge location; 
         * i.e. by finding one that is far from the other geometry edges.
         * However, this still creates a result which may not reflect the 
         * actual input topology.
         * </li>
         * </ul>
         * 
         */
        [Test, Ignore("Known to fail")]
        public void TestPolygonsWithClippingPerturbationIntersection()
        {
            var a = Read(
                "POLYGON ((4373089.33 5521847.89, 4373092.24 5521851.6, 4373118.52 5521880.22, 4373137.58 5521896.63, 4373153.33 5521906.43, 4373270.51 5521735.67, 4373202.5 5521678.73, 4373100.1 5521827.97, 4373089.33 5521847.89))");
            var b = Read(
                "POLYGON ((4373225.587574724 5521801.132991467, 4373209.219497436 5521824.985294571, 4373355.5585138 5521943.53124194, 4373412.83157427 5521860.49206234, 4373412.577392304 5521858.140878815, 4373412.290476093 5521855.48690386, 4373374.245799139 5521822.532711867, 4373271.028377312 5521736.104060946, 4373225.587574724 5521801.132991467))");
            var actual = Intersection(a, b);
            bool isCorrect = actual.Area < 1;
            Assert.IsTrue(isCorrect, "Area of intersection result area is too large");
        }

        static Geometry Intersection(Geometry a, Geometry b)
        {
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Intersection);
        }
    }
}
