using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Flogging.Core
{
    public class PerfTracker
    {
        private readonly Stopwatch _sw;
        private readonly FlogInfo _infoToLog;
        private DateTime _beginTime;
        private readonly Dictionary<string, object> _details;        

        public PerfTracker(string name, string userId, string userName, string location, string product, string layer)
        {
            _sw = Stopwatch.StartNew();
            _infoToLog = new FlogInfo() {
                Message = name,
                UserId = userId, 
                UserName = userName,
                Product = product,
                Layer = layer,
                Location = location,
                Hostname = Environment.MachineName
            };            
            
            _beginTime = DateTime.Now;
            _details = new Dictionary<string, object>();
        }
        public PerfTracker(string name, string userId, string userName, string location, string product, string layer, Dictionary<string, object> perfParams) 
            : this(name, userId, userName, location, product, layer)
        {
            foreach (var item in perfParams)
                _details.Add("input-" + item.Key, item.Value);
        }

        public void Stop()
        {
            _sw.Stop();

            if (!_details.ContainsKey("Started"))
                _details.Add("Started", _beginTime.ToString(CultureInfo.InvariantCulture));
            else
                _details["Started"] = _beginTime.ToString(CultureInfo.InvariantCulture);

            _infoToLog.ElapsedMilliseconds = _sw.ElapsedMilliseconds;
            _infoToLog.AdditionalInfo = _details;

            Flogger.WritePerf(_infoToLog);
        }
    }
}
