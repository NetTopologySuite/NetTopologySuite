// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems
{
	/// <summary>
	/// A 2D cartographic coordinate system.
	/// </summary>
	public class ProjectedCoordinateSystem : HorizontalCoordinateSystem,  IProjectedCoordinateSystem
	{
		/// <summary>
		/// Initializes a new instance of a projected coordinate system
		/// </summary>
		/// <param name="datum">Horizontal datum</param>
		/// <param name="geographicCoordinateSystem">Geographic coordinate system</param>
		/// <param name="linearUnit">Linear unit</param>
		/// <param name="projection">Projection</param>
		/// <param name="axisInfo">Axis info</param>
		/// <param name="name">Name</param>
		/// <param name="authority">Authority name</param>
		/// <param name="code">Authority-specific identification code.</param>
		/// <param name="alias">Alias</param>
		/// <param name="abbreviation">Abbreviation</param>
		/// <param name="remarks">Provider-supplied remarks</param>
		internal ProjectedCoordinateSystem(IHorizontalDatum datum, IGeographicCoordinateSystem geographicCoordinateSystem,
			ILinearUnit linearUnit, IProjection projection, List<AxisInfo> axisInfo,
			string name, string authority, long code, string alias,
			string remarks, string abbreviation)
			: base(datum, axisInfo, name, authority, code, alias, abbreviation, remarks)
		{
			_GeographicCoordinateSystem = geographicCoordinateSystem;
			_LinearUnit = linearUnit;
			_Projection = projection;
		}

		#region Predefined projected coordinate systems
/*
		/// <summary>
		/// Universal Transverse Mercator - WGS84
		/// </summary>
		/// <param name="Zone">UTM zone</param>
		/// <param name="ZoneIsNorth">true of Northern hemisphere, false if southern</param>
		/// <returns>UTM/WGS84 coordsys</returns>
		public static ProjectedCoordinateSystem WGS84_UTM(int Zone, bool ZoneIsNorth)
		{
			ParameterInfo pInfo = new ParameterInfo();
			pInfo.Add("latitude_of_origin", 0);
			pInfo.Add("central_meridian", Zone * 6 - 183);
			pInfo.Add("scale_factor", 0.9996);
			pInfo.Add("false_easting", 500000);
			pInfo.Add("false_northing", ZoneIsNorth ? 0 : 10000000);
			Projection proj = new Projection(String.Empty,String.Empty,pInfo,AngularUnit.Degrees,
				SharpMap.SpatialReference.LinearUnit.Metre,Ellipsoid.WGS84,
				"Transverse_Mercator", "EPSG", 32600 + Zone + (ZoneIsNorth ? 0 : 100), String.Empty, String.Empty, String.Empty);

			return new ProjectedCoordinateSystem("Large and medium scale topographic mapping and engineering survey.",
				SharpMap.SpatialReference.GeographicCoordinateSystem.WGS84,
				SharpMap.SpatialReference.LinearUnit.Metre, proj, pInfo,
				"WGS 84 / UTM zone " + Zone.ToString() + (ZoneIsNorth ? "N" : "S"), "EPSG", 32600 + Zone + (ZoneIsNorth ? 0 : 100),
				String.Empty,String.Empty,string.Empty);
			
		}*/

		#endregion

		#region IProjectedCoordinateSystem Members

		private IGeographicCoordinateSystem _GeographicCoordinateSystem;

		/// <summary>
		/// Gets or sets the GeographicCoordinateSystem.
		/// </summary>
		public IGeographicCoordinateSystem GeographicCoordinateSystem
		{
			get { return _GeographicCoordinateSystem; }
			set { _GeographicCoordinateSystem = value; }
		}

		private ILinearUnit _LinearUnit;

		/// <summary>
		/// Gets or sets the <see cref="LinearUnit">LinearUnits</see>. The linear unit must be the same as the <see cref="CoordinateSystem"/> units.
		/// </summary>
		public ILinearUnit LinearUnit
		{
			get { return _LinearUnit; }
			set { _LinearUnit = value; }
		}

		/// <summary>
		/// Gets units for dimension within coordinate system. Each dimension in 
		/// the coordinate system has corresponding units.
		/// </summary>
		/// <param name="dimension">Dimension</param>
		/// <returns>Unit</returns>
		public override IUnit GetUnits(int dimension)
		{
			return _LinearUnit;
		}

		private IProjection _Projection;

		/// <summary>
		/// Gets or sets the projection
		/// </summary>
		public IProjection Projection
		{
			get { return _Projection; }
			set { _Projection = value; }
		}

		/// <summary>
		/// Returns the Well-known text for this object
		/// as defined in the simple features specification.
		/// </summary>
		public override string WKT
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("PROJCS[\"{0}\", {1}, {2}",Name, GeographicCoordinateSystem.WKT, Projection.WKT);
				for(int i=0;i<Projection.NumParameters;i++)
					sb.AppendFormat(Global.GetNfi(), ", {0}", Projection.GetParameter(i).WKT);
				sb.AppendFormat(", {0}", LinearUnit.WKT);
				//Skip axis info if they contain default values
				if (AxisInfo.Count != 2 ||
					AxisInfo[0].Name != "X" || AxisInfo[0].Orientation != AxisOrientationEnum.East ||
					AxisInfo[1].Name != "Y" || AxisInfo[1].Orientation != AxisOrientationEnum.North)
					for (int i = 0; i < AxisInfo.Count; i++)
						sb.AppendFormat(", {0}", GetAxis(i).WKT);
				if (!String.IsNullOrEmpty(Authority) && AuthorityCode > 0)
					sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);				
				sb.Append("]");
				return sb.ToString();
			}
		}

		/// <summary>
		/// Gets an XML representation of this object.
		/// </summary>
		public override string XML
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat(Global.GetNfi(),
					"<CS_CoordinateSystem Dimension=\"{0}\"><CS_ProjectedCoordinateSystem>{1}",
					this.Dimension, InfoXml);
				foreach (AxisInfo ai in this.AxisInfo)
					sb.Append(ai.XML);

				sb.AppendFormat("{0}{1}{2}</CS_ProjectedCoordinateSystem></CS_CoordinateSystem>",
					GeographicCoordinateSystem.XML, LinearUnit.XML, Projection.XML);
				return sb.ToString();
			}
		}

		/// <summary>
		/// Checks whether the values of this instance is equal to the values of another instance.
		/// Only parameters used for coordinate system are used for comparison.
		/// Name, abbreviation, authority, alias and remarks are ignored in the comparison.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>True if equal</returns>
		public override bool EqualParams(object obj)
		{
			if (!(obj is ProjectedCoordinateSystem))
				return false;
			ProjectedCoordinateSystem pcs = obj as ProjectedCoordinateSystem;
			if(pcs.Dimension != this.Dimension)
				return false;
			for (int i = 0; i < pcs.Dimension; i++)
			{
				if(pcs.GetAxis(i).Orientation != this.GetAxis(i).Orientation)
					return false;
				if (!pcs.GetUnits(i).EqualParams(this.GetUnits(i)))
					return false;
			}

			return	pcs.GeographicCoordinateSystem.EqualParams(this.GeographicCoordinateSystem) && 
					pcs.HorizontalDatum.EqualParams(this.HorizontalDatum) &&
					pcs.LinearUnit.EqualParams(this.LinearUnit) &&
					pcs.Projection.EqualParams(this.Projection);
		}

		#endregion
	}
}
