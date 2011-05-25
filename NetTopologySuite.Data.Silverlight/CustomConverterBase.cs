using System;

namespace NetTopologySuite.Data
{
    public abstract class CustomConverterBase<TFrom, TTo> : ICustomConverter<TFrom, TTo>
    {
        #region ICustomConverter<TFrom,TTo> Members

        public abstract TTo Convert(TFrom source);

        public Type TargetType
        {
            get { return typeof (TTo); }
        }

        public Type SourceType
        {
            get { return typeof (TFrom); }
        }

        public object Convert(object source)
        {
            return Convert((TFrom) source);
        }

        #endregion
    }
}