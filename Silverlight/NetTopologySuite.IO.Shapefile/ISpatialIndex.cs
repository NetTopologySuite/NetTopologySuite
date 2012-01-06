using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shapefile
{
    public interface ISpatialIndex<TKey, TValue> :IDisposable
    {
        IEnumerable<TValue> Query(TKey bounds);

        TKey Bounds { get; }

        void Insert(TKey key, TValue value);
    }


    public class FakeSpatialIndex<TValue> : ISpatialIndex<IEnvelope, TValue>
    {

        private readonly IDictionary<IEnvelope, TValue> _dictionary = new Dictionary<IEnvelope, TValue>();

        #region ISpatialIndex<IEnvelope,TValue> Members

        public IEnumerable<TValue> Query(IEnvelope bounds)
        {
            return
                _dictionary.Keys.Where(envelope => envelope.Intersects(bounds)).Select(envelope => _dictionary[envelope]);
        }

        #endregion


        public IEnvelope Bounds
        {
            get;
            protected set;
        }


        public void Insert(IEnvelope key, TValue value)
        {
            _dictionary.Add(key, value);
            IEnvelope env = Bounds != null ? (IEnvelope)Bounds.Clone() : (IEnvelope)key.Clone();
            if (Bounds != null)
                env.ExpandToInclude(key);
            Bounds = env;
        }

        public void Dispose()
        {
           
        }
    }
}