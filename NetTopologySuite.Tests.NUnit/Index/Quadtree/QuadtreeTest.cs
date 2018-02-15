/*
 * Copyright (c) 2016 Martin Davis.
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * and Eclipse Distribution License v. 1.0 which accompanies this distribution.
 * The Eclipse Public License is available at http://www.eclipse.org/legal/epl-v10.html
 * and the Eclipse Distribution License is available at
 *
 * http://www.eclipse.org/org/documents/edl-v10.php.
 */

using GeoAPI.Geometries;
using NetTopologySuite.Index.Quadtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Quadtree
{
    public class QuadtreeTest
    {
        [Test]
        public void TestNullQuery()
        {
            var qt = new Quadtree<string>();
            var result1 = qt.Query(null);
            Assert.That(result1.Count, Is.EqualTo(0));

            qt.Insert(new Envelope(0, 10, 0, 10), "some data");
            var result2 = qt.Query(null);
            Assert.That(result2.Count, Is.EqualTo(0));
        }
    }
}