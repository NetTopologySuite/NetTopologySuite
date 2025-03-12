using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Operation.RelateNG
{
    internal class NodeSections
    {

        private readonly Coordinate _nodePt;

        private readonly List<NodeSection> _sections = new List<NodeSection>();

        public NodeSections(Coordinate pt)
        {
            _nodePt = pt;
        }

        public Coordinate Coordinate { get { return _nodePt; } }

        public void AddNodeSection(NodeSection e)
        {
            //System.out.println(e);
            _sections.Add(e);
        }

        public bool HasInteractionAB
        {
            get
            {
                bool isA = false;
                bool isB = false;
                foreach (var ns in _sections)
                {
                    if (ns.IsA)
                        isA = true;
                    else
                        isB = true;
                    if (isA && isB)
                        return true;
                }
                return false;
            }
        }


        public Geometry GetPolygonal(bool isA)
        {
            foreach (var ns in _sections)
            {
                if (ns.IsA == isA)
                {
                    var poly = ns.Polygonal;
                    if (poly != null)
                        return poly;
                }
            }
            return null;
        }

        public RelateNode CreateNode()
        {
            PrepareSections();

            var node = new RelateNode(_nodePt);
            int i = 0;
            while (i < _sections.Count)
            {
                var ns = _sections[i];
                //-- if there multiple polygon sections incident at node convert them to maximal-ring structure 
                if (ns.IsArea && HasMultiplePolygonSections(_sections, i))
                {
                    var polySections = CollectPolygonSections(_sections, i);
                    var nsConvert = PolygonNodeConverter.Convert(polySections);
                    node.AddEdges(nsConvert);
                    i += polySections.Count;
                }
                else
                {
                    //-- the most common case is a line or a single polygon ring section
                    node.AddEdges(ns);
                    i += 1;
                }
            }
            return node;
        }

        /// <summary>
        /// Sorts the sections so that:
        /// <list type="bullet">
        /// <item><description>lines are before areas</description></item>
        /// <item><description>edges from the same polygon are contiguous</description></item>
        /// </list>
        /// </summary>
        private void PrepareSections()
        {
            _sections.Sort();
            //TODO: remove duplicate sections
        }

        private static bool HasMultiplePolygonSections(List<NodeSection> sections, int i)
        {
            //-- if last section can only be one
            if (i >= sections.Count - 1)
                return false;
            //-- check if there are at least two sections for same polygon
            var ns = sections[i];
            var nsNext = sections[i + 1];
            return ns.IsSamePolygon(nsNext);
        }

        private static List<NodeSection> CollectPolygonSections(List<NodeSection> sections, int i)
        {
            var polySections = new List<NodeSection>();
            //-- note ids are only unique to a geometry
            var polySection = sections[i];
            while (i < sections.Count &&
                polySection.IsSamePolygon(sections[i]))
            {
                polySections.Add(sections[i]);
                i++;
            }
            return polySections;
        }

    }

}
