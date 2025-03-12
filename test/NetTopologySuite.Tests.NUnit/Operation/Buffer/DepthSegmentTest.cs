/*
 * The JTS Topology Suite is a collection of Java classes that
 * implement the fundamental operations required to validate a given
 * geo-spatial data set to a known topological specification.
 *
 * Copyright (C) 2001 Vivid Solutions
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * For more information, contact:
 *
 *     Vivid Solutions
 *     Suite #1A
 *     2328 Government Street
 *     Victoria BC  V8T 5G5
 *     Canada
 *
 *     (250)385-6040
 *     www.vividsolutions.com
 */

using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{

    /**
     * @version 1.7
     */

    [TestFixture]
    public class DepthSegmentTest
    {
        [Test]
        public void TestCompareTipToTail()
        {
            var ds0 = DepthSeg(0.7, 0.2, 1.4, 0.9);
            var ds1 = DepthSeg(0.7, 0.2, 0.3, 1.1);
            CheckCompare(ds0, ds1, 1);
        }

        [Test]
        public void TestCompare2()
        {
            var ds0 = DepthSeg(0.1, 1.9, 0.5, 1.0);
            var ds1 = DepthSeg(1.0, 0.9, 1.9, 1.4);
            CheckCompare(ds0, ds1, -1);
        }

        [Test]
        public void TestCompareVertical()
        {
            var ds0 = DepthSeg(1, 1, 1, 2);
            var ds1 = DepthSeg(1, 0, 1, 1);
            CheckCompare(ds0, ds1, 1);
        }

        [Test]
        public void TestCompareOrientBug()
        {
            var ds0 = DepthSeg(146.268, -8.42361, 146.263, -8.3875);
            var ds1 = DepthSeg(146.269, -8.42889, 146.268, -8.42361);
            CheckCompare(ds0, ds1, -1);
        }

        [Test]
        public void TestCompareEqual()
        {
            var ds0 = DepthSeg(1, 1, 2, 2);
            CheckCompare(ds0, ds0, 0);
        }

        private void CheckCompare(
           SubgraphDepthLocater.DepthSegment ds0,
           SubgraphDepthLocater.DepthSegment ds1,
           int expectedComp)
        {
            Assert.That(ds0.IsUpward);
            Assert.That(ds1.IsUpward);

            // check compareTo contract - should never have ds1 < ds2 && ds2 < ds1
            int comp0 = ds0.CompareTo(ds1);
            int comp1 = ds1.CompareTo(ds0);
            Assert.That(comp0, Is.EqualTo(expectedComp));
            Assert.That(comp0, Is.EqualTo(-comp1));
        }


        private SubgraphDepthLocater.DepthSegment DepthSeg(double x0, double y0, double x1, double y1)
        {
            var seg = new LineSegment(x0, y0, x1, y1);
            if (seg.P0.Y > seg.P1.Y)
                seg.Reverse();
            return new SubgraphDepthLocater.DepthSegment(seg, 0);
        }

    }
}
