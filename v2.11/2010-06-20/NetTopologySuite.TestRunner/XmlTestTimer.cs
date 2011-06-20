using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace GisSharpBlog.NetTopologySuite
{
    [Obsolete("Use System.Diagnostics.Stopwatch")]
    public class XmlTestTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long startTime,
                     stopTime;

        private readonly long freq;

        // Constructor
        public XmlTestTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported 
                throw new Win32Exception();
            }
        }

        // Start the timer
        public void Start()
        {
            // lets do the waiting threads there work
            Thread.Sleep(0);

            QueryPerformanceCounter(out startTime);
        }

        // Stop the timer
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        // Returns the duration of the timer (in seconds)
        public Double Duration
        {
            get { return (stopTime - startTime)/(Double) freq; }
        }
    }
}