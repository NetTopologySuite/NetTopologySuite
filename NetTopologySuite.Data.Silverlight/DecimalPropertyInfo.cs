namespace GisSharpBlog.NetTopologySuite.Data
{
    internal class DecimalPropertyInfo : PropertyInfo<decimal>, IDecimalPropertyInfo
    {
        internal DecimalPropertyInfo(IPropertyInfoFactory factory, string name)
            : base(factory, name)
        {
        }

        public int Precision
        {
            get;
            set;
        }


    }
}