using Microsoft.Xrm.Sdk;
using System;

namespace TestHarness.MockServices
{
    public class MockServiceProvider : IServiceProvider
    {
        private readonly IPluginExecutionContext _context;
        private readonly IOrganizationServiceFactory _serviceFactory;
        private readonly ITracingService _tracingService;

        public MockServiceProvider(IPluginExecutionContext context, IOrganizationServiceFactory serviceFactory, ITracingService tracingService)
        {
            _context = context;
            _serviceFactory = serviceFactory;
            _tracingService = tracingService;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IPluginExecutionContext))
                return _context;
            if (serviceType == typeof(IOrganizationServiceFactory))
                return _serviceFactory;
            if (serviceType == typeof(ITracingService))
                return _tracingService;

            throw new InvalidOperationException($"Service of type {serviceType.Name} is not available.");
        }
    }
}