using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Coordinates;

namespace ManagedBufferedCoordinate2DTests
{
    public class Buffered2DEnumeratorHelper : IEnumerable<BufferedCoordinate2D>
    {
        private IEnumerable<BufferedCoordinate2D> _backingObject;

        public Buffered2DEnumeratorHelper(IEnumerable<BufferedCoordinate2D> backingObject)
        {
            _backingObject = backingObject;
        }


        #region IEnumerable<BufferedCoordinate2D> Members

        public IEnumerator<BufferedCoordinate2D> GetEnumerator()
        {
            return _backingObject.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _backingObject.GetEnumerator();
        }

        #endregion
    }
}
