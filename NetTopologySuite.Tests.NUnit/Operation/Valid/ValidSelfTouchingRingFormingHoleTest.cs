using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Valid
{
    ///<summary>
    ///Tests allowing IsValidOp to validate polygons with
    ///Self-Touching Rings forming holes.
    ///Mainly tests that configuring <see cref="IsValidOp"/> to allow validating
    ///the STR validates polygons with this condition, and does not validate
    ///polygons with other kinds of self-intersection (such as ones with Disconnected Interiors).
    ///Includes some basic tests to confirm that other invalid cases remain detected correctly,
    ///but most of this testing is left to the existing XML validation tests.
    ///</summary>
    ///<author>Martin Davis</author>
    ///<version>1.7</version>
    [TestFixtureAttribute]
    public class ValidSelfTouchingRingFormingHoleTest
    {
        private static WKTReader rdr = new WKTReader();

        ///<summary>
        ///Tests a geometry with both a shell self-touch and a hole self=touch.
        ///This is valid if STR is allowed, but invalid in OGC
        ///</summary>
        [TestAttribute]
        public void TestShellAndHoleSelfTouch()
        {
            String wkt = "POLYGON ((0 0, 0 340, 320 340, 320 0, 120 0, 180 100, 60 100, 120 0, 0 0),   (80 300, 80 180, 200 180, 200 240, 280 200, 280 280, 200 240, 200 300, 80 300))";
            CheckIsValidSTR(wkt, true);
            CheckIsValidDefault(wkt, false);
        }

        ///<summary>
        ///Tests a geometry representing the same area as in <see cref="TestShellAndHoleSelfTouch"/>
        ///but using a shell-hole touch and a hole-hole touch.
        ///This is valid in OGC.
        ///</summary>
        [TestAttribute]
        public void TestShellHoleAndHoleHoleTouch()
        {
            String wkt = "POLYGON ((0 0, 0 340, 320 340, 320 0, 120 0, 0 0),   (120 0, 180 100, 60 100, 120 0),   (80 300, 80 180, 200 180, 200 240, 200 300, 80 300),  (200 240, 280 200, 280 280, 200 240))";
            CheckIsValidSTR(wkt, true);
            CheckIsValidDefault(wkt, true);
        }

        ///<summary>
        ///Tests an overlapping hole condition, where one of the holes is created by a shell self-touch.
        ///This is never vallid.
        ///</summary>
        [TestAttribute]
        public void TestShellSelfTouchHoleOverlappingHole()
        {
            String wkt = "POLYGON ((0 0, 220 0, 220 200, 120 200, 140 100, 80 100, 120 200, 0 200, 0 0),   (200 80, 20 80, 120 200, 200 80))";
            CheckIsValidSTR(wkt, false);
            CheckIsValidDefault(wkt, false);
        }

        ///<summary>
        ///Ensure that the Disconnected Interior condition is not validated
        ///</summary>
        [TestAttribute]
        public void TestDisconnectedInteriorShellSelfTouchAtNonVertex()
        {
            String wkt = "POLYGON ((40 180, 40 60, 240 60, 240 180, 140 60, 40 180))";
            CheckIsValidSTR(wkt, false);
            CheckIsValidDefault(wkt, false);
        }

        ///<summary>
        ///Ensure that the Disconnected Interior condition is not validated
        ///</summary>
        [TestAttribute]
        public void TestDisconnectedInteriorShellSelfTouchAtVertex()
        {
            String wkt = "POLYGON ((20 20, 20 100, 140 100, 140 180, 260 180, 260 100, 140 100, 140 20, 20 20))";
            CheckIsValidSTR(wkt, false);
            CheckIsValidDefault(wkt, false);
        }

        [TestAttribute]
        public void TestShellCross()
        {
            String wkt = "POLYGON ((20 20, 120 20, 120 220, 240 220, 240 120, 20 120, 20 20))";
            CheckIsValidSTR(wkt, false);
            CheckIsValidDefault(wkt, false);
        }

        [TestAttribute]
        public void TestShellCrossAndSTR()
        {
            String wkt = "POLYGON ((20 20, 120 20, 120 220, 180 220, 140 160, 200 160, 180 220, 240 220, 240 120, 20 120,  20 20))";
            CheckIsValidSTR(wkt, false);
            CheckIsValidDefault(wkt, false);
        }

        private void CheckIsValidDefault(String wkt, bool expected)
        {
            IGeometry geom = FromWKT(wkt);
            IsValidOp validator = new IsValidOp(geom);
            bool isValid = validator.IsValid;
            Assert.IsTrue(isValid == expected);
        }

        private void CheckIsValidSTR(String wkt, bool expected)
        {
            IGeometry geom = FromWKT(wkt);
            IsValidOp validator = new IsValidOp(geom);
            validator.IsSelfTouchingRingFormingHoleValid = true;
            bool isValid = validator.IsValid;
            Assert.IsTrue(isValid == expected);
        }

        IGeometry FromWKT(String wkt)
        {
            IGeometry geom = null;
            try
            {
                geom = rdr.Read(wkt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return geom;
        }
    }

}