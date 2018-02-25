using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Flogging.Wcf
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FloggingPerformanceInspectorAttribute : Attribute, IServiceBehavior
    {
        public void ApplyDispatchBehavior(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase
        )
        {
            foreach (var endpoint in serviceDescription.Endpoints)
                foreach (var op in endpoint.Contract.Operations)
                    op.OperationBehaviors
                        .Add(new FloggingPerformanceOperationBehavior());
        }
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters
        )
        {}
        public void Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase
        )
        {}
    }
}