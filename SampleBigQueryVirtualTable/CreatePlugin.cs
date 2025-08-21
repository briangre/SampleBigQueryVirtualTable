using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Newtonsoft.Json.Linq;
using System;

public class CreatePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.Get<IPluginExecutionContext>();
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        tracingService.Trace("CreatePlugin started.");

        string entityName = context.PrimaryEntityName;
        Entity inputEntity = (Entity)context.InputParameters["Target"];

        var bigQueryConnection = new BigQueryConnection(tracingService);

        try
        {
            // Convert Entity to JObject for REST API using FieldMappingHelper
            var rowData = new JObject();
            
            foreach (var attr in inputEntity.Attributes)
            {
                // Use FieldMappingHelper for reverse mapping
                string bigQueryFieldName = FieldMappingHelper.GetSourceFieldName(attr.Key);
                if (!string.IsNullOrEmpty(bigQueryFieldName))
                {
                    rowData[bigQueryFieldName] = JToken.FromObject(attr.Value);
                    tracingService.Trace($"Mapped field: {attr.Key} -> {bigQueryFieldName} = {attr.Value}");
                }
                else
                {
                    tracingService.Trace($"No mapping found for Dataverse field: {attr.Key}");
                }
            }

            // Generate a new GUID for row_id if not already present
            string newGuid = Guid.NewGuid().ToString();
            rowData["row_id"] = newGuid;
            tracingService.Trace($"Generated new row_id: {newGuid}");

            // Also set the entity ID to the same GUID for consistency
            inputEntity.Id = Guid.Parse(newGuid);

            tracingService.Trace($"Inserting row with {rowData.Count} fields into BigQuery");
            bigQueryConnection.InsertRow("schedule", rowData);
            
            tracingService.Trace($"Entity name: {entityName}");
            tracingService.Trace($"Entity ID: {inputEntity.Id}");
            tracingService.Trace($"Attributes processed: {inputEntity.Attributes.Count}");
            tracingService.Trace($"Create operation succeeded for entity '{entityName}' with Id '{inputEntity.Id}'.");
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Create operation failed for entity '{entityName}': {ex.Message}");
            throw;
        }

        context.OutputParameters["id"] = inputEntity.Id;
        tracingService.Trace("CreatePlugin completed successfully.");
    }
}