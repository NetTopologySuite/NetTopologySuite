// Copyright 2005 - 2009 - Morten Nielsen (www.sharpgis.net)
//
// This file is part of ProjNet.
// ProjNet is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// ProjNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with ProjNet; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace ProjNet.CoordinateSystems
{
	/// <summary>
	/// Creates spatial reference objects using codes.
	/// </summary>
	/// <remarks>
	///  The codes are maintained by an external authority. A commonly used authority is EPSG, which is also used in the GeoTIFF standard and in ProjNet.
	/// </remarks>
	public interface ICoordinateSystemAuthorityFactory
	{
		/// <summary>
		/// Returns the authority name for this factory (e.g., "EPSG" or "POSC").
		/// </summary>
		string Authority { get; }
		/// <summary>
		/// Returns a projected coordinate system object corresponding to the given code.
		/// </summary>
		/// <param name="code">The identification code.</param>
		/// <returns>The projected coordinate system object with the given code.</returns>
		IProjectedCoordinateSystem CreateProjectedCoordinateSystem(long code);
		/// <summary>
		/// Returns a geographic coordinate system object corresponding to the given code.
		/// </summary>
		/// <param name="code">The identification code.</param>
		/// <returns>The geographic coordinate system object with the given code.</returns>
		IGeographicCoordinateSystem CreateGeographicCoordinateSystem(long code);
		/// <summary>
		/// Returns a horizontal datum object corresponding to the given code.
		/// </summary>
		/// <param name="code">The identification code.</param>
		/// <returns>The horizontal datum object with the given code.</returns>
		IHorizontalDatum CreateHorizontalDatum(long code);
		/// <summary>
		/// Returns an ellipsoid object corresponding to the given code.
		/// </summary>
		/// <param name="code">The identification code.</param>
		/// <returns>The ellipsoid object with the given code.</returns>
		IEllipsoid CreateEllipsoid(long code);
		/// <summary>
		/// Returns a prime meridian object corresponding to the given code.
		/// </summary>
		/// <param name="code">The identification code.</param>
		/// <returns>The prime meridian object with the given code.</returns>
		IPrimeMeridian CreatePrimeMeridian(long code);
		/// <summary>
		/// Returns a linear unit object corresponding to the given code.
		/// </summary>
		/// <param name="code">The identification code.</param>
		/// <returns>The linear unit object with the given code.</returns>
		ILinearUnit CreateLinearUnit(long code);
		/// <summary>
		/// Returns an <see cref="IAngularUnit">angular unit</see> object corresponding to the given code.
		/// </summary>
		/// <param name="code">The identification code.</param>
		/// <returns>The angular unit object for the given code.</returns>
		IAngularUnit CreateAngularUnit(long code);
		/// <summary>
		/// Creates a <see cref="IVerticalDatum"/> from a code.
		/// </summary>
		/// <param name="code">Authority code</param>
		/// <returns>Vertical datum for the given code</returns>
		IVerticalDatum CreateVerticalDatum(long code);
		/// <summary>
		/// Create a <see cref="IVerticalCoordinateSystem">vertical coordinate system</see> from a code.
		/// </summary>
		/// <param name="code">Authority code</param>
		/// <returns></returns>
		IVerticalCoordinateSystem CreateVerticalCoordinateSystem(long code);
		/// <summary>
		/// Creates a 3D coordinate system from a code.
		/// </summary>
		/// <param name="code">Authority code</param>
		/// <returns>Compound coordinate system for the given code</returns>
		ICompoundCoordinateSystem CreateCompoundCoordinateSystem(long code);
		/// <summary>
		/// Creates a <see cref="IHorizontalCoordinateSystem">horizontal co-ordinate system</see> from a code.
		/// The horizontal coordinate system could be geographic or projected.
		/// </summary>
		/// <param name="code">Authority code</param>
		/// <returns>Horizontal coordinate system for the given code</returns>
		IHorizontalCoordinateSystem CreateHorizontalCoordinateSystem(long code);
		/// <summary>
		/// Gets a description of the object corresponding to a code.
		/// </summary>
		string DescriptionText { get; }
		/// <summary>
		/// Gets the Geoid code from a WKT name.
		/// </summary>
		/// <remarks>
		///  In the OGC definition of WKT horizontal datums, the geoid is referenced 
		/// by a quoted string, which is used as a key value. This method converts 
		/// the key value string into a code recognized by this authority.
		/// </remarks>
		/// <param name="wkt"></param>
		/// <returns></returns>
		string GeoidFromWktName(string wkt);
		/// <summary>
		/// Gets the WKT name of a Geoid.
		/// </summary>
		/// <remarks>
		///  In the OGC definition of WKT horizontal datums, the geoid is referenced by 
		/// a quoted string, which is used as a key value. This method gets the OGC WKT 
		/// key value from a geoid code.
		/// </remarks>
		/// <param name="geoid"></param>
		/// <returns></returns>
		string WktGeoidName(string geoid);		
	}
}
