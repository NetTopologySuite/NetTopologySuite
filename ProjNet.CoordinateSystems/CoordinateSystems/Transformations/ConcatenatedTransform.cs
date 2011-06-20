// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
using GeoAPI.CoordinateSystems.Transformations;

namespace ProjNet.CoordinateSystems.Transformations
{
    /// <summary>
    /// 
    /// </summary>
	internal class ConcatenatedTransform : MathTransform
	{
        /// <summary>
        /// 
        /// </summary>
		protected IMathTransform _inverse;

        /// <summary>
        /// 
        /// </summary>
		public ConcatenatedTransform() : 
            this(new List<ICoordinateTransformation>()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transformlist"></param>
		public ConcatenatedTransform(List<ICoordinateTransformation> transformlist)
		{
			_CoordinateTransformationList = transformlist;
		}

		private List<ICoordinateTransformation> _CoordinateTransformationList;

        /// <summary>
        /// 
        /// </summary>
		public List<ICoordinateTransformation> CoordinateTransformationList
		{
			get { return _CoordinateTransformationList; }
			set
			{
				_CoordinateTransformationList = value;
				_inverse = null;
			}
		}
		
        /// <summary>
        /// Transforms a point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double[] Transform(double[] point)
		{
            foreach (ICoordinateTransformation ct in _CoordinateTransformationList)
				point = ct.MathTransform.Transform(point);            
			return point;			
		}

        /// <summary>
		/// Transforms a list point
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public override List<double[]> TransformList(List<double[]> points)
		{
			List<double[]> pnts = new List<double[]>(points.Count);
			pnts.AddRange(points);
			foreach (ICoordinateTransformation ct in _CoordinateTransformationList)
				pnts = ct.MathTransform.TransformList(pnts);
			return pnts;
		}

		/// <summary>
		/// Returns the inverse of this conversion.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current conversion.</returns>
		public override IMathTransform Inverse()
		{
			if (_inverse == null)
			{
				_inverse = this.Clone();
				_inverse.Invert();
			}
			return _inverse;
		}
		
		/// <summary>
		/// Reverses the transformation
		/// </summary>
		public override void Invert()
		{
			_CoordinateTransformationList.Reverse();
			foreach (ICoordinateTransformation ic in _CoordinateTransformationList)
				ic.MathTransform.Invert();
		}

		public ConcatenatedTransform Clone()
		{
			List<ICoordinateTransformation> clonedList = new List<ICoordinateTransformation>(_CoordinateTransformationList.Count);
			foreach (ICoordinateTransformation ct in _CoordinateTransformationList)
				clonedList.Add(ct);
			return new ConcatenatedTransform(clonedList);
		}

        /// <summary>
        /// Gets a Well-Known text representation of this object.
        /// </summary>
        /// <value></value>
        public override string WKT
		{
			get { throw new NotImplementedException(); }
		}

        /// <summary>
        /// Gets an XML representation of this object.
        /// </summary>
        /// <value></value>
		public override string XML
		{
			get { throw new NotImplementedException(); }
		}
	}
}
