using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace TestHarness.MockServices
{
    public class MockPluginExecutionContext : IPluginExecutionContext
    {
        public string PrimaryEntityName { get; set; }
        public ParameterCollection InputParameters { get; set; }
        public ParameterCollection OutputParameters { get; set; }
        public Guid UserId { get; set; }

        public MockPluginExecutionContext()
        {
            InputParameters = new ParameterCollection();
            OutputParameters = new ParameterCollection();
            SharedVariables = new ParameterCollection();
            PreEntityImages = new EntityImageCollection();
            PostEntityImages = new EntityImageCollection();
            UserId = Guid.NewGuid();
            RequestId = Guid.NewGuid();
            OperationId = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            OperationCreatedOn = DateTime.UtcNow;
        }

        // Required interface members
        public int Mode { get; set; }
        public int IsolationMode { get; set; }
        public int Depth { get; set; }
        public string MessageName { get; set; }
        public string SecondaryEntityName { get; set; } // Missing property added
        public Guid PrimaryEntityId { get; set; }
        public EntityReference PrimaryEntityReference { get; set; }
        public ParameterCollection SharedVariables { get; set; }
        public Guid? RequestId { get; set; } // Changed from Guid to Guid?
        public Guid OperationId { get; set; }
        public DateTime OperationCreatedOn { get; set; }
        public Guid CorrelationId { get; set; }
        public bool IsExecutingOffline { get; set; }
        public bool IsOfflinePlayback { get; set; }
        public bool IsInTransaction { get; set; }
        public Guid BusinessUnitId { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public EntityImageCollection PreEntityImages { get; set; }
        public EntityImageCollection PostEntityImages { get; set; }
        public EntityReference OwningExtension { get; set; }
        public IPluginExecutionContext ParentContext { get; set; }
        public int Stage { get; set; }
        public Guid InitiatingUserId { get; set; }
    }
}