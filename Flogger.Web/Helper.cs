using Flogging.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Web;

namespace Flogging.Web
{
    public static class Helper
    {
        public static void LogWebUsage(string product, string layer, string activityName)
        {
            string userId, userName, location;
            var webInfo = GetWebFloggingData(out userId, out userName, out location);

            var usageInfo = new FlogInfo()
            {
                Product = product,
                Layer = layer,
                Location = location,
                UserId = userId,
                UserName = userName,
                Hostname = Environment.MachineName,
                CorrelationId = HttpContext.Current.Session.SessionID,
                Message = activityName,                
                AdditionalInfo = webInfo
            };

            Flogger.WriteUsage(usageInfo);
        }

        public static void LogWebDiagnostic(string product, string layer, string message, Dictionary<string, object> diagnosticInfo = null)
        {
            var writeDiagnostics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDiagnostics"]);
            if (!writeDiagnostics)
                return;

            string userId, userName, location;
            var webInfo = GetWebFloggingData(out userId, out userName, out location);
            if (diagnosticInfo != null)
            {
                foreach (var key in diagnosticInfo.Keys)
                {
                    webInfo.Add(key, diagnosticInfo[key]);
                }
            }
            
            var diagInfo = new FlogInfo()
            {
                Product = product,
                Layer = layer,
                Location = location,
                UserId = userId,
                UserName = userName,
                Hostname = Environment.MachineName,
                CorrelationId = HttpContext.Current.Session.SessionID,
                Message = message,
                AdditionalInfo = webInfo
            };

            Flogger.WriteDiagnostic(diagInfo);
        }

        public static void LogWebError(string product, string layer, Exception ex)
        {
            string userId, userName, location;
            var webInfo = GetWebFloggingData(out userId, out userName, out location);

            var errorInformation = new FlogInfo()
            {
                Product = product,
                Layer = layer,
                Location = location,
                UserId = userId,
                UserName = userName,
                Hostname = Environment.MachineName,
                CorrelationId = HttpContext.Current.Session.SessionID,
                Exception = ex,
                AdditionalInfo = webInfo
            };

            Flogger.WriteError(errorInformation);
        }

        public static void GetHttpStatus(Exception ex, out int httpStatus)
        {
            httpStatus = 500;  // default is server error
            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;
                httpStatus = httpEx.GetHttpCode();
            }
        }

        public static Dictionary<string, object> GetWebFloggingData(out string userId, out string userName, out string location)
        {
            var data = new Dictionary<string, object>();

            GetSessionData(data);
            GetRequestData(data, out location);
            GetUserData(data, out userId, out userName);
            
            return data;
        }

        private static void GetUserData(Dictionary<string, object> data, out string userId, out string userName)
        {
            userId = "";
            userName = "";
            var user = ClaimsPrincipal.Current;
            if (user != null)
            {
                var i = 1;
                foreach (var claim in user.Claims)
                {
                    if (claim.Type == ClaimTypes.NameIdentifier)
                    {
                        userId = claim.Value;
                    }
                    else if (claim.Type == ClaimTypes.Name)
                    {
                        userName = claim.Value;
                    }
                    else
                    {
                        data.Add(string.Format("UserClaim-{0}-{1}", i++, claim.Type), claim.Value);
                    }
                }
            }
        }

        private static void GetRequestData(Dictionary<string, object> data, out string location)
        {
            location = "";
            var request = HttpContext.Current.Request;
            if (request != null)
            {
                location = request.Path;

                data.Add("Browser", request.Browser.Type + " (v " + request.Browser.MajorVersion + "." + request.Browser.MinorVersion + ")");
                data.Add("UserHostAddress", request.UserHostAddress);
                data.Add("UserAgent", request.UserAgent);                
                data.Add("Languages", request.UserLanguages);
                foreach (var qsKey in request.QueryString.Keys)
                {
                    data.Add(string.Format("QueryString-{0}", qsKey), request.QueryString[qsKey.ToString()]);
                }
            }
        }

        private static void GetSessionData(Dictionary<string, object> data)
        {
            if (HttpContext.Current.Session != null)
            {
                foreach (var key in HttpContext.Current.Session.Keys)
                {
                    if (HttpContext.Current.Session[key.ToString()] != null)
                    {
                        data.Add(string.Format("Session-{0}", key.ToString()), HttpContext.Current.Session[key.ToString()].ToString());
                    }
                }
                data.Add("SessionId", HttpContext.Current.Session.SessionID);
            }
        }
    }
}
