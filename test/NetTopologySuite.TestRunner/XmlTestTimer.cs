using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace Open.Topology.TestRunner
{
    public class XmlTestTimer
    {
        private long startTime, stopTime;
        private long freq;

        // Constructor
        public XmlTestTimer()
        {
            startTime = 0;
            stopTime  = 0;

            if (!Stopwatch.IsHighResolution)
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

            startTime = Stopwatch.GetTimestamp();
        }

        // Stop the timer
        public void Stop()
        {
            stopTime = Stopwatch.GetTimestamp();
        }

        // Returns the duration of the timer (in seconds)
        public double Duration => (double)(stopTime - startTime) / (double) freq;
    }
}
