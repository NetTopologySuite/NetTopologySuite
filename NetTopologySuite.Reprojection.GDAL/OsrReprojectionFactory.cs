using System;
using System.Collections.Generic;

namespace NetTopologySuite.Reprojection
{
    internal class OsrReprojectionFactory : ReprojectionFactory, IDisposable
    {
        private readonly Dictionary<Tuple<SpatialReference, SpatialReference>, OsrReprojection> _cache = new Dictionary<Tuple<SpatialReference, SpatialReference>, OsrReprojection>();

        public override Reprojection Create(SpatialReference source, SpatialReference target, bool cache = false)
        {
            OsrReprojection res = null;
            var key = Tuple.Create(source, target);
            if (cache)
            {
                if (_cache.TryGetValue(key, out res))
                {
                    if (!res.IsDisposed) return res;
                    _cache.Remove(key);
                }
            }

            res = new OsrReprojection(source, target);
            if (cache)
                _cache[key] = res;

            return res;
        }

        void IDisposable.Dispose()
        {
            foreach (var value in _cache.Values)
                ((IDisposable)value).Dispose();
            _cache.Clear();
            
        }
    }
}