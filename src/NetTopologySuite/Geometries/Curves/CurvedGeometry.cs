// SPDX-License-Identifier: BSD-3-Clause
// Assisted-by: xAI Grok

using System;

namespace NetTopologySuite.Geometries.Curves
{
    /// <summary>
    /// Shared fail-closed guards for SQL/MM curve types until arc-aware
    /// metric and analytic implementations land.
    /// </summary>
    internal static class CurvedGeometry
    {
        /// <summary>
        /// True for the five SQL/MM curve types. Excludes
        /// <see cref="LineString"/>, <see cref="LinearRing"/>, <see cref="Polygon"/>,
        /// and <see cref="Triangle"/>.
        /// </summary>
        public static bool IsCurvedType(Geometry g)
        {
            return g is CircularString
                || g is CompoundCurve
                || g is CurvePolygon
                || g is MultiCurve
                || g is MultiSurface;
        }

        /// <summary>
        /// True if <paramref name="g"/> is a curved type or a
        /// <see cref="GeometryCollection"/> that contains one.
        /// </summary>
        public static bool ContainsCurvedType(Geometry g)
        {
            if (IsCurvedType(g))
                return true;
            if (g is GeometryCollection gc)
            {
                for (int i = 0; i < gc.NumGeometries; i++)
                {
                    if (ContainsCurvedType(gc.GetGeometryN(i)))
                        return true;
                }
            }
            return false;
        }

        public static NotSupportedException NotYetSupported(Geometry g, string operation)
        {
            return new NotSupportedException(
                $"Arc-aware {operation} is not implemented for {g.GeometryType} yet (planned follow-up PR). Call Linearize() to opt in to an explicit chord approximation.");
        }

        public static void CheckNotCurved(Geometry g, string operation)
        {
            if (ContainsCurvedType(g))
                throw NotYetSupported(g, operation);
        }

        public static NotSupportedException ToleranceLinearizeNotSupported()
        {
            return new NotSupportedException(
                "Tolerance-driven linearization is not implemented yet (planned follow-up PR). Call Linearize() for the explicit chord approximation.");
        }

        /// <summary>
        /// Hash of a control-point envelope. Identity only — not a geometric answer.
        /// </summary>
        public static int HashControlEnvelope(CoordinateSequence points)
        {
            var env = new Envelope();
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                    env.ExpandToInclude(points.GetCoordinate(i));
            }
            return env.GetHashCode();
        }

        /// <summary>
        /// Hash of a control-point envelope. Identity only — not a geometric answer.
        /// </summary>
        public static int HashControlEnvelope(Coordinate[] coordinates)
        {
            var env = new Envelope();
            if (coordinates != null)
            {
                for (int i = 0; i < coordinates.Length; i++)
                    env.ExpandToInclude(coordinates[i]);
            }
            return env.GetHashCode();
        }

        /// <summary>
        /// Hash of the union of control-point envelopes, walking collections.
        /// </summary>
        public static int HashControlEnvelope(Geometry g)
        {
            var env = new Envelope();
            ExpandControlEnvelope(g, env);
            return env.GetHashCode();
        }

        private static void ExpandControlEnvelope(Geometry g, Envelope env)
        {
            if (g is GeometryCollection gc)
            {
                for (int i = 0; i < gc.NumGeometries; i++)
                    ExpandControlEnvelope(gc.GetGeometryN(i), env);
                return;
            }
            var coordinates = g.Coordinates;
            for (int i = 0; i < coordinates.Length; i++)
                env.ExpandToInclude(coordinates[i]);
        }
    }
}
