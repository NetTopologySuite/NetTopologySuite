using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class NTSFunctions
    {
        private static readonly double HEIGHT = 70;
        private static readonly double WIDTH = 150; //125;
        private static readonly double J_WIDTH = 30;
        private static readonly double J_RADIUS = J_WIDTH - 5;

        private static readonly double S_RADIUS = HEIGHT / 4;

        private static readonly double T_WIDTH = WIDTH - 2 * S_RADIUS - J_WIDTH;

        public static Geometry LogoLines(Geometry g)
        {
            return CreateJ(g)
                .Union(CreateT(g))
                .Union(CreateS(g));
        }

        public static Geometry LogoBuffer(Geometry g, double distance)
        {
            var lines = LogoLines(g);
            var bufParams = new BufferParameters();
            bufParams.EndCapStyle = EndCapStyle.Square;
            return BufferOp.Buffer(lines, distance, bufParams);
        }

        private static Geometry CreateJ(Geometry g)
        {
            var gf = FunctionsUtil.GetFactoryOrDefault(g);

            var jTop = new Coordinate[]
                           {
                               new Coordinate(0, HEIGHT),
                               new Coordinate(J_WIDTH, HEIGHT),
                               new Coordinate(J_WIDTH, J_RADIUS)
                           };
            var jBottom = new Coordinate[]
                              {
                                  new Coordinate(J_WIDTH - J_RADIUS, 0),
                                  new Coordinate(0, 0)
                              };

            var gsf = new GeometricShapeFactory(gf);
            gsf.Base = new Coordinate(J_WIDTH - 2 * J_RADIUS, 0);
            gsf.Size = 2 * J_RADIUS;
            gsf.NumPoints = 10;
            var jArc = gsf.CreateArc(1.5 * Math.PI, 0.5 * Math.PI);

            var coordList = new CoordinateList();
            coordList.Add(jTop, false);
            coordList.Add(((Geometry)jArc).Reverse().Coordinates, false, 1, jArc.NumPoints - 1);
            coordList.Add(jBottom, false);

            return gf.CreateLineString(coordList.ToCoordinateArray());
        }

        private static Geometry CreateT(Geometry g)
        {
            var gf = FunctionsUtil.GetFactoryOrDefault(g);

            var tTop = new[]
                           {
                               new Coordinate(J_WIDTH, HEIGHT),
                               new Coordinate(WIDTH - S_RADIUS - 5, HEIGHT)
                           };
            var tBottom = new[]
                              {
                                  new Coordinate(J_WIDTH + 0.5*T_WIDTH, HEIGHT),
                                  new Coordinate(J_WIDTH + 0.5*T_WIDTH, 0)
                              };
            var lines = new[]
                            {
                                gf.CreateLineString(tTop),
                                gf.CreateLineString(tBottom)
                            };
            return gf.CreateMultiLineString(lines);
        }

        private static Geometry CreateS(Geometry g)
        {
            var gf = FunctionsUtil.GetFactoryOrDefault(g);

            double centreX = WIDTH - S_RADIUS;

            var top = new[]
                          {
                              new Coordinate(WIDTH, HEIGHT),
                              new Coordinate(centreX, HEIGHT)
                          };
            var bottom = new[]
                             {
                                 new Coordinate(centreX, 0),
                                 new Coordinate(WIDTH - 2*S_RADIUS, 0)
                             };

            var gsf = new GeometricShapeFactory(gf);
            gsf.Centre = new Coordinate(centreX, HEIGHT - S_RADIUS);
            gsf.Size = 2 * S_RADIUS;
            gsf.NumPoints = 10;
            var arcTop = gsf.CreateArc(0.5 * Math.PI, Math.PI);

            var gsf2 = new GeometricShapeFactory(gf);
            gsf2.Centre = new Coordinate(centreX, S_RADIUS);
            gsf2.Size = 2 * S_RADIUS;
            gsf2.NumPoints = 10;
            var arcBottom = (LineString)((Geometry)gsf2.CreateArc(1.5 * Math.PI, Math.PI)).Reverse();

            var coordList = new CoordinateList();
            coordList.Add(top, false);
            coordList.Add(arcTop.Coordinates, false, 1, arcTop.NumPoints - 1);
            coordList.Add(new Coordinate(centreX, HEIGHT / 2));
            coordList.Add(arcBottom.Coordinates, false, 1, arcBottom.NumPoints - 1);
            coordList.Add(bottom, false);

            return gf.CreateLineString(coordList.ToCoordinateArray());
        }
    }
}