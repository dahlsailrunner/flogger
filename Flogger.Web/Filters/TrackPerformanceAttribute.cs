using Flogging.Core;
using System.Web.Mvc;

namespace Flogging.Web.Filters
{
    public class TrackPerformanceAttribute : ActionFilterAttribute
    {
        private string _productName;
        private string _layerName;

        public TrackPerformanceAttribute(string product, string layer)
        {
            _productName = product;
            _layerName = layer;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string userId, userName, location;
            var dict = Helper.GetWebFloggingData(out userId, out userName, out location);                                    

            var type = filterContext.HttpContext.Request.RequestType;
            var perfName = filterContext.ActionDescriptor.ActionName + "_" + type;

            var stopwatch = new PerfTracker(perfName, userId, userName, location, _productName, _layerName, dict);
            filterContext.HttpContext.Items["Stopwatch"] = stopwatch;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var stopwatch = (PerfTracker)filterContext.HttpContext.Items["Stopwatch"];
            if (stopwatch != null)
                stopwatch.Stop();
        }
    }
}
