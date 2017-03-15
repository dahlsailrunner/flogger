using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Configuration;

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
                .WriteTo.File(formatter: new FloggerJsonFormatter("FlogInfo"), path: "C:\\users\\edahl\\Source\\perf.json")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    CustomFormatter = new FloggerJsonFormatter("FlogInfo"),
                    TypeName = "performance",  // assigned                    
                    IndexFormat = "performance-{0:yyy.MM.dd}"

                })
                .CreateLogger();

            _usageLogger = new LoggerConfiguration()
                .WriteTo.File(formatter: new FloggerJsonFormatter("FlogInfo"), path: "C:\\users\\edahl\\Source\\usage.json")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    CustomFormatter = new FloggerJsonFormatter("FlogInfo"),
                    TypeName = "usage",  // assigned                    
                    IndexFormat = "usage-{0:yyy.MM.dd}"

                })
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                .WriteTo.File(formatter: new FloggerJsonFormatter("FlogInfo"), path: "C:\\users\\edahl\\Source\\error.json")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    CustomFormatter = new FloggerJsonFormatter("FlogInfo"),
                    TypeName = "error",  // assigned                    
                    IndexFormat = "error-{0:yyy.MM.dd}"

                })
                .CreateLogger();

            _diagnosticLogger = new LoggerConfiguration()
                .WriteTo.File(formatter: new FloggerJsonFormatter("FlogInfo"), path: "C:\\users\\edahl\\Source\\diagnostic.json")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    CustomFormatter = new FloggerJsonFormatter("FlogInfo"),
                    TypeName = "diagnostics",  // assigned                    
                    IndexFormat = "diagnostics-{0:yyy.MM.dd}"

                })
                .CreateLogger();
        }

        public static void WritePerf(FlogInfo infoToLog)
        {
            _perfLogger.Write(LogEventLevel.Information, "{@FlogInfo}", infoToLog);
        }
        public static void WriteUsage(FlogInfo infoToLog)
        {
            _usageLogger.Write(LogEventLevel.Information, "{@FlogInfo}", infoToLog);
        }
        public static void WriteError(FlogInfo infoToLog)
        {
            infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            _errorLogger.Write(LogEventLevel.Information, "{@FlogInfo}", infoToLog);
        }
        public static void WriteDiagnostic(FlogInfo infoToLog)
        {
            var writeDiagnostics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDiagnostics"]);
            if (!writeDiagnostics)
                return;
            
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
