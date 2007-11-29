using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Index
{
    public class ArrayListVisitor : IItemVisitor
    {
        private ArrayList items = new ArrayList();

        public void VisitItem(object item)
        {
            items.Add(item);
        }

        public ArrayList Items
        {
            get { return items; }
        }
    }
}