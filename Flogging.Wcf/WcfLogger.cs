using Flogging.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace Flogging.Wcf
{
    public static class WcfLogger
    {
        //public static void LogDiagnostic(string message, object additionalInfo = null)
        //{
        //    var logEntry = GetWcfLogEntry(message, additionalInfo);
        //    LogIt(logEntry, "Diagnostic");
        //}
        public static void LogError(
            string message, 
            Exception ex,
            object additionalInfo = null
        )
        {
            var logEntry = GetWcfLogEntry(message, additionalInfo, null, ex);
            if (ex.Data.Contains("ErrorId"))
                logEntry.CorrelationId = ex.Data["ErrorId"].ToString();
            LogIt(logEntry, "Error");
        }
        internal static void LogIt(FlogDetail logEntry, string endpoint)
        {
            switch (endpoint)
            {
                case "Performance":
                    Flogger.WritePerf(logEntry);
                    break;
                case "Diagnostic":
                    Flogger.WriteDiagnostic(logEntry);
                    break;
                case "Error":
                    Flogger.WriteError(logEntry);
                    break;
                default:
                    return;
            }
        }
        internal static FlogDetail GetWcfLogEntry(
            string message,
            object additionalInfo, 
            string serviceName = null,
            Exception ex = null
        )
        {
            var logEntry = new FlogDetail
            {
                Timestamp = DateTime.Now,
                UserId = "",
                UserName = "",
                Hostname = Environment.MachineName,
                Product = ConfigurationManager.AppSettings["Product"],
                Layer = "WcfService",
                Message = message,
                Location = serviceName,
                Exception = ex,
                CustomException = ex.ToCustomException()
            };
            if (string.IsNullOrEmpty(logEntry.Location))
                logEntry.Location = GetLocationFromExceptionOrStackTrace(logEntry);
            logEntry.AdditionalInfo = new Dictionary<string, object>();
            if (additionalInfo != null)
                SetPropertiesFromAdditionalInfo(logEntry, additionalInfo);
            return logEntry;
        }
        private static string GetLocationFromExceptionOrStackTrace(FlogDetail logEntry)
        {
            var st = logEntry.Exception != null
                ? new StackTrace(logEntry.Exception)
                : new StackTrace();
            var sf = st.GetFrames();
            foreach (var frame in sf)
            {
                var method = frame.GetMethod();
                if 
                (
                    method.DeclaringType == typeof(WcfLogger) 
                    ||
                    method.DeclaringType == typeof(FloggingPerformanceInspector)
                )
                    continue;
                return $"{method.DeclaringType.FullName}->{method.Name}";
            }
            return "Unable to determine location from StackTrace";
        }
        private static void SetPropertiesFromAdditionalInfo(
            FlogDetail logEntry, object additionalInfo
        )
        {
            if (additionalInfo is Dictionary<string, object>)
            {
                foreach (var item in additionalInfo as Dictionary<string, object>)
                    if (!logEntry.AdditionalInfo.ContainsKey(item.Key))
                        logEntry.AdditionalInfo[item.Key] = item.Value;
            }
            else
            {
                foreach (var prop in additionalInfo.GetType().GetProperties())
                    logEntry.AdditionalInfo[$"dtl-{prop.Name}"] =
                        prop.GetValue(additionalInfo).ToString();
            }
        }
    }
}