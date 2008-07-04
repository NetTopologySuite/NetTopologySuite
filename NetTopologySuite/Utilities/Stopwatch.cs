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

        /// <summary>
        /// 
        /// </summary>
        public Stopwatch()
        {
            startTime = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            startTime = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Time
        {
            get
            {
                DateTime endTime = DateTime.Now;
                TimeSpan totalTime = endTime - startTime;
                return totalTime.Milliseconds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public String TimeAsString
        {
            get
            {
                int totalTime = Time;
                string totalTimeStr = totalTime < 10000 ? totalTime + " ms" : (double)totalTime / 1000.0 + " s";
                return totalTimeStr;
            }
        }
    }
}
