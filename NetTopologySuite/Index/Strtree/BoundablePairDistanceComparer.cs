﻿/*
 * Copyright (c) 2017 Jia Yu.
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * and Eclipse Distribution License v. 1.0 which accompanies this distribution.
 * The Eclipse Public License is available at http://www.eclipse.org/legal/epl-v10.html
 * and the Eclipse Distribution License is available at
 *
 * http://www.eclipse.org/org/documents/edl-v10.php.
 */
using System.Collections.Generic;
namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// The Class BoundablePairDistanceComparator. It implements .Net <see cref="IComparer{T}"/> and is used
    /// as a parameter to sort the BoundablePair list.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [System.Serializable]
#endif
    internal class BoundablePairDistanceComparer<TItem> : IComparer<BoundablePair<TItem>>
    {
        /// <summary>The normal order</summary>
        readonly bool _normalOrder;
        /// <summary>
        /// Instantiates a new boundable pair distance comparator.
        /// </summary>
        /// <param name="normalOrder">
        /// A value of <c>true</c> means puts the least record at the head of this queue.
        /// This is the natural order.PriorityQueue peek() will get the least element.Vice versa.
        /// </param>
        public BoundablePairDistanceComparer(bool normalOrder)
        {
            this._normalOrder = normalOrder;
        }
        /// <inheritdoc cref="IComparer{T}.Compare"/>
        public int Compare(BoundablePair<TItem> p1, BoundablePair<TItem> p2)
        {
            var distance1 = p1.Distance;
            var distance2 = p2.Distance;
            if (_normalOrder)
            {
                if (distance1 > distance2)
                {
                    return 1;
                }
                if (distance1 == distance2)
                {
                    return 0;
                }
                return -1;
            }
            else
            {
                if (distance1 > distance2)
                {
                    return -1;
                }
                else if (distance1 == distance2)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}
