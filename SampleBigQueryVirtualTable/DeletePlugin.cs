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
            string query = $"DELETE FROM `myproject-469115.my_baseball_data.schedule` WHERE row_id = '{recordId}'";
            tracingService.Trace($"Delete query: {query}");
            
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