using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Data
{
    public interface IMemoryRecordSet : IList<IRecord>
    {
        ISchema Schema { get; }

    }

    public class MemoryRecordSet : IMemoryRecordSet
    {
        private readonly List<IRecord> _records;
        private readonly ISchema _schema;

        internal MemoryRecordSet(ISchema schema)
            : this(schema, new IRecord[] { })
        {

        }

        public MemoryRecordSet(ISchema schema, IEnumerable<IRecord> records)
        {
            Guard.IsNotNull(schema, "schema");
            Guard.IsNotNull(records, "records");

            _schema = schema;
            List<IRecord> trecs = records.ToList();
            trecs.ForEach(a => CheckSchemaMatches(a.Schema));
            _records = trecs;
        }

        #region IMemoryRecordSet Members

        public ISchema Schema
        {
            get { return _schema; }
        }

        public int IndexOf(IRecord item)
        {
            CheckSchemaMatches(item.Schema);
            return _records.IndexOf(item);
        }

        public void Insert(int index, IRecord item)
        {
            CheckSchemaMatches(item.Schema);
            _records.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _records.RemoveAt(index);
        }

        public IRecord this[int index]
        {
            get { return _records[index]; }
            set
            {
                CheckSchemaMatches(value.Schema);
                _records[index] = value;
            }
        }

        public void Add(IRecord item)
        {
            CheckSchemaMatches(item.Schema);
            _records.Add(item);
        }

        public void Clear()
        {
            _records.Clear();
        }

        public bool Contains(IRecord item)
        {
            CheckSchemaMatches(item.Schema);
            return _records.Contains(item);
        }

        public void CopyTo(IRecord[] array, int arrayIndex)
        {
            _records.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _records.Count; }
        }

        bool ICollection<IRecord>.IsReadOnly
        {
            get { return ((ICollection<IRecord>)_records).IsReadOnly; }
        }

        public bool Remove(IRecord item)
        {
            CheckSchemaMatches(item.Schema);
            return _records.Remove(item);
        }

        public IEnumerator<IRecord> GetEnumerator()
        {
            return _records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private void CheckSchemaMatches(ISchema other)
        {
            if (!Schema.Equals(other))
                throw new SchemaMismatchException();
        }
    }
}