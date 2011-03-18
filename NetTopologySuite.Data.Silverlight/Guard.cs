using System;

namespace GisSharpBlog.NetTopologySuite.Data
{

    public static class Guard
    {
        public static void IsNotNull(object candidate, string propertyName)
        {
            if (candidate == null)
                throw new ArgumentNullException(propertyName);
        }
    }

}
