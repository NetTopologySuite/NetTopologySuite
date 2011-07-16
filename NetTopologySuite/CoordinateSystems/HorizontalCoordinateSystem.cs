// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Proj.Net.
// Proj.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Proj.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Proj.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// A 2D coordinate system suitable for positions on the Earth's surface.
    /// </summary>
    public abstract class HorizontalCoordinateSystem<TCoordinate> : CoordinateSystem<TCoordinate>,
                                                                    IHorizontalCoordinateSystem<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly IHorizontalDatum _horizontalDatum;

        /// <summary>
        /// Creates an instance of HorizontalCoordinateSystem
        /// </summary>
        /// <param name="datum">Horizontal datum</param>
        /// <param name="axisInfo">Axis information</param>
        /// <param name="name">Name</param>
        /// <param name="authority">Authority name</param>
        /// <param name="authorityCode">Authority-specific identification code.</param>
        /// <param name="alias">Alias</param>
        /// <param name="abbreviation">Abbreviation</param>
        /// <param name="remarks">Provider-supplied remarks</param>
        protected internal HorizontalCoordinateSystem(IExtents<TCoordinate> extents,
                                                      IHorizontalDatum datum, IEnumerable<IAxisInfo> axisInfo,
                                                      String name, String authority, String authorityCode, String alias,
                                                      String remarks, String abbreviation)
            : base(extents, name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _horizontalDatum = datum;

            AxisInfo = new List<IAxisInfo>(axisInfo);

            if (AxisInfo.Count != 2)
            {
                throw new ArgumentException("Axis info should contain two axes " +
                                            "for horizontal coordinate systems");
            }
        }

        #region IHorizontalCoordinateSystem Members

        /// <summary>
        /// Gets or sets the HorizontalDatum.
        /// </summary>
        public IHorizontalDatum HorizontalDatum
        {
            get { return _horizontalDatum; }
        }

        #endregion
    }
}