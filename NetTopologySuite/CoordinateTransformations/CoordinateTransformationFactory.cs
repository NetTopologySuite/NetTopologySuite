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
using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;
namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
	/// <summary>
	/// Creates coordinate transformations.
	/// </summary>
	public class CoordinateTransformationFactory : ICoordinateTransformationFactory
	{
		/// <summary>
		/// Creates coordinate transformations (not imple.
		/// </summary>
		public CoordinateTransformationFactory()
		{
		}

		#region Implementation of ICoordinateTransformationFactory
		/// <summary>
		/// Creates a transformation between two coordinate systems. (not implemented) 
		/// </summary>
		/// <remarks>
		/// This method will examine the coordinate systems in order to construct a transformation between them. This method may fail if no path between the coordinate systems is found, using the normal failing behavior of the DCP (e.g. throwing an exception).
		/// </remarks>
		/// <param name="sourceCoordinateSystem">The source coordinate system.</param>
		/// <param name="targetCoordinateSystem">The target coordinate system.</param>
		/// <returns></returns>
		public ICoordinateTransformation CreateFromCoordinateSystems(ICoordinateSystem sourceCoordinateSystem, ICoordinateSystem targetCoordinateSystem)
		{
			IProjectedCoordinateSystem projectedCS =  null;
			IGeographicCoordinateSystem geographicCS = null;

			if (sourceCoordinateSystem is IProjectedCoordinateSystem && targetCoordinateSystem is IGeographicCoordinateSystem)
			{
				projectedCS = (IProjectedCoordinateSystem)sourceCoordinateSystem;
				geographicCS = (IGeographicCoordinateSystem)targetCoordinateSystem; 
			}
			else if (targetCoordinateSystem is IProjectedCoordinateSystem && sourceCoordinateSystem is IGeographicCoordinateSystem)
			{
				projectedCS = (IProjectedCoordinateSystem)targetCoordinateSystem;
				geographicCS = (IGeographicCoordinateSystem)sourceCoordinateSystem; 
			}
			if (projectedCS==null || geographicCS==null)
			{
				throw new InvalidOperationException("Need a geographic and a projetced coordinate reference system to make a transform.");
			}
			IMathTransform mathTransform =	CreateCoordinateOperation( projectedCS.Projection, projectedCS.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid);
			
			ICoordinateTransformation coordinateTransformation = new CoordinateTransformation(
				TransformType.Transformation,
				geographicCS,
				projectedCS,
				mathTransform,
				String.Empty,
				String.Empty,
				String.Empty,
				String.Empty,
				String.Empty,String.Empty);
			return coordinateTransformation;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="ellipsoid"></param>
        /// <returns></returns>
		public static IMathTransform CreateCoordinateOperation(IProjection projection, IEllipsoid ellipsoid)
		{
			ParameterList parameterList = new ParameterList();
			for(int i=0; i< projection.NumParameters; i++)
			{
				ProjectionParameter param = projection.GetParameter(i);
				parameterList.Add(param.Name,param.Value);
			}
			parameterList.Add("semi_major",ellipsoid.SemiMajorAxis);
			parameterList.Add("semi_minor",ellipsoid.SemiMinorAxis);

			IMathTransform transform = null;
			switch(projection.Name.ToLower())
			{
				case "mercator":
					//1SP
					transform = new MercatorProjection(parameterList);
					break;
				case "transverse_mercator": 
					transform = new TransverseMercatorProjection(parameterList);
					break;
				case "albers": 
					transform = new AlbersProjection(parameterList);
					break;
				case "lambert": 
					transform = new LambertConformalConic2SPProjection(parameterList);
					break;
				default:
					throw new NotSupportedException(String.Format("Projection {0} is not supported.",projection.AuthorityCode));
			}
			return transform;
		}
		#endregion
	}
}
