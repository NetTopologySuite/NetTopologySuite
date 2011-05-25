using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Shapefile
{
    public abstract class FeatureProviderBase : IDisposable
    {

        public bool IsDisposed
        {
            get;
            protected set;
        }

        public void Dispose()
        {

            if (IsDisposed)
                return;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}
