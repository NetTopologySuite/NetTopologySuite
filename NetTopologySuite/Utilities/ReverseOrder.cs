using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
	internal class ReverseOrder :IComparer
	{
		public ReverseOrder() { }

		public Int32 Compare(object x, object y)
		{
			// flips result
			return Comparer.Default.Compare(x, y) * -1;
		}

	}
}
