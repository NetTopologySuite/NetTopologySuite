using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Shapefile
{
    public abstract class FeatureProviderBase : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }
}
