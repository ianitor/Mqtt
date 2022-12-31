using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using NLog;

namespace MqttBenchmark
{
    /// <summary>
    /// Performance manager implementation
    /// </summary>
    public class PerfManager
    {
        private readonly ConcurrentDictionary<string, PerfItem> _items;
        private static PerfManager _inst;
        private readonly char _separator = ';';

        /// <summary>
        /// Returns the single instance of the performance manager
        /// </summary>
        public static PerfManager Instance => _inst ?? (_inst = new PerfManager());

        private PerfManager()
        {
            _items = new ConcurrentDictionary<string, PerfItem>();
        }

        internal PerfItem CreateMeasurement(string methodName)
        {
            var itm = _items.AddOrUpdate(methodName, (s)=> new PerfItem(s), (s, item) => item);
            return itm;
        }

        /// <summary>
        /// Writes logging information to a logger
        /// </summary>
        /// <param name="logger"></param>
        //   [Conditional("PERF")]
        public void WriteLogging(Logger logger)
        {
            logger.Info("Performance analysis");
            logger.Info(WriteStatistics());
        }

        private string WriteStatistics()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Header
            // string x = "Full member name" + _separator;
            // x += "Checkpoint (cp)" + _separator;
            // x += "Total exec." + _separator;
            // x += "Time 1st run" + _separator;
            // x += "Total time 1st run exec." + _separator;
            // x += "1st run exec. prev. cp" + _separator;
            // x += "Average to prev. cp" + _separator;
            // x += "Average to prev. cp (1st run ignored)" + _separator;
            // x += "Execution time" + _separator;
            // stringBuilder.AppendLine(x);

            foreach (PerfItem item in _items.Values)
            {
                item.WriteToStream(stringBuilder);
            }

            return stringBuilder.ToString();
        }
    }


    /// <summary>
    /// Represents a performance item that summarizes performance information
    /// </summary>
    public class PerfItem
    {
        private readonly ThreadLocal<Stopwatch> _stopwatch;
        private readonly ThreadLocal<Stopwatch> _stopwatchCp;
        private readonly ConcurrentDictionary<string, ConcurrentBag<PerfLogItem>> _result;
        private char _separator = ';';

        internal PerfItem(string methodName)
        {
            MethodName = methodName;
            _result = new ConcurrentDictionary<string, ConcurrentBag<PerfLogItem>>();
            _stopwatch = new ThreadLocal<Stopwatch>(()=> new Stopwatch());
            _stopwatchCp = new ThreadLocal<Stopwatch>(()=> new Stopwatch());
        }

        /// <summary>
        /// Returns the method name
        /// </summary>
        private string MethodName { get; }

        internal void Start()
        {
            Debug.Assert(_stopwatch.Value != null, "_stopwatch.Value != null");
            Debug.Assert(_stopwatchCp.Value != null, "_stopwatchCp.Value != null");

            _stopwatch.Value.Reset();
            _stopwatchCp.Value.Reset();
            _stopwatch.Value.Start();
            _stopwatchCp.Value.Start();
        }

        internal void SetCheckPoint(string strDescription)
        {
            Debug.Assert(_stopwatch.Value != null, "_stopwatch.Value != null");
            Debug.Assert(_stopwatchCp.Value != null, "_stopwatchCp.Value != null");
            
            _stopwatch.Value.Stop();
            _stopwatchCp.Value.Stop();
            setLog(strDescription, _stopwatchCp.Value.ElapsedMilliseconds, _stopwatch.Value.ElapsedMilliseconds, DateTime.Now);
            _stopwatchCp.Value.Reset();
            _stopwatch.Value.Start();
            _stopwatchCp.Value.Start();
        }

        private void Stop(string strDescription)
        {
            Debug.Assert(_stopwatch.Value != null, "_stopwatch.Value != null");
            Debug.Assert(_stopwatchCp.Value != null, "_stopwatchCp.Value != null");
            
            _stopwatch.Value.Stop();
            _stopwatchCp.Value.Stop();
            
            var elapsed = _stopwatch.Value.ElapsedMilliseconds;
            var elapsedCp = _stopwatchCp.Value.ElapsedMilliseconds;

            setLog(strDescription, elapsedCp, elapsed, DateTime.Now);
        }

        internal void Stop()
        {
            Stop("Finished");
        }

        private void setLog(string strCheckPoint, long nElapsedCheckpoint, long nElapsed, DateTime dt)
        {
            var lst = _result.AddOrUpdate(strCheckPoint,
                (s)=> new ConcurrentBag<PerfLogItem>(), (s, item) => item);

            lst.Add(new PerfLogItem(nElapsed, nElapsedCheckpoint, dt));
        }

        internal void WriteToStream(StringBuilder stringBuilder)
        {
            foreach (KeyValuePair<string, ConcurrentBag<PerfLogItem>> item in _result)
            {
                ConcurrentBag<PerfLogItem> lst = item.Value;

                string x = MethodName + _separator;
                x += item.Key + _separator;

                x += lst.Count.ToString() + _separator;

                var maxElapsed = lst.Max(p => p.ElapsedTotalRun);
                var minElapsed = lst.Min(p => p.ElapsedTotalRun);
                var avgElapsed = Math.Round(lst.Average(p => p.ElapsedTotalRun), 0);
                var medElapsed = Math.Round(lst.Median(p => p.ElapsedTotalRun), 0);
                var sdElapsed = Math.Round(lst.StandardDeviation(p => p.ElapsedTotalRun), 0);

                var y = lst.Where(p => p.ElapsedTotalRun > 1000);

                //x += maxElapsed.ToString() + _separator;
               // x += minElapsed.ToString() + _separator;
                x += ",mean" + avgElapsed.ToString(CultureInfo.InvariantCulture) + _separator;
               // x += medElapsed.ToString(CultureInfo.InvariantCulture) + _separator;
                x += ",stddev" + sdElapsed.ToString(CultureInfo.InvariantCulture) + _separator;
                x += ",1=" + lst.GetQuantileOne(p=> p.ElapsedTotalRun).ToString(CultureInfo.InvariantCulture) + _separator;
                x += ",2=" + lst.GetQuantileTwo(p=> p.ElapsedTotalRun).ToString(CultureInfo.InvariantCulture) + _separator;
                x += ",3=" + lst.GetQuantileThree(p=> p.ElapsedTotalRun).ToString(CultureInfo.InvariantCulture) + _separator;
                x += ",4=" + lst.GetQuantileFour(p=> p.ElapsedTotalRun).ToString(CultureInfo.InvariantCulture) + _separator;
                x += ",95%=" + lst.GetQuantile(p=> p.ElapsedTotalRun, 95).ToString(CultureInfo.InvariantCulture) + _separator;

                
                stringBuilder.AppendLine(x);
            }
        }

        // private void WriteMessage(string text)
        // {
        //     WriteMessage(text, new object[] { });
        // }
        //
        // private void WriteMessage(string text, params object[] arg)
        // {
        //     string dt = DateTime.Now.ToString("H:mm:ss:f");
        //     string str = String.Format(text, arg);
        //   //  Console.WriteLine("{0}:{1} {2}", dt, this.MethodName, str);
        // }

        private class PerfLogItem
        {
            public long ElapsedTotalRun { get; }
            public long ElapsedSinceCp { get; }
            public DateTime DateTime { get; }

            internal PerfLogItem(long elapsedTotalRun, long elapsedSinceCp, DateTime dateTime)
            {
                ElapsedTotalRun = elapsedTotalRun;
                ElapsedSinceCp = elapsedSinceCp;
                DateTime = dateTime;
            }
        }
    }
}