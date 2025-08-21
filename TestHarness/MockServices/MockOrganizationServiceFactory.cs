using Microsoft.Xrm.Sdk;
using System;

namespace TestHarness.MockServices
{
    public class MockOrganizationServiceFactory : IOrganizationServiceFactory
    {
        public IOrganizationService CreateOrganizationService(Guid? userId)
        {
            return new MockOrganizationService();
        }
    }
}