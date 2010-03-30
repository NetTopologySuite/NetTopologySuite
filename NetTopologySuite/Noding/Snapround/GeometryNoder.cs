using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding.Snapround
{
    ///<summary>
    /// Nodes a list of <see cref="IGeometry{TCoordiante}"/>s using Snap Rounding
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class GeometryNoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                IComparable<TCoordinate>, IConvertible,
                IComputable<Double, TCoordinate>
    {
        private IGeometryFactory<TCoordinate> _geomFact;
        private IPrecisionModel<TCoordinate> _pm;
        private Boolean _isValidityChecked;

        ///<summary>
        /// Creates an instance of this class
        ///</summary>
        ///<param name="pm">Precision model</param>
        public GeometryNoder(IPrecisionModel<TCoordinate> pm)
        {
            _pm = pm;
        }

        ///<summary>
        /// 
        ///</summary>
        public Boolean Validate
        {
            get { return _isValidityChecked; }
            set { _isValidityChecked = value; }
        }

        /**
         * Nodes the linework of a set of Geometrys using SnapRounding. 
         * 
         * @param geoms a Collection of Geometrys of any type
         * @return a List of LineStrings representing the noded linework of the input
         */
        public List<IGeometry<TCoordinate>> Node(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            // get geometry factory
            IGeometry<TCoordinate> geom0 = Slice.GetFirst(geoms);
            _geomFact = geom0.Factory;

            IEnumerable<ISegmentString<TCoordinate>> segStrings = ToSegmentStrings(ExtractLines(geoms));
            //Noder sr = new SimpleSnapRounder(_pm);
            INoder<TCoordinate> sr = new MonotoneChainIndexSnapRounder<TCoordinate>(_geomFact);
            //sr.ComputeNodes(segStrings);
            //Collection nodedLines = sr.getNodedSubstrings();
            IEnumerable<ISegmentString<TCoordinate>> nodedLines = sr.Node(segStrings);

            if (_isValidityChecked)
            {
                NodingValidator<TCoordinate> nv = new NodingValidator<TCoordinate>(_geomFact, nodedLines);
                nv.CheckValid();
            }

            return ToLineStrings(nodedLines);
        }

        private List<IGeometry<TCoordinate>> ToLineStrings(IEnumerable<ISegmentString<TCoordinate>> segStrings)
        {
            List<IGeometry<TCoordinate>> lines = new List<IGeometry<TCoordinate>>();
            foreach (ISegmentString<TCoordinate> segString in segStrings)
            {
                if (segString.Count < 2)
                    continue;
                lines.Add(_geomFact.CreateLineString(segString.Coordinates));
            }
            return lines;
        }

        private IEnumerable<ILineString<TCoordinate>> ExtractLines(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            List<ILineString<TCoordinate>> lines = new List<ILineString<TCoordinate>>();
            LinearComponentExtracter<TCoordinate> lce = new LinearComponentExtracter<TCoordinate>(lines);

            Enumerable.Apply(lines, lce.Filter);
            return lines;
        }

        private static IEnumerable<ISegmentString<TCoordinate>> ToSegmentStrings(IEnumerable<ILineString<TCoordinate>> lines)
        {
            /*List segStrings = new ArrayList();
            for (Iterator it = lines.iterator(); it.hasNext(); ) {
              LineString line = (LineString) it.next();
              segStrings.add(new NodedSegmentString(line.getCoordinates(), null));
            }
            return segStrings;
             */
            foreach (ILineString<TCoordinate> line in lines)
            {
                yield return new NodedSegmentString<TCoordinate>(line.Coordinates, null);
            }
        }
    }
}