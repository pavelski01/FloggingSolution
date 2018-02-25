using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Flogging.Wcf
{
    public class FloggingPerformanceOperationBehavior : IOperationBehavior
    {
        public void ApplyDispatchBehavior(
            OperationDescription operationDescription,
            DispatchOperation dispatchOperation
        )
        {
            if (dispatchOperation.Parent.Type != null)
                dispatchOperation.ParameterInspectors.Add(
                    new FloggingPerformanceInspector(dispatchOperation.Parent.Type.FullName)
                );
        }
        public void AddBindingParameters(
            OperationDescription operationDescription,
            BindingParameterCollection bindingParameters
        )
        {}
        public void ApplyClientBehavior(
            OperationDescription operationDescription,
            ClientOperation clientOperation
        )
        {}
        public void Validate(OperationDescription operationDescription)
        {}
    }
}
