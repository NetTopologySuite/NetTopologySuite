using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Extracts all the 1-dimensional (<c>LineString</c>) components from a <c>Geometry</c>.
    /// </summary>
    public class LinearComponentExtracter : IGeometryComponentFilter
    {
        /// <summary>
        /// Extracts the linear components from a <see cref="ICollection{IGeometry}"/>
        /// and adds them to the provided <see cref="ICollection{ILineString}"/>.
        /// </summary>
        /// <param name="geoms">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static ICollection<ILineString> GetLines(ICollection<IGeometry> geoms, ICollection<ILineString> lines)
        {
            foreach (IGeometry g in geoms)
            {
                GetLines(g, lines);
            }
            return lines;
        }

        /// <summary>
        /// Extracts the linear components from a <see cref="ICollection{IGeometry}"/>
        /// and adds them to the provided <see cref="ICollection{ILineString}"/>.
        /// </summary>
        /// <param name="geoms">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <param name="forceToLineString"></param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static ICollection<ILineString> GetLines(ICollection<IGeometry> geoms, ICollection<ILineString> lines, bool forceToLineString)
        {
            foreach (IGeometry g in geoms)
            {
                GetLines(g, lines, forceToLineString);
            }
            return lines;
        }
        /// <summary>
        /// Extracts the linear components from a single <see cref="IGeometry"/>
        /// and adds them to the provided <see cref="ICollection{ILineString}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static ICollection<ILineString> GetLines(IGeometry geom, ICollection<ILineString> lines)
        {
            if (geom is ILineString)
            {
                lines.Add((ILineString) geom);
            }
            else
            {
                geom.Apply(new LinearComponentExtracter(lines));
            }
            return lines;
        }

        /// <summary>
        /// Extracts the linear components from a single <see cref="IGeometry"/>
        /// and adds them to the provided <see cref="ICollection{ILineString}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract linear components</param>
        /// <param name="lines">The Collection to add the extracted linear components to</param>
        /// <param name="forceToLineString"></param>
        /// <returns>The Collection of linear components (LineStrings or LinearRings)</returns>
        public static ICollection<ILineString> GetLines(IGeometry geom, ICollection<ILineString> lines, bool forceToLineString)
        {
            geom.Apply(new LinearComponentExtracter(lines, forceToLineString));
            return lines;
        }


        /// <summary> 
        /// Extracts the linear components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <c>LineExtracterFilter</c> instance
        /// and pass it to multiple geometries.
        /// </summary>
        /// <param name="geom">The point from which to extract linear components.</param>
        /// <returns>The list of linear components.</returns>
        public static ICollection<ILineString> GetLines(IGeometry geom)
        {
            ICollection<ILineString> lines = new Collection<ILineString>();
            geom.Apply(new LinearComponentExtracter(lines));
            return lines;
        }

        private readonly ICollection<ILineString> _lines;

        /// <summary> 
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        /// <param name="lines"></param>
        public LinearComponentExtracter(ICollection<ILineString> lines)
            :this(lines, false)
        {
        }

        /// <summary> 
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="isForcedToLineString"></param>
        public LinearComponentExtracter(ICollection<ILineString> lines, bool isForcedToLineString)
        {
            _lines = lines;
            IsForcedToLineString = isForcedToLineString;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsForcedToLineString { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(IGeometry geom)
        {
  	        if (IsForcedToLineString && geom is ILinearRing)
            {
  		        ILineString line = geom.Factory.CreateLineString( ((ILinearRing) geom).CoordinateSequence);
  		        _lines.Add(line);
  		        return;
  	        }
  	        // if not being forced, and this is a linear component
  	        if (geom is ILineString)
                _lines.Add((ILineString)geom);
  	
  	        // else this is not a linear component, so skip it
        }
    }
}
