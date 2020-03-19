using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.HPRtree
{
    public class Item<T>
    {
        public Item(Envelope env, T item)
        {
            Envelope = env;
            Value = item;
        }

        public Envelope Envelope { get; }

        /// <remarks>
        /// This property is named Item in JTS
        /// </remarks>
        public T Value { get; }

        ///<inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return $"Item: {Envelope}";
        }
    }
}
