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
using GisSharpBlog.NetTopologySuite.CoordinateTransformations;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// A projection from geographic coordinates to projected coordinates.
	/// </summary>
	public class Projection : AbstractInformation, IProjection
	{
		//ParameterList _parameters;
		string _classication;
		ProjectionParameter[] _projectionParameters;

		#region Constructors
		
		/// <summary>
		/// Initializes a new instance of the Projection class.
		/// </summary>
		internal Projection(string name, ProjectionParameter[] projectionParameters, string classification, string remarks, string authority, string authorityCode)
			: base(remarks,authority,authorityCode,name,String.Empty,String.Empty)
		{
			_projectionParameters = projectionParameters;
			_classication = classification;
		}
		#endregion

		#region Implementation of IProjection
		/// <summary>
		/// Gets the parameters for this projection.
		/// </summary>
		/// <param name="index">The index of the parameter to return.</param>
		/// <returns>IProjectionParameter containing the parameter information.</returns>
		public ProjectionParameter GetParameter(int index)
		{
			return _projectionParameters[index];
		}

		
		/// <summary>
		/// The number of parameters for this projection.
		/// </summary>
		public int NumParameters
		{
			get
			{
				return _projectionParameters.Length;
			}
		}

	
		/// <summary>
		/// Gets the class name for this projection.
		/// </summary>
		public string ClassName
		{
			get
			{
				return _classication;
			}
		}

		#endregion
	}
}
