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
	/// The ISpatialReferenceInfo interface defines the standard 
	/// information stored with spatial reference objects. This
	/// interface is reused for many of the spatial reference
	/// objects in the system.
	/// </summary>
	public interface IInfo
	{
		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets or sets the authority name for this object, e.g., “POSC”,
		/// is this is a standard object with an authority specific
		/// identity code. Returns “CUSTOM” if this is a custom object.
		/// </summary>
		string Authority { get; }
		/// <summary>
		/// Gets or sets the authority specific identification code of the object
		/// </summary>
		long AuthorityCode { get; }
		/// <summary>
		/// Gets or sets the alias of the object.
		/// </summary>
		string Alias { get; }
		/// <summary>
		/// Gets or sets the abbreviation of the object.
		/// </summary>
		string Abbreviation { get; }
		/// <summary>
		/// Gets or sets the provider-supplied remarks for the object.
		/// </summary>
		string Remarks { get; }
		/// <summary>
		/// Returns the Well-known text for this spatial reference object
		/// as defined in the simple features specification.
		/// </summary>
		string WKT { get; }
		/// <summary>
		/// Gets an XML representation of this object.
		/// </summary>
		string XML { get; }

		/// <summary>
		/// Checks whether the values of this instance is equal to the values of another instance.
		/// Only parameters used for coordinate system are used for comparison.
		/// Name, abbreviation, authority, alias and remarks are ignored in the comparison.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>True if equal</returns>
		bool EqualParams(object obj);
	}
}
