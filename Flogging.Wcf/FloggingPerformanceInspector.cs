using Flogging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Dispatcher;

namespace Flogging.Wcf
{
    public class FloggingPerformanceInspector : IParameterInspector
    {
        string _serviceName;
        public FloggingPerformanceInspector(string serviceName)
        {
            _serviceName = serviceName;
        }
        public object BeforeCall(string operationName, object[] inputs)
        {
            var details = new Dictionary<string, object>();
            if (inputs != null)
            {
                for (var i = 0; i < inputs.Count(); i++)
                    details.Add(
                        $"input-{i}",
                        inputs[i] != null ? inputs[i].ToString() : string.Empty
                    );
            }
            return 
                WcfLogger.GetWcfLogEntry(
                    operationName, details, _serviceName
                );
        }
        public void AfterCall(
            string operationName, object[] outputs,
            object returnValue, object correlationState
        )
        {
            var logEntry = correlationState as FlogDetail;
            if (logEntry == null)
                return;
            logEntry.ElapsedMilliseconds = 
                (DateTime.Now - logEntry.Timestamp).Milliseconds;
            WcfLogger.LogIt(logEntry, "Performance");
        }
    }
}