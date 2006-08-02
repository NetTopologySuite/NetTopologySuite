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
	/// A local datum.
	/// </summary>
	public class LocalDatum : Datum, ILocalDatum
	{
		/// <summary>
		/// Initializes a new instance of the LocalDatum class with the specified propeties.
		/// </summary>
		/// <param name="name">The name of the datum.</param>
		/// <param name="datumType">The Datum type.</param>
		internal LocalDatum(string name, DatumType datumType) : base(name, datumType)
		{
		}
	}
}
