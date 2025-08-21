using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace TestHarness.MockServices
{
    public class MockOrganizationService : IOrganizationService
    {
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public Guid Create(Entity entity)
        {
            return Guid.NewGuid();
        }

        public void Delete(string entityName, Guid id)
        {
            // Mock implementation
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            throw new NotImplementedException();
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return new Entity(entityName) { Id = id };
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            return new EntityCollection();
        }

        public void Update(Entity entity)
        {
            // Mock implementation
        }
    }
}