using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
	/// <summary>
	/// 
	/// </summary>
	internal class ReverseOrder :IComparer
	{		
        /// <summary>
        /// 
        /// </summary>
		public ReverseOrder() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
		public int Compare(object x, object y)
		{
			// flips result
			return Comparer.Default.Compare(x, y) * -1;
		}

	}
}
