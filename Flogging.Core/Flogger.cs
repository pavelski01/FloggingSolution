using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

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
            var connStr = 
                ConfigurationManager.ConnectionStrings["FloggerConnection"].ToString();
            _perfLogger = 
                new LoggerConfiguration()
                    .WriteTo
                    .MSSqlServer(
                        connStr, 
                        "PerfLogs", 
                        batchPostingLimit: 1, 
                        autoCreateSqlTable: true, 
                        columnOptions: GetSqlColumnOptions()
                    )
                    .CreateLogger();
            _usageLogger =
                new LoggerConfiguration()
                    .WriteTo
                    .MSSqlServer(
                        connStr,
                        "UsageLogs",
                        batchPostingLimit: 1,
                        autoCreateSqlTable: true,
                        columnOptions: GetSqlColumnOptions()
                    )
                    .CreateLogger();
            _errorLogger =
                new LoggerConfiguration()
                    .WriteTo
                    .MSSqlServer(
                        connStr,
                        "ErrorLogs",
                        batchPostingLimit: 1,
                        autoCreateSqlTable: true,
                        columnOptions: GetSqlColumnOptions()
                    )
                    .CreateLogger();
            _diagnosticLogger =
                new LoggerConfiguration()
                    .WriteTo
                    .MSSqlServer(
                        connStr,
                        "DiagnosticLogs",
                        batchPostingLimit: 1,
                        autoCreateSqlTable: true,
                        columnOptions: GetSqlColumnOptions()
                    )
                    .CreateLogger();
            SelfLog.Enable(msg => Debug.WriteLine(msg));
        }
        public static ColumnOptions GetSqlColumnOptions()
        {
            var colOptions = new ColumnOptions();
            colOptions.Store.Remove(StandardColumn.Properties);
            colOptions.Store.Remove(StandardColumn.MessageTemplate);
            colOptions.Store.Remove(StandardColumn.Message);
            colOptions.Store.Remove(StandardColumn.Exception);
            colOptions.Store.Remove(StandardColumn.TimeStamp);
            colOptions.Store.Remove(StandardColumn.Level);
            colOptions.AdditionalDataColumns = 
                new Collection<DataColumn>
                {
                    new DataColumn
                    {
                        DataType = typeof(DateTime),
                        ColumnName = "Timestamp"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "Product"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "Layer"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "Location"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "Message"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "Hostname"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "UserId"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "UserName"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "Exception"
                    },
                    new DataColumn
                    {
                        DataType = typeof(long),
                        ColumnName = "ElapsedMilliseconds"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "CorrelationId"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "CustomException"
                    },
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "AdditionalInfo"
                    }
                };
            return colOptions;
        }
        public static void WritePerf(FlogDetail infoToLog)
        {
            _perfLogger.Write(
                LogEventLevel.Information,
                "{Timestamp}{Message}" +
                "{Layer}{Location}" +
                "{Product}{CustomException}" +
                "{ElapsedMilliseconds}{Exception}" +
                "{Hostname}{UserId}" +
                "{UserName}{CorrelationId}" +
                "{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );
        }
        public static void WriteUsage(FlogDetail infoToLog)
        {
            _usageLogger.Write(
                LogEventLevel.Information,
                "{Timestamp}{Message}" +
                "{Layer}{Location}" +
                "{Product}{CustomException}" +
                "{ElapsedMilliseconds}{Exception}" +
                "{Hostname}{UserId}" +
                "{UserName}{CorrelationId}" +
                "{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );
        }
        public static void WriteError(FlogDetail infoToLog)
        {
            if (infoToLog.Exception != null)
            {
                var procName = FindProcName(infoToLog.Exception);
                infoToLog.Location = string.IsNullOrEmpty(procName) ? infoToLog.Location : procName;
                infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            }          
            _errorLogger.Write(
                LogEventLevel.Information,
                "{Timestamp}{Message}" +
                "{Layer}{Location}" +
                "{Product}{CustomException}" +
                "{ElapsedMilliseconds}{Exception}" +
                "{Hostname}{UserId}" +
                "{UserName}{CorrelationId}" +
                "{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );
        }
        public static void WriteDiagnostic(FlogDetail infoToLog)
        {
            var isWriteDiagnostics = 
                Convert.ToBoolean(
                    ConfigurationManager.AppSettings["EnableDiagnostics"]
                );
            if (!isWriteDiagnostics)
                return;
            _diagnosticLogger.Write(
                LogEventLevel.Information,
                "{Timestamp}{Message}" +
                "{Layer}{Location}" +
                "{Product}{CustomException}" +
                "{ElapsedMilliseconds}{Exception}" +
                "{Hostname}{UserId}" +
                "{UserName}{CorrelationId}" +
                "{AdditionalInfo}",
                infoToLog.Timestamp, infoToLog.Message,
                infoToLog.Layer, infoToLog.Location,
                infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog.Exception?.ToBetterString(),
                infoToLog.Hostname, infoToLog.UserId,
                infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo
            );
        }
        private static string FindProcName(Exception ex)
        {
            if (ex is SqlException sqlEx)
            {
                var procName = sqlEx.Procedure;
                if (!string.IsNullOrEmpty(procName))
                    return procName;
            }
            if (!string.IsNullOrEmpty((string)ex.Data["Procedure"]))
                return (string)ex.Data["Procedure"];
            if (ex.InnerException != null)
                return FindProcName(ex.InnerException);
            return null;
        }
        private static string GetMessageFromException(Exception ex)
        {
            if (ex.InnerException != null)
                return GetMessageFromException(ex.InnerException);
            return ex.Message;
        }
    }
}