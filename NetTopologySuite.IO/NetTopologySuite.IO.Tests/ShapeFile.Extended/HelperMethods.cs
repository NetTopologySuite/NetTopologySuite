using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO.Handlers;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.ShapeFile.Extended
{
    public static class HelperMethods
    {
        private static readonly double REQUIRED_PRECISION = Math.Pow(10, -9);

        public static void AssertEnvelopesEqual(Envelope env1, Envelope env2)
        {
            AssertEnvelopesEqual(env1, env2, REQUIRED_PRECISION);
        }

        public static void AssertEnvelopesEqual(Envelope env1, Envelope env2, double requiredPrecision, string errorMessage = "")
        {
            AssertDoubleValuesEqual(env1.MaxX, env2.MaxX, requiredPrecision, errorMessage);
            AssertDoubleValuesEqual(env1.MaxY, env2.MaxY, requiredPrecision, errorMessage);
            AssertDoubleValuesEqual(env1.MinX, env2.MinX, requiredPrecision, errorMessage);
            AssertDoubleValuesEqual(env1.MinY, env2.MinY, requiredPrecision, errorMessage);
        }

        public static void AssertPolygonsEqual(IPolygon poly1, IPolygon poly2)
        {
            Assert.IsNotNull(poly1);
            Assert.IsNotNull(poly2);

            ILineString line1 = poly1.Shell;
            ILineString line2 = poly2.Shell;

            Assert.AreEqual(line1.Coordinates.Length, line2.Coordinates.Length, "Number of coordinates between polygons doesn't match");

            for (int i = 0; i < line2.Coordinates.Length; i++)
            {
                AssertCoordinatesEqual(line2.Coordinates[i], line1.Coordinates[i]);
            }
        }

        public static void AssertCoordinatesEqual(Coordinate coord1, Coordinate coord2)
        {
            AssertDoubleValuesEqual(coord1.X, coord2.X);
            AssertDoubleValuesEqual(coord1.Y, coord2.Y);
        }

        public static void AssertDoubleValuesEqual(double num1, double num2)
        {
            AssertDoubleValuesEqual(num1, num2, REQUIRED_PRECISION);
        }

        public static void AssertDoubleValuesEqual(double num1, double num2, double requiredPrecision, string errorMessage = "")
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                Assert.AreEqual(num1, num2, requiredPrecision);
            }
            else
            {
                Assert.AreEqual(num1, num2, requiredPrecision, errorMessage);
            }
        }

        public static void AssertMBRInfoEqual(MBRInfo info1, MBRInfo info2)
        {
            Assert.AreEqual(info1.ShapeFileDetails.OffsetFromStartOfFile, info2.ShapeFileDetails.OffsetFromStartOfFile);
            Assert.AreEqual(info1.ShapeFileDetails.ShapeIndex, info2.ShapeFileDetails.ShapeIndex);
            AssertEnvelopesEqual(info1.ShapeMBR, info2.ShapeMBR);
        }
    }
}
