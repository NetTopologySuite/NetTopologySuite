using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
/**
 * Utility functions for working with angles.
 * Unless otherwise noted, methods in this class express angles in radians.
 */
    public static class Angle<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
    {
        public const Double PiTimes2 = 2.0 * Math.PI;
        public const Double PiOver2 = Math.PI / 2.0;
        public const Double PiOver4 = Math.PI / 4.0;

        ///<summary>
        /// Converts from radians to degrees.
        ///</summary>
        ///<param name="radians">an angle in radians</param>
        ///<returns>the angle in degrees</returns>
        public static Double ToDegrees(Double radians)
        {
            return (radians * 180) / (Math.PI);
        }

        ///<summary>
        /// Converts from degrees to radians.
        ///</summary>
        ///<param name="angleDegrees">an angle in degrees</param>
        ///<returns>the angle in radians</returns>
        public static Double ToRadians(Double angleDegrees)
        {
            return (angleDegrees * Math.PI) / 180.0;
        }


        ///<summary>
        /// Returns the angle of the vector from p0 to p1,
        /// relative to the positive X-axis.
        /// The angle is normalized to be in the range [ -Pi, Pi ].
        ///</summary>
        ///<param name="p0"></param>
        ///<param name="p1"></param>
        ///<returns>the normalized angle (in radians) that p0-p1 makes with the positive x-axis.</returns>
        public static Double CalculateAngle(TCoordinate p0, TCoordinate p1)
        {
            Double dx = p1[Ordinates.X] - p0[Ordinates.X];
            Double dy = p1[Ordinates.Y] - p0[Ordinates.Y];
            return Math.Atan2(dy, dx);
        }

        ///<summary>
        /// Returns the angle that the vector from (0,0) to p,
        /// relative to the positive X-axis.
        /// The angle is normalized to be in the range [ -Pi, Pi ].
        ///</summary>
        ///<param name="p"></param>
        ///<returns>the normalized angle (in radians) that p makes with the positive x-axis.</returns>
        public static Double CalculateAngle(TCoordinate p)
        {
            return Math.Atan2(p[Ordinates.Y], p[Ordinates.Y]);
        }

        ///<summary>
        /// Tests whether the angle between p0-p1-p2 is acute.
        /// An angle is acute if it is less than 90 degrees.
        /// Note: this implementation is not robust for angles very close to 90 degrees.
        ///</summary>
        ///<param name="p0">an endpoint of the angle</param>
        ///<param name="p1">the base of the angle</param>
        ///<param name="p2">the other endpoint of the angle</param>
        ///<returns></returns>
        public static Boolean IsAcute(TCoordinate p0, TCoordinate p1, TCoordinate p2)
        {
            // relies on fact that A dot B is positive iff A ang B is acute
            Double dx0 = p0[Ordinates.X] - p1[Ordinates.X];
            Double dy0 = p0[Ordinates.Y] - p1[Ordinates.Y];
            Double dx1 = p2[Ordinates.X] - p1[Ordinates.X];
            Double dy1 = p2[Ordinates.Y] - p1[Ordinates.Y];
            Double dotprod = dx0 * dx1 + dy0 * dy1;
            return dotprod > 0d;
        }

        ///<summary>
        /// Tests whether the angle between p0-p1-p2 is obtuse.
        /// An angle is obtuse if it is greater than 90 degrees.
        /// Note: this implementation is not robust for angles very close to 90 degrees.
        ///</summary>
        ///<param name="p0">an endpoint of the angle</param>
        ///<param name="p1">the base of the angle</param>
        ///<param name="p2">the other endpoint of the angle</param>
        ///<returns></returns>
        public static Boolean IsObtuse(TCoordinate p0, TCoordinate p1, TCoordinate p2)
        {
            // relies on fact that A dot B is negative iff A ang B is obtuse
            Double dx0 = p0[Ordinates.X] - p1[Ordinates.X];
            Double dy0 = p0[Ordinates.Y] - p1[Ordinates.Y];
            Double dx1 = p2[Ordinates.X] - p1[Ordinates.X];
            Double dy1 = p2[Ordinates.Y] - p1[Ordinates.Y];
            Double dotprod = dx0 * dx1 + dy0 * dy1;
            return dotprod < 0d;
        }

        ///<summary>
        /// Returns the unoriented smallest angle between two vectors.
        /// The computed angle will be in the range [0, Pi].
        ///</summary>
        ///<param name="tip1">the tip of one vector</param>
        ///<param name="tail">the tail of each vector</param>
        ///<param name="tip2">the tip of the other vector</param>
        ///<returns>the angle between tail-tip1 and tail-tip2</returns>
        public static Double CalculateAngleBetween(TCoordinate tip1, TCoordinate tail,
                  TCoordinate tip2)
        {
            Double a1 = CalculateAngle(tail, tip1);
            Double a2 = CalculateAngle(tail, tip2);

            return Difference(a1, a2);
        }

        ///<summary>
        /// Returns the oriented smallest angle between two vectors.
        /// The computed angle will be in the range (-Pi, Pi].
        /// A positive result corresponds to a counterclockwise rotation
        /// from v1 to v2;
        /// a negative result corresponds to a clockwise rotation.
        ///</summary>
        ///<param name="tip1">the tip of v1</param>
        ///<param name="tail">the tail of each vector</param>
        ///<param name="tip2">the tip of v2</param>
        ///<returns>the angle between v1 and v2, relative to v1</returns>
        public static Double CalculateAngleBetweenOriented(TCoordinate tip1, TCoordinate tail,
                  TCoordinate tip2)
        {
            Double a1 = CalculateAngle(tail, tip1);
            Double a2 = CalculateAngle(tail, tip2);
            Double angDel = a2 - a1;

            // normalize, maintaining orientation
            if (angDel <= -Math.PI)
                return angDel + PiTimes2;
            if (angDel > Math.PI)
                return angDel - PiTimes2;
            return angDel;
        }

        ///<summary>
        /// Computes the interior angle between two segments of a ring. The ring is
        /// assumed to be oriented in a clockwise direction. The computed angle will be
        /// in the range [0, 2Pi]
        ///</summary>
        ///<param name="p0">a point of the ring</param>
        ///<param name="p1">the next point of the ring</param>
        ///<param name="p2">the next point of the ring</param>
        ///<returns>the interior angle based at <code>p1</code></returns>
        public static Double InteriorAngle(TCoordinate p0, TCoordinate p1, TCoordinate p2)
        {
            Double anglePrev = CalculateAngle(p1, p0);
            Double angleNext = CalculateAngle(p1, p2);
            return Math.Abs(angleNext - anglePrev);
        }

        ///<summary>
        /// Returns whether an angle must turn clockwise or counterclockwise
        /// to overlap another angle.
        ///</summary>
        ///<param name="ang1">an angle (in radians)</param>
        ///<param name="ang2">an angle (in radians)</param>
        ///<returns>whether <code>ang1</code> must turn CLOCKWISE, COUNTERCLOCKWISE or NONE to overlap <code>ang2</code></returns>
        public static Orientation GetTurn(Double ang1, Double ang2)
        {
            Double crossproduct = Math.Sin(ang2 - ang1);

            if (crossproduct > 0)
            {
                return Orientation.CounterClockwise;
            }

            if (crossproduct < 0)
            {
                return Orientation.Clockwise;
            }

            return Orientation.Collinear;
        }

        ///<summary>
        /// Computes the normalized value of an angle, which is the
        /// equivalent angle in the range [ -Pi, Pi ].
        ///</summary>
        ///<param name="angle">the angle to normalize</param>
        ///<returns>an equivalent angle in the range [-Pi, Pi]</returns>
        public static Double Normalize(Double angle)
        {
            while (angle > Math.PI)
                angle -= PiTimes2;
            while (angle < -Math.PI)
                angle += PiTimes2;
            return angle;
        }

        ///<summary>
        /// Computes the normalized positive value of an angle, which is the
        /// equivalent angle in the range [ 0, 2*Pi ].
        ///</summary>
        ///<param name="angle">the angle to normalize, in radians</param>
        ///<returns>an equivalent positive angle</returns>
        public static Double NormalizePositive(Double angle)
        {
            while (angle < 0.0)
                angle += PiTimes2;
            return angle;
        }

        ///<summary>
        /// Computes the unoriented smallest difference between two angles.
        /// The angles are assumed to be normalized to the range [-Pi, Pi].
        /// The result will be in the range [0, Pi].
        ///</summary>
        ///<param name="ang1">the angle of one vector (in [-Pi, Pi] )</param>
        ///<param name="ang2">the angle of the other vector (in range [-Pi, Pi] )</param>
        ///<returns>the angle (in radians) between the two vectors (in range [0, Pi] )</returns>
        public static Double Difference(Double ang1, Double ang2)
        {
            Double delAngle;

            if (ang1 < ang2)
            {
                delAngle = ang2 - ang1;
            }
            else
            {
                delAngle = ang1 - ang2;
            }

            if (delAngle > Math.PI)
            {
                delAngle = (2 * Math.PI) - delAngle;
            }

            return delAngle;
        }
    }
}
