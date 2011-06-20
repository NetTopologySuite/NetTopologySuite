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
	/// A named projection parameter value.
	/// </summary>
	/// <remarks>
	/// The linear units of parameters' values match the linear units of the containing 
	/// projected coordinate system. The angular units of parameter values match the 
	/// angular units of the geographic coordinate system that the projected coordinate 
	/// system is based on. (Notice that this is different from <see cref="Parameter"/>,
	/// where the units are always meters and degrees.)
	/// </remarks>
	public class ProjectionParameter
	{
		/// <summary>
		/// Initializes an instance of a ProjectionParameter
		/// </summary>
		/// <param name="name">Name of parameter</param>
		/// <param name="value">Parameter value</param>
		public ProjectionParameter(string name, double value)
		{
			_Name = name;
			_Value = value;
		}

		private string _Name;

		/// <summary>
		/// Parameter name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		private double _Value;

		/// <summary>
		/// Parameter value.
		/// The linear units of a parameters' values match the linear units of the containing 
		/// projected coordinate system. The angular units of parameter values match the 
		/// angular units of the geographic coordinate system that the projected coordinate 
		/// system is based on.
		/// </summary>
		public double Value
		{
			get { return _Value; }
			set { _Value = value; }
		}


		/// <summary>
		/// Returns the Well-known text for this object
		/// as defined in the simple features specification.
		/// </summary>
		public string WKT
		{
			get
			{
				return String.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "PARAMETER[\"{0}\", {1}]", Name, Value);
			}
		}

		/// <summary>
		/// Gets an XML representation of this object
		/// </summary>
		public string XML
		{
			get
			{
                return String.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "<CS_ProjectionParameter Name=\"{0}\" Value=\"{1}\"/>", Name, Value);
			}
		}
	}
}
