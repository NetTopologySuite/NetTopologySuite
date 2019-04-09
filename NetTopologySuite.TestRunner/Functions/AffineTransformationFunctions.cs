using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class AffineTransformationFunctions
    {
        /// <summary>
        /// Transforms a geometry using one to three control vectors.
        /// </summary>
        public static Geometry TransformByVectors(Geometry g, Geometry control)
        {
            int nControl = control.NumGeometries;
            var src = new Coordinate[nControl];
            var dest = new Coordinate[nControl];
            for (int i = 0; i < nControl; i++)
            {
                var contComp = control.GetGeometryN(i);
                var pts = contComp.Coordinates;
                src[i] = pts[0];
                dest[i] = pts[1];
            }
            var trans = AffineTransformationFactory.CreateFromControlVectors(src, dest);
            Console.WriteLine(trans);
            return trans.Transform(g);
        }

        /// <summary>
        /// Transforms a geometry by mapping envelope baseline to target vector.
        /// </summary>
        public static Geometry TransformByBaseline(Geometry g, Geometry destBaseline)
        {
            var env = g.EnvelopeInternal;
            var src0 = new Coordinate(env.MinX, env.MinY);
            var src1 = new Coordinate(env.MaxX, env.MinY);

            var destPts = destBaseline.Coordinates;
            var dest0 = destPts[0];
            var dest1 = destPts[1];
            var trans = AffineTransformationFactory.CreateFromBaseLines(src0, src1, dest0, dest1);
            return trans.Transform(g);
        }

        private static Coordinate EnvelopeCentre(Geometry g)
        {
            return g.EnvelopeInternal.Centre;
        }

        private static Coordinate EnvelopeLowerLeft(Geometry g)
        {
            var env = g.EnvelopeInternal;
            return new Coordinate(env.MinX, env.MinY);
        }

        public static Geometry Scale(Geometry g, double scale)
        {
            var centre = EnvelopeCentre(g);
            var trans = AffineTransformation.ScaleInstance(scale, scale, centre.X, centre.Y);
            return trans.Transform(g);
        }

        public static Geometry ReflectInX(Geometry g)
        {
            var centre = EnvelopeCentre(g);
            var trans = AffineTransformation.ScaleInstance(1, -1, centre.X, centre.Y);
            return trans.Transform(g);
        }

        public static Geometry ReflectInY(Geometry g)
        {
            var centre = EnvelopeCentre(g);
            var trans = AffineTransformation.ScaleInstance(-1, 1, centre.X, centre.Y);
            return trans.Transform(g);
        }

        public static Geometry Rotate(Geometry g, double multipleOfPi)
        {
            var centre = EnvelopeCentre(g);
            var trans = AffineTransformation.RotationInstance(multipleOfPi * Math.PI, centre.X, centre.Y);
            return trans.Transform(g);
        }

        public static Geometry TranslateCentreToOrigin(Geometry g)
        {
            var centre = EnvelopeCentre(g);
            var trans = AffineTransformation.TranslationInstance(-centre.X, -centre.Y);
            return trans.Transform(g);
        }
        public static Geometry TranslateToOrigin(Geometry g)
        {
            var lowerLeft = EnvelopeLowerLeft(g);
            var trans = AffineTransformation.TranslationInstance(-lowerLeft.X, -lowerLeft.Y);
            return trans.Transform(g);
        }
    }
}
