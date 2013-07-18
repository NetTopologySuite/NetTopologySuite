// Copyright 2011 - 2013 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of DotSpatial.Projections.Wrapper.
// DotSpatial.Projections.Wrapper is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// DotSpatial.Projections.Wrapper is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using DotSpatial.Projections;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.CoordinateSystems.Transformation.DotSpatial.Projections
{
    /// <summary>
    /// 
    /// </summary>
    public class DotSpatialMathTransform : IMathTransform
    {
        #region Fields
        public ProjectionInfo Source;
        public ProjectionInfo Target;
        #endregion

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public DotSpatialMathTransform(ProjectionInfo source, ProjectionInfo target)
        {
            Source = source;
            Target = target;
        }

        public int DimSource
        {
            get { return Source.IsGeocentric ? 3 : 2; }
        }

        public int DimTarget
        {
            get { return Target.IsGeocentric ? 3 : 2; }
        }

        public bool Identity()
        {
            return Source.Equals(Target);
        }

        public string WKT
        {
            get { return string.Empty; }
        }

        public string XML
        {
            get { throw new NotSupportedException();}
        }

        public double[,] Derivative(double[] point)
        {
            throw new NotSupportedException();
        }

        public List<double> GetCodomainConvexHull(List<double> points)
        {
            throw new NotSupportedException();
        }

        public DomainFlags GetDomainFlags(List<double> points)
        {
            throw new NotSupportedException();
        }

        public IMathTransform Inverse()
        {
            return new DotSpatialMathTransform(Target, Source);
        }

        public double[] Transform(double[] point)
        {
            var xy = new[] { point[0], point[1] } ;
            var z = new double[1];

            if (DimSource > 2)
                z[0] = point[2];

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, 1);

            if (DimTarget > 2)
                return new [] {xy[0], xy[1], z[0]};

            return xy;
        }

        public IList<double[]> TransformList(IList<double[]> points)
        {
            var xy = new double[2 * points.Count];
            var z = new double[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                xy[2 * i] = points[i][0];
                xy[2 * i + 1] = points[i][1];
                if (DimSource > 2)
                    z[i] = points[i][2];
            }

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, points.Count);

            var ret = new List<double[]>(points.Count);
            if (DimTarget > 2)
            {
                for (int i = 0; i < points.Count; i++)
                    ret.Add(new[] {xy[2*i], xy[2*i + 1], z[i]});
            }
            else
            {
                for (int i = 0; i < points.Count; i++)
                    ret.Add(new[] { xy[2 * i], xy[2 * i + 1] });
            }
            return ret;
        }

        public IList<Coordinate> TransformList(IList<Coordinate> points)
        {
            var xy = new double[2 * points.Count];
            var z = new double[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                xy[2 * i] = points[i].X;
                xy[2 * i + 1] = points[i].Y;
                if (DimSource > 2)
                    z[i] = points[i].Z;
            }

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, points.Count);

            var ret = new List<Coordinate>(points.Count);
            if (DimTarget > 2)
            {
                for (int i = 0; i < points.Count; i++)
                    ret.Add(new Coordinate( xy[2 * i], xy[2 * i + 1], z[i] ));
            }
            else
            {
                for (int i = 0; i < points.Count; i++)
                    ret.Add(new Coordinate( xy[2 * i], xy[2 * i + 1]));
            }
            return ret;
        }

        public void Invert()
        {
            var tmp = Source;
            Source = Target;
            Target = tmp;
        }

        [Obsolete]
        public ICoordinate Transform(ICoordinate coordinate)
        {
            var xy = new[] {coordinate.X, coordinate.Y};
            var z = new[] {coordinate.Z};

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, 1);

            var ret = (ICoordinate)coordinate.Clone();
            ret.X = xy[0];
            ret.Y = xy[1];
            ret.Z = z[0];

            return ret;
        }

        public Coordinate Transform(Coordinate coordinate)
        {
            var xy = new[] { coordinate.X, coordinate.Y };
            double[] z = null;
            if (!coordinate.Z.Equals(Coordinate.NullOrdinate))
                z = new[] { coordinate.Z };

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, 1);

            var ret = (Coordinate)coordinate.Clone();
            ret.X = xy[0];
            ret.Y = xy[1];
            if (z != null)
                ret.Z = z[0];

            return ret;
        }

        public ICoordinateSequence Transform(ICoordinateSequence coordinateSequence)
        {
            //use shortcut if possible
            var sequence = coordinateSequence as DotSpatialAffineCoordinateSequence;
            if (sequence != null)
                return TransformDotSpatialAffine(sequence);

            var xy = new double[2*coordinateSequence.Count];
            double[] z = null;
            if (!double.IsNaN(coordinateSequence.GetOrdinate(0, Ordinate.Z)))
                z = new double[coordinateSequence.Count];

            var j = 0;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                xy[j++] = coordinateSequence.GetOrdinate(i, Ordinate.X);
                xy[j++] = coordinateSequence.GetOrdinate(i, Ordinate.Y);
                if (z != null) z[i] = coordinateSequence.GetOrdinate(i, Ordinate.Z);
            }

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, coordinateSequence.Count);

            var ret = (ICoordinateSequence) coordinateSequence.Clone();
            j = 0;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                ret.SetOrdinate(i, Ordinate.X, xy[j++]);
                ret.SetOrdinate(i, Ordinate.Y, xy[j++]);
                if (z != null && DimTarget>2) 
                    ret.SetOrdinate(i, Ordinate.Z, z[i]);
                else 
                    ret.SetOrdinate(i,Ordinate.Z, coordinateSequence.GetOrdinate(i, Ordinate.Z));
            }

            return ret;
        }

        private ICoordinateSequence TransformDotSpatialAffine(DotSpatialAffineCoordinateSequence sequence)
        {
            var seq = (DotSpatialAffineCoordinateSequence)sequence.Clone();
            Reproject.ReprojectPoints(seq.XY, seq.Z, Source, Target, 0, seq.Count);
            return seq;
        }
    }
}
