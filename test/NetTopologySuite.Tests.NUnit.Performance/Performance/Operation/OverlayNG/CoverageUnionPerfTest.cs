using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayNG
{
    /// <summary>
    /// Shows how linear performance of <see cref="GeometryCollection.Dimension"/>
    /// affects performance.
    /// (See https://github.com/locationtech/jts/issues/1100)
    /// </summary>
    /// <author>Martin Davis</author>
    [Category("LongRunning")]
    public class CoverageUnionPerfTest : PerformanceTestCase
    {
        private Geometry _grid;

        public CoverageUnionPerfTest() : base(nameof(CoverageUnionPerfTest))
        {
            RunSize = new[] {10_000, 20_000, 40_000, 100_000, 200_000, 400_000};
        }

        public override void StartRun(int nCells)
        {
            _grid = CreateGrid(100.0, nCells, new GeometryFactory());
            TestContext.WriteLine("\n-------  Running with cells = " + nCells);
        }

        private static Geometry CreateGrid(double size, int nCells, GeometryFactory geomFact)
        {
            int nCellsOnSideY = (int)System.Math.Sqrt(nCells);
            int nCellsOnSideX = nCells / nCellsOnSideY;

            double cellSizeX = size / nCellsOnSideX;
            double cellSizeY = size / nCellsOnSideY;

            var geoms = new List<Geometry>();

            for (int i = 0; i < nCellsOnSideX; i++)
            {
                for (int j = 0; j < nCellsOnSideY; j++)
                {
                    double x = 0 + i * cellSizeX;
                    double y = 0 + j * cellSizeY;
                    double x2 = 0 + (i + 1) * cellSizeX;
                    double y2 = 0 + (j + 1) * cellSizeY;

                    var cellEnv = new Envelope(x, x2, y, y2);
                    geoms.Add(geomFact.ToGeometry(cellEnv));
                }
            }
            return geomFact.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geoms));
        }

        public void RunUnion()
        {
            CoverageUnion.Union(_grid);
        }
    }
}
