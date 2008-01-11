using System;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    internal class ReverseOrder<TItem> : IComparer<TItem>
	{
		public Int32 Compare(TItem x, TItem y)
		{
			// flips result
			return Comparer<TItem>.Default.Compare(x, y) * -1;
		}

	}
}
