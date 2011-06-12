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
using System.Globalization;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Base interface for all coordinate systems.
    /// </summary>
    /// <remarks>
    /// <para>A coordinate system is a mathematical space, where the elements of the space
    /// are called positions. Each position is described by a list of numbers. The length 
    /// of the list corresponds to the dimension of the coordinate system. So in a 2D 
    /// coordinate system each position is described by a list containing 2 numbers.</para>
    /// <para>However, in a coordinate system, not all lists of numbers correspond to a 
    /// position - some lists may be outside the domain of the coordinate system. For 
    /// example, in a 2D Lat/Lon coordinate system, the list (91,91) does not correspond
    /// to a position.</para>
    /// <para>Some coordinate systems also have a mapping from the mathematical space into 
    /// locations in the real world. So in a Lat/Lon coordinate system, the mathematical 
    /// position (lat, long) corresponds to a location on the surface of the Earth. This 
    /// mapping from the mathematical space into real-world locations is called a Datum.</para>
    /// </remarks>		
    public abstract class CoordinateSystem<TCoordinate> : Info, ICoordinateSystem<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly List<IAxisInfo> _axisInfo = new List<IAxisInfo>();
        private readonly IExtents<TCoordinate> _extents;

        /// <summary>
        /// Initializes a new instance of a coordinate system.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="authority">Authority name</param>
        /// <param name="authorityCode">Authority-specific identification code.</param>
        /// <param name="alias">Alias</param>
        /// <param name="abbreviation">Abbreviation</param>
        /// <param name="remarks">Provider-supplied remarks</param>
        protected internal CoordinateSystem(IExtents<TCoordinate> extents,
                                            String name, String authority, String authorityCode, String alias,
                                            String abbreviation, String remarks)
            : base(name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _extents = extents;
        }

        #region ICoordinateSystem Members

        /// <summary>
        /// Dimension of the coordinate system.
        /// </summary>
        public Int32 Dimension
        {
            get { return _axisInfo.Count; }
        }

        /// <summary>
        /// Gets the units for the dimension within coordinate system. 
        /// Each dimension in the coordinate system has corresponding units.
        /// </summary>
        public abstract IUnit GetUnits(Int32 dimension);

        internal IList<IAxisInfo> AxisInfo
        {
            get { return _axisInfo; }
            set
            {
                _axisInfo.Clear();
                _axisInfo.AddRange(value);
            }
        }

        /// <summary>
        /// Gets axis details for dimension within coordinate system.
        /// </summary>
        /// <param name="dimension">Dimension</param>
        /// <returns>Axis info</returns>
        public IAxisInfo GetAxis(Int32 dimension)
        {
            if (dimension >= _axisInfo.Count || dimension < 0)
            {
                throw new ArgumentException("AxisInfo not available for dimension " +
                                            dimension.ToString(CultureInfo.InvariantCulture));
            }

            return _axisInfo[dimension];
        }

        /// <summary>
        /// Gets default envelope of coordinate system.
        /// </summary>
        /// <remarks>
        /// Coordinate systems which are bounded should return the minimum bounding box of their domain. 
        /// Unbounded coordinate systems should return a box which is as large as is likely to be used. 
        /// For example, a (lon,lat) geographic coordinate system in degrees should return a box from 
        /// (-180,-90) to (180,90), and a geocentric coordinate system could return a box from (-r,-r,-r)
        /// to (+r,+r,+r) where r is the approximate radius of the Earth.
        /// </remarks>
        public IExtents<TCoordinate> DefaultEnvelope
        {
            get { return _extents; }
        }

        #endregion

        #region ICoordinateSystem Members

        IExtents ICoordinateSystem.DefaultEnvelope
        {
            get { return DefaultEnvelope; }
        }

        #endregion
    }
}