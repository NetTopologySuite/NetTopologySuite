using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Utilities
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
