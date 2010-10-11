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
using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    ///<summary>
    ///An intersector for the red-blue intersection problem.
    ///In this class of line arrangement problem,
    ///two disjoint sets of linestrings are provided.
    ///It is assumed that within
    ///each set, no two linestrings intersect except possibly at their endpoints.
    ///Implementations can take advantage of this fact to optimize processing.
    ///
    ///@author Martin Davis
    ///@version 1.10
    ///</summary>
    public abstract class SegmentSetMutualIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        protected ISegmentIntersector<TCoordinate> _segInt;

        ///<summary>
        ///Sets the <see cref="ISegmentIntersector{TCoordinate}"/> to use with this intersector.
        ///The SegmentIntersector will either rocord or add intersection nodes
        ///for the input segment strings.
        ///<summary>
        ///<param name="segInt">the segment intersector to use</param>
        public void SetSegmentIntersector(ISegmentIntersector<TCoordinate> segInt)
        {
            _segInt = segInt;
        }

        /// <summary>
        /// </summary>
        ///<param name="segStrings">a collection of <see cref="ISegmentString{TCoordinate}"/>s to node</param>
        public abstract void SetBaseSegments(IEnumerable<ISegmentString<TCoordinate>> segStrings);

        ///<summary>
        ///Computes the intersections for two collections of <see cref="ISegmentString{TCoordinate}"/>s.
        ///<summary>
        ///<param name="segStrings">a collection of <see cref="ISegmentString{TCoordinate}"/>s to node</param>
        public abstract void Process(List<ISegmentString<TCoordinate>> segStrings);
    }
}