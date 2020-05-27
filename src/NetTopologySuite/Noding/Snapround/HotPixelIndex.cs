using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

namespace NetTopologySuite.Noding.Snapround
{
    /**
     * An index which creates {@link HotPixel}s for provided points,
     * and allows performing range queries on them.
     * 
     * @author mdavis
     *
     */
    class HotPixelIndex
    {
        private readonly PrecisionModel _precModel;
        private readonly double _scaleFactor;

        /**
         * Use a kd-tree to index the pixel centers for optimum performance.
         * Since HotPixels have an extent, queries to the
         * index must enlarge the query range by a suitable value 
         * (using the pixel width is safest).
         */
        private readonly KdTree<HotPixel> _index = new KdTree<HotPixel>();

        public HotPixelIndex(PrecisionModel pm)
        {
            _precModel = pm;
            _scaleFactor = pm.Scale;
        }

        public void Add(IEnumerable<Coordinate> pts)
        {
            foreach (var pt in pts)
            {
                Add(pt);
            }
        }

        public HotPixel Add(Coordinate p)
        {
            // TODO: is there a faster way of doing this?
            var pRound = Round(p);

            var hp = Find(p);
            if (hp != null)
                return hp;

            // not found, so create a new one
            hp = new HotPixel(pRound, _scaleFactor);
            _index.Insert(hp.Coordinate, hp);
            return hp;
        }

        private HotPixel Find(Coordinate pixelPt)
        {
            var kdNode = _index.Query(pixelPt);
            if (kdNode == null)
                return null;
            return kdNode.Data;
        }

        private Coordinate Round(Coordinate pt)
        {
            var p2 = pt.Copy();
            _precModel.MakePrecise(p2);
            return p2;
        }

        public void Query(Coordinate p0, Coordinate p1, IKdNodeVisitor<HotPixel> visitor)
        {
            var queryEnv = new Envelope(p0, p1);
            // expand query range to account for HotPixel extent
            // expand by full width of one pixel to be safe
            queryEnv.ExpandBy(1.0 / _scaleFactor);
            _index.Query(queryEnv, visitor);
        }
    }
}
