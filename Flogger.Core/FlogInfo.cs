using System;
using System.Collections.Generic;

namespace Flogging.Core
{
    public class FlogInfo
    {
        public string Product { get; set; }
        public string Layer { get; set; }
        public string Location { get; set; }
        public string Hostname { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Message { get; set; }
        internal DateTime Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; }
        public Exception Exception { get; set; }
    }
}
