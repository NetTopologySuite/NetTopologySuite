using System;
using System.Collections.Generic;
using DotSpatial.Projections;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.CoordinateSystems.Transformation.DotSpatial.Projections
{
    public class DotSpatialMathTransform : IMathTransform
    {
        //#region static helpers

        //internal static void ExtractCoordinates(double[] point, out double[] xy, out double[] z)
        //{
            
        //}

        //internal static void ExtractCoordinates(List<double[]> points, out double[] xy, out double[] z)
        //{

        //}

        //#endregion

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

        public List<double[]> TransformList(List<double[]> points)
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

        public void Invert()
        {
            var tmp = Source;
            Source = Target;
            Target = tmp;
        }

        public Coordinate Transform(Coordinate coordinate)
        {
            var xy = new[] { coordinate.X, coordinate.Y };
            var z = new[] { coordinate.Z };

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, 1);

            var ret = (Coordinate)coordinate.Clone();
            ret.X = xy[0];
            ret.Y = xy[1];
            ret.Z = z[0];

            return ret;
        }

        public ICoordinate Transform(ICoordinate coordinate)
        {
            return Transform(coordinate.ToCoordinate());
        }

        public ICoordinateSequence Transform(ICoordinateSequence coordinateSequence)
        {
            //use shortcut if possible
            if (coordinateSequence is DotSpatialAffineCoordinateSequence)
                return TransformDotSpatialAffine((DotSpatialAffineCoordinateSequence) coordinateSequence);

            var xy = new double[2*coordinateSequence.Count];
            var z = new double[coordinateSequence.Count];

            var j = 0;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                xy[j++] = coordinateSequence.GetOrdinate(i, Ordinate.X);
                xy[j++] = coordinateSequence.GetOrdinate(i, Ordinate.Z);
                z[i] = coordinateSequence.GetOrdinate(i, Ordinate.Z);
            }

            Reproject.ReprojectPoints(xy, z, Source, Target, 0, coordinateSequence.Count);

            var ret = (ICoordinateSequence) coordinateSequence.Clone();
            j = 0;
            for (var i = 0; i < coordinateSequence.Count; i++)
            {
                ret.SetOrdinate(i, Ordinate.X, xy[j]);
                ret.SetOrdinate(i, Ordinate.X, xy[j]);
                if (DimTarget > 2) ret.SetOrdinate(i, Ordinate.Z, z[j]);
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
