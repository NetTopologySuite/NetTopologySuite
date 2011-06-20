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
	/// The IParameterInfo interface provides an interface through which clients of a
	/// Projected Coordinate System or of a Projection can set the parameters of the
	/// projection. It provides a generic interface for discovering the names and default
	/// values of parameters, and for setting and getting parameter values. Subclasses of
	/// this interface may provide projection specific parameter access methods.
	/// </summary>
	public interface IParameterInfo
	{
		/// <summary>
		/// Gets the number of parameters expected.
		/// </summary>
		int NumParameters { get; }
		/// <summary>
		/// Returns the default parameters for this projection.
		/// </summary>
		/// <returns></returns>
		Parameter[] DefaultParameters();
		/// <summary>
		/// Gets or sets the parameters set for this projection.
		/// </summary>
		List<Parameter> Parameters { get; set; }
		/// <summary>
		/// Gets the parameter by its name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Parameter GetParameterByName(string name);
	}
}
