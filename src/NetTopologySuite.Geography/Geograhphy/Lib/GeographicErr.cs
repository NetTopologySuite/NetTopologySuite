using System;

namespace NetTopologySuite.Geography.Lib
{
    /**
     * Exception handling for GeographicLib.
     * <p>
     * A class to handle exceptions.  It's derived from RuntimeException so it
     * can be caught by the usual catch clauses.
     **********************************************************************/
    public class GeographicErr : Exception
    {
        /**
   * Constructor
   * <p>
   * @param msg a string message, which is accessible in the catch
   *   clause via getMessage().
   **********************************************************************/
        public GeographicErr(string msg) : base(msg) { }
    }
}
