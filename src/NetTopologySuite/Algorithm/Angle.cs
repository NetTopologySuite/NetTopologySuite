using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Utility functions for working with angles.
    /// Unless otherwise noted, methods in this class express angles in radians.
    /// </summary>
    public static class AngleUtility
    {
        /// <summary>
        /// Value of 2 * Pi
        /// </summary>
        public const double PiTimes2 = 2.0 * System.Math.PI;
        /// <summary>
        /// Value of Pi / 2
        /// </summary>
        public const double PiOver2 = System.Math.PI / 2.0;
        /// <summary>
        /// Value of Pi / 4
        /// </summary>
        public const double PiOver4 = System.Math.PI / 4.0;

        /// <summary>
        /// Converts from radians to degrees.
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        /// <returns>The angle in degrees</returns>
        public static double ToDegrees(double radians)
        {
            return (radians * 180) / (System.Math.PI);
        }

        /// <summary>
        /// Converts from degrees to radians.
        /// </summary>
        /// <param name="angleDegrees">An angle in degrees</param>
        /// <returns>The angle in radians</returns>
        public static double ToRadians(double angleDegrees)
        {
            return (angleDegrees * System.Math.PI) / 180.0;
        }

        /// <summary>
        /// Returns the angle of the vector from p0 to p1, relative to the positive X-axis.
        /// </summary>
        /// <remarks>The angle is normalized to be in the range [ -Pi, Pi ].</remarks>
        /// <param name="p0">The initial point of the vector.</param>
        /// <param name="p1">The terminal point of the vector.</param>
        /// <returns>The normalized angle (in radians) that p0-p1 makes with the positive X-axis</returns>
        public static double Angle(Coordinate p0, Coordinate p1)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            return System.Math.Atan2(dy, dx);
        }

        /// <summary>
        /// Returns the angle of the vector from (0,0) to p, relative to the positive X-axis.
        /// </summary>
        /// <remarks>
        /// The angle is normalized to be in the range ( -Pi, Pi ].
        /// </remarks>
        /// <param name="p">The terminal point of the vector.</param>
        /// <returns>The normalized angle (in radians) that (0,0)-p makes with the positive X-axis.</returns>
        public static double Angle(Coordinate p)
        {
            return System.Math.Atan2(p.Y, p.X);
        }

        /// <summary>
        /// Tests whether the angle between p0-p1-p2 is acute.
        /// </summary>
        /// <remarks>
        /// <para>An angle is acute if it is less than 90 degrees.</para>
        /// <para>Note: this implementation is not precise (deterministic) for angles very close to 90 degrees.</para>
        /// </remarks>
        /// <param name="p0">An endpoint of the angle</param>
        /// <param name="p1">The base of the angle</param>
        /// <param name="p2">Another endpoint of the angle</param>
        /// <returns><see langword="true"/> if the angle is acute.</returns>
        public static bool IsAcute(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            // relies on fact that A dot B is positive if A ang B is acute
            double dx0 = p0.X - p1.X;
            double dy0 = p0.Y - p1.Y;
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dotprod = dx0 * dx1 + dy0 * dy1;
            return dotprod > 0;
        }

        /// <summary>
        /// Tests whether the angle between p0-p1-p2 is obtuse
        /// </summary>
        /// <remarks>
        /// <para>An angle is obtuse if it is greater than 90 degrees.</para>
        /// <para>Note: this implementation is not precise (deterministic) for angles very close to 90 degrees.</para>
        /// </remarks>
        /// <param name="p0">An endpoint of the angle</param>
        /// <param name="p1">The base of the angle</param>
        /// <param name="p2">Another endpoint of the angle</param>
        /// <returns><see langword="true"/> if the angle is obtuse.</returns>
        public static bool IsObtuse(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            // relies on fact that A dot B is negative if A ang B is obtuse
            double dx0 = p0.X - p1.X;
            double dy0 = p0.Y - p1.Y;
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dotprod = dx0 * dx1 + dy0 * dy1;
            return dotprod < 0;
        }

        /// <summary>
        /// Returns the unoriented smallest angle between two vectors.
        /// </summary>
        /// <remarks>
        /// The computed angle will be in the range [0, Pi).
        /// </remarks>
        /// <param name="tip1">The tip of one vector</param>
        /// <param name="tail">The tail of each vector</param>
        /// <param name="tip2">The tip of the other vector</param>
        public static double AngleBetween(Coordinate tip1, Coordinate tail, Coordinate tip2)
        {
            double a1 = Angle(tail, tip1);
            double a2 = Angle(tail, tip2);

            return Diff(a1, a2);
        }

        /// <summary>
        /// Returns the oriented smallest angle between two vectors.
        /// The computed angle will be in the range (-Pi, Pi].
        /// A positive result corresponds to a <see cref="OrientationIndex.CounterClockwise"/> rotation (CCW) from v1 to v2;
        /// a negative result corresponds to a <see cref="OrientationIndex.Clockwise"/> (CW) rotation;
        /// a zero result corresponds to no rotation.
        /// </summary>
        /// <param name="tip1">The tip of v1</param>
        /// <param name="tail">The tail of each vector</param>
        /// <param name="tip2">The tip of v2</param>
        /// <returns>The angle between v1 and v2, relative to v1</returns>
        public static double AngleBetweenOriented(Coordinate tip1, Coordinate tail, Coordinate tip2)
        {
            double a1 = Angle(tail, tip1);
            double a2 = Angle(tail, tip2);
            double angDel = a2 - a1;

            // normalize, maintaining orientation
            if (angDel <= -System.Math.PI)
                return angDel + PiTimes2;
            if (angDel > System.Math.PI)
                return angDel - PiTimes2;
            return angDel;
        }

        /// <summary>
        /// Computes the angle of the unoriented bisector
        /// of the smallest angle between two vectors.
        /// </summary>
        /// <remarks>The computed angle will be in the range (-Pi, Pi].</remarks>
        /// <param name="tip1">The tip of v1</param>
        /// <param name="tail">The tail of each vector</param>
        /// <param name="tip2">The tip of v2</param>
        /// <returns>The angle of the bisector between v1 and v2</returns>
        public static double Bisector(Coordinate tip1, Coordinate tail,
            Coordinate tip2)
        {
            double angDel = AngleBetweenOriented(tip1, tail, tip2);
            double angBi = Angle(tail, tip1) + angDel / 2;
            return Normalize(angBi);
        }

        /// <summary>
        /// Computes the interior angle between two segments of a ring.
        /// The ring is assumed to be oriented in a clockwise direction.
        /// </summary>
        /// <remarks>The computed angle will be in the range [0, 2Pi]</remarks>
        /// <param name="p0">A point of the ring</param>
        /// <param name="p1">The next point of the ring</param>
        /// <param name="p2">The next point of the ring</param>
        /// <returns>The interior angle based at <paramref name="p1"/></returns>
        public static double InteriorAngle(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            double anglePrev = Angle(p1, p0);
            double angleNext = Angle(p1, p2);
            return NormalizePositive(angleNext - anglePrev);
        }

        /// <summary>
        /// Returns whether an angle must turn clockwise or counterclockwise to overlap another angle.
        /// </summary>
        /// <param name="ang1">An angle (in radians)</param>
        /// <param name="ang2">An angle (in radians)</param>
        /// <returns>Whether a1 must turn <see cref="OrientationIndex.Clockwise"/>, <see cref="OrientationIndex.CounterClockwise"/> or <see cref="OrientationIndex.None"/> to overlap a2.</returns>
        public static OrientationIndex GetTurn(double ang1, double ang2)
        {
            double crossproduct = System.Math.Sin(ang2 - ang1);

            if (crossproduct > 0)
            {
                return OrientationIndex.CounterClockwise;
            }

            if (crossproduct < 0)
            {
                return OrientationIndex.Clockwise;
            }

            return OrientationIndex.None;
        }

        /// <summary>
        /// Computes the normalized value of an angle, which is the equivalent angle in the range ( -Pi, Pi ].
        /// </summary>
        /// <param name="angle">The angle to normalize</param>
        /// <returns>An equivalent angle in the range (-Pi, Pi]</returns>
        public static double Normalize(double angle)
        {
            while (angle > System.Math.PI)
                angle -= PiTimes2;
            while (angle <= -System.Math.PI)
                angle += PiTimes2;
            return angle;
        }

        /// <summary>
        /// Computes the normalized positive value of an angle, which is the equivalent angle in the range [ 0, 2*Pi ).
        /// <para/>
        /// E.g.
        /// <list type="table">
        /// <listheader><term>Function call</term><description>Result</description></listheader>
        /// <item><term>NormalizePositive(0.0)</term><description>0.0</description></item>
        /// <item><term>NormalizePositive(-PI)</term><description><see cref="System.Math.PI"/></description></item>
        /// <item><term>NormalizePositive(-2PI)</term><description>0.0</description></item>
        /// <item><term>NormalizePositive(-3PI)</term><description><see cref="System.Math.PI"/></description></item>
        /// <item><term>NormalizePositive(-4PI)</term><description>0.0</description></item>
        /// <item><term>NormalizePositive(PI)</term><description><see cref="System.Math.PI"/></description></item>
        /// <item><term>NormalizePositive(2PI)</term><description>0.0</description></item>
        /// <item><term>NormalizePositive(3PI)</term><description><see cref="System.Math.PI"/></description></item>
        /// <item><term>NormalizePositive(4PI)</term><description>0.0</description></item>
        /// </list>
        /// </summary>
        /// <remarks></remarks>
        /// <param name="angle">The angle to normalize, in radians.</param>
        /// <returns>An equivalent positive angle</returns>
        public static double NormalizePositive(double angle)
        {
            if (angle < 0.0)
            {
                while (angle < 0.0)
                    angle += PiTimes2;
                // in case round-off error bumps the value over
                if (angle >= PiTimes2)
                    angle = 0.0;
            }
            else
            {
                while (angle >= PiTimes2)
                    angle -= PiTimes2;
                // in case round-off error bumps the value under
                if (angle < 0.0)
                    angle = 0.0;
            }
            return angle;
        }

        /// <summary>
        /// Computes the unoriented smallest difference between two angles.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>The angles are assumed to be normalized to the range [-Pi, Pi].</description></item>
        /// <item><description>The result will be in the range [0, Pi].</description></item>
        /// </list>
        /// </remarks>
        /// <param name="ang1">The angle of one vector (in [-Pi, Pi] )</param>
        /// <param name="ang2">The angle of the other vector (in range [-Pi, Pi] )</param>
        /// <returns>The angle (in radians) between the two vectors (in range [0, Pi] )</returns>
        public static double Diff(double ang1, double ang2)
        {
            double delAngle;

            if (ang1 < ang2)
            {
                delAngle = ang2 - ang1;
            }
            else
            {
                delAngle = ang1 - ang2;
            }

            if (delAngle > System.Math.PI)
            {
                delAngle = PiTimes2 - delAngle;
            }

            return delAngle;
        }

        /// <summary>
        /// Computes <c>Math.Sin</c> of an angle, snapping near-zero values to zero.
        /// </summary>
        /// <param name="ang">The input angle (in radians)</param>
        /// <returns>The result of the trigonometric function</returns>
        public static double SinSnap(double ang)
        {
            double res = System.Math.Sin(ang);
            if (System.Math.Abs(res) < 5e-16) return 0.0;
            return res;
        }

        /// <summary>
        /// Computes <c>Math.Cos</c> of an angle, snapping near-zero values to zero.
        /// </summary>
        /// <param name="ang">The input angle (in radians)</param>
        /// <returns>The result of the trigonometric function</returns>
        public static double CosSnap(double ang)
        {
            double res = System.Math.Cos(ang);
            if (System.Math.Abs(res) < 5e-16) return 0.0;
            return res;
        }


        /// <summary>
        /// Projects a point by a given angle and distance.
        /// </summary>
        /// <param name="p">The point to project</param>
        /// <param name="angle">The angle at which to project</param>
        /// <param name="dist">The distance to project</param>
        /// <returns>The projected point</returns>
        public static Coordinate Project(Coordinate p, double angle, double dist)
        {
            double x = p.X + dist * CosSnap(angle);
            double y = p.Y + dist * SinSnap(angle);
            return new Coordinate(x, y);
        }
    }
}
