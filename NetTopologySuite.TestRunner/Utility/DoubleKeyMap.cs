using System.Collections.Generic;

namespace Open.Topology.TestRunner.Utility
{
    public class DoubleKeyMap<TKey1, TKey2, TValue> 
    {
        internal IDictionary<TKey1, IDictionary<TKey2, TValue>> Data = new Dictionary<TKey1, IDictionary<TKey2, TValue>>();

        public virtual TValue Put(TKey1 k1, TKey2 k2, TValue v) {
            IDictionary<TKey2, TValue> data2;
            Data.TryGetValue(k1, out data2);
            TValue prev = default(TValue);
            if (data2 == null) {
                data2 = new Dictionary<TKey2, TValue>();
                Data[k1] = data2;
            } else {
                data2.TryGetValue(k2, out prev);
            }
            data2[k2] = v;
            return prev;
        }

        public virtual TValue Get(TKey1 k1, TKey2 k2) {
            IDictionary<TKey2, TValue> data2;
            Data.TryGetValue(k1, out data2);
            if (data2 == null)
                return default(TValue);

            TValue value;
            data2.TryGetValue(k2, out value);
            return value;
        }

        public virtual IDictionary<TKey2, TValue> Get(TKey1 k1) {
            IDictionary<TKey2, TValue> value;
            Data.TryGetValue(k1, out value);
            return value;
        }

        /** Get all values associated with primary key */
        public virtual ICollection<TValue> Values(TKey1 k1) {
            IDictionary<TKey2, TValue> data2;
            Data.TryGetValue(k1, out data2);
            if (data2 == null)
                return null;

            return data2.Values;
        }

        /** get all primary keys */
        public virtual ICollection<TKey1> KeySet() {
            return Data.Keys;
        }

        /** get all secondary keys associated with a primary key */
        public virtual ICollection<TKey2> KeySet(TKey1 k1) {
            IDictionary<TKey2, TValue> data2;
            Data.TryGetValue(k1, out data2);
            if (data2 == null)
                return null;

            return data2.Keys;
        }

        public virtual ICollection<TValue> Values() {
            List<TValue> s = new List<TValue>();
            foreach (IDictionary<TKey2, TValue> k2 in Data.Values) {
                foreach (TValue v in k2.Values) {
                    s.Add(v);
                }
            }
            return s;
        }
    }
}
