/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */
using System;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Proceedure used to measure vertical distances.
	/// </summary>
	public class VerticalDatum : Datum, IVerticalDatum
	{
		/// <summary>
		/// Initializes a new instance of the VerticalDatum class with the specified properties.
		/// </summary>
		/// <param name="name">The name of the vertical datum.</param>
		/// <param name="verticalDatumType">The datum type.</param>
		internal VerticalDatum(string name, DatumType verticalDatumType)
			: this(verticalDatumType,String.Empty,String.Empty,String.Empty,name,String.Empty,String.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the VerticalDatum class with the specified properties.
		/// </summary>
		/// <param name="datumType">The datum type.</param>
		/// <param name="remarks">The provider-supplied remarks.</param>
		/// <param name="authority">The authority-specific identification code.</param>
		/// <param name="authorityCode">The authority-specific identification code.</param>
		/// <param name="name">The name.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="abbreviation">The abbreviation.</param>
		public VerticalDatum(	DatumType datumType,
			string remarks,
			string authorityCode,
			string authority, 
			string name,
			string alias,
			string abbreviation )
			: base(datumType, remarks,authority,authorityCode, name, alias, abbreviation )
		{
		}

		#region Static
		
		/// <summary>
		/// Default vertical datum for ellipsoidal heights. Ellipsoidal heights
		/// are measured along the normal to the ellipsoid used in the definition
		/// of horizontal datum.
		/// </summary>
		public static VerticalDatum Ellipsoidal
		{
			get
			{
				return new VerticalDatum("Ellipsoidal", DatumType.IVD_Ellipsoidal);
			}
		}
		#endregion
	}
}
