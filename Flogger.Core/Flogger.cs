using Serilog;
using Serilog.Events;
using System;

namespace Flogging.Core
{
    public static class Flogger
    {
        private static readonly ILogger _perfLogger;
        private static readonly ILogger _usageLogger;
        private static readonly ILogger _errorLogger;
        private static readonly ILogger _diagnosticLogger;

        static Flogger()
        {
            _perfLogger = new LoggerConfiguration()
                .WriteTo.File(path: "C:\\users\\edahl\\Source\\perf.log")
                .CreateLogger();

            _usageLogger = new LoggerConfiguration()
                .WriteTo.File(path: "C:\\users\\edahl\\Source\\usage.log")
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                .WriteTo.File(path: "C:\\users\\edahl\\Source\\error.log")
                .CreateLogger();

            _diagnosticLogger = new LoggerConfiguration()
                .WriteTo.File(path: "C:\\users\\edahl\\Source\\diagnostic.log")
                .CreateLogger();
        }

        public static void WritePerf(FlogInfo infoToLog)
        {
            infoToLog.Timestamp = DateTime.Now;
            _perfLogger.Write(LogEventLevel.Information, "{@FlogInfo}", infoToLog);
        }
        public static void WriteUsage(FlogInfo infoToLog)
        {
            infoToLog.Timestamp = DateTime.Now;
            _usageLogger.Write(LogEventLevel.Information, "{@FlogInfo}", infoToLog);
        }
        public static void WriteError(FlogInfo infoToLog)
        {
            infoToLog.Timestamp = DateTime.Now;
            infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            _errorLogger.Write(LogEventLevel.Information, "{@FlogInfo}", infoToLog);
        }
        public static void WriteDiagnostic(FlogInfo infoToLog)
        {
            infoToLog.Timestamp = DateTime.Now;
            _diagnosticLogger.Write(LogEventLevel.Information, "{@FlogInfo}", infoToLog);
        }

        private static string GetMessageFromException(Exception ex)
        {
            if (ex == null) return "";
            if (ex.InnerException != null)
            {
                return GetMessageFromException(ex.InnerException);
            }
            return ex.Message;
        }

    }
}
