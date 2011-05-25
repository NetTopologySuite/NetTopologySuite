namespace NetTopologySuite.Data.InternalImpl
{
    internal class StringPropertyInfo : PropertyInfo<string>, IStringPropertyInfo
    {
        internal StringPropertyInfo(IPropertyInfoFactory factory, string name)
            : base(factory, name)
        {
        }

        public int MaxLength
        {
            get;
            set;
        }
    }
}