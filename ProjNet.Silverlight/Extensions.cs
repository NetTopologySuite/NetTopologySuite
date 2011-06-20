using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;

namespace ProjNet.CoordinateSystems
{
	/// <summary>
	/// Silverlight Extension methods
	/// </summary>
	public static class SilverlightExtensions
	{
		/// <summary>
		/// Transforms a coordinate point.
		/// </summary>
		/// <param name="point">Input point</param>
		/// <returns>Transformed point</returns>
		public static System.Windows.Point Transform(this IMathTransform transform, System.Windows.Point point)
		{
			double[] p = transform.Transform(new double[] { point.X, point.Y });
			if (p == null || p.Length < 2) return new System.Windows.Point(double.NaN, double.NaN);
			return new System.Windows.Point(p[0], p[1]);
		}

		internal static Parameter Find(this List<Parameter> items, Predicate<Parameter> match)
		{
			foreach (Parameter item in items)
			{
				if (match(item))
					return item;
			}
			return null;
		}

		internal static ProjectionParameter Find(this List<ProjectionParameter> items, Predicate<ProjectionParameter> match)
		{
			foreach (ProjectionParameter item in items)
			{
				if (match(item))
					return item;
			}
			return null;
		}

		//public static T Find(this List<T> items, Predicate<T> match)
		//{
		//    foreach (T item in items)
		//    {
		//        if (match(item))
		//            return item;
		//    }
		//    return default(T);
		//}
	}
}
