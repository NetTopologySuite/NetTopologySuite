using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// Implements a timer function which can compute
    /// elapsed time as well as split times.
    /// </summary>
    public class Stopwatch
    {
        private DateTime startTime;

        public Stopwatch()
        {
            startTime = DateTime.Now;
        }

        public void Start()
        {
            startTime = DateTime.Now;
        }

        public Int32 Time
        {
            get
            {
                DateTime endTime = DateTime.Now;
                TimeSpan totalTime = endTime - startTime;
                return totalTime.Milliseconds;
            }
        }

        public String TimeAsString
        {
            get
            {
                Int32 totalTime = Time;
                string totalTimeStr = totalTime < 10000 ? totalTime + " ms" : (Double)totalTime / 1000.0 + " s";
                return totalTimeStr;
            }
        }
    }
}
