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
using GisSharpBlog.NetTopologySuite.Positioning;
using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{


	/// <summary>
	/// Abstract class from which transformation can inherit from to get default functionality.
	/// </summary>
	public abstract class MathTransform : IMathTransform
	{
		/// <summary>
		/// Initializes a new instance of the AbstractMathTransform class.
		/// </summary>
		public MathTransform() 
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the AbstractMathTransform class.
		/// </summary>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		public MathTransform(string remarks,string authority, string authorityCode,
			string name,string alias,string abbreviation )
		{
		}

		#region Implementation of IMathTransform
		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="Ord"></param>
		/// <returns></returns>
		public double[]GetCodomainConvexHull(double[] Ord)
		{
			throw new NotImplementedException();
		}

		

		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <returns></returns>
		public bool IsIdentity()
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="Ord"></param>
		/// <returns></returns>
		public DomainFlags GetDomainFlags(double[] Ord)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///  Not implemented.
		/// </summary>
		/// <param name="Cp"></param>
		/// <returns></returns>
		public Matrix Derivative(CoordinatePoint Cp)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="Cp"></param>
		/// <returns></returns>
		public virtual CoordinatePoint Transform(CoordinatePoint Cp)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///  Not implemented.
		/// </summary>
		/// <param name="Ord"></param>
		/// <returns></returns>
		public virtual double[] TransformList(double[] Ord)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///  Not implemented.
		/// </summary>
		public int GetDimTarget()
		{
			throw new NotImplementedException();
		}


		/// <summary>
		///  Not implemented.
		/// </summary>
		public int DimSource
		{
			get
			{
				throw new NotImplementedException();
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public virtual IMathTransform GetInverse()
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// 
        /// </summary>
		public string WKT
		{
			get
			{
				throw new NotImplementedException();
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public string XML
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}	
}
