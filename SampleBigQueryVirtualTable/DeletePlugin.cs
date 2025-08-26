using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Newtonsoft.Json.Linq;
using System;

public class DeletePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.Get<IPluginExecutionContext>();
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        tracingService.Trace("DeletePlugin started.");

        // Declare recordId at method scope
        Guid recordId = Guid.Empty;
        string entityName = context.PrimaryEntityName;

        if (context.InputParameters.Contains("Target") &&
            context.InputParameters["Target"] is EntityReference target)
        {
            recordId = target.Id;
            string logicalName = target.LogicalName;

            tracingService.Trace($"Delete request for entity '{logicalName}' with ID: {recordId}");
        }

        tracingService.Trace($"Deleting entity: {entityName}");

        // Validate that we have a valid recordId
        if (recordId == Guid.Empty)
        {
            tracingService.Trace("Error: No valid record ID found in InputParameters");
            throw new InvalidPluginExecutionException("Target EntityReference is required for delete operation");
        }

        var bigQueryConnection = new BigQueryConnection(serviceProvider, tracingService);

        try
        {
            // Get the primary key field name from field mapping instead of hardcoding
            string primaryKeyField = FieldMappingHelper.GetPrimaryKeyFieldName();
            if (string.IsNullOrEmpty(primaryKeyField))
            {
                throw new InvalidPluginExecutionException("No primary key field mapping found");
            }

            // Use configuration values instead of hardcoded table reference
            string tableReference = $"`{bigQueryConnection.GetProjectId()}.{bigQueryConnection.GetDatasetId()}.{bigQueryConnection.GetTableId()}`";
            string query = $"DELETE FROM {tableReference} WHERE {primaryKeyField} = '{recordId}'";
            tracingService.Trace($"Delete query: {query}");
            tracingService.Trace($"Using primary key field: {primaryKeyField}");
            
            JObject result = bigQueryConnection.ExecuteQuery(query);

            tracingService.Trace($"Entity name: {entityName}");
            tracingService.Trace($"Record ID: {recordId}");
            tracingService.Trace($"Delete operation succeeded for entity '{entityName}' with Id '{recordId}'.");
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Delete operation failed for entity '{entityName}' with Id '{recordId}': {ex.Message}");
            throw;
        }

        tracingService.Trace("DeletePlugin completed successfully.");
    }
}