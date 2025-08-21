using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Newtonsoft.Json.Linq;
using System;

public class RetrievePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.Get<IPluginExecutionContext>();
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        tracingService.Trace("RetrievePlugin started.");

        // Declare recordId at method scope
        Guid recordId = Guid.Empty;
        string entityName = context.PrimaryEntityName;

        if (context.InputParameters.Contains("Target") &&
            context.InputParameters["Target"] is EntityReference target)
        {
            recordId = target.Id;
            string logicalName = target.LogicalName;

            tracingService.Trace($"Retrieve request for entity '{logicalName}' with ID: {recordId}");

            // Trace all available properties and attributes on the target EntityReference
            tracingService.Trace("=== Target EntityReference Details ===");
            tracingService.Trace($"Target ID: {target.Id}");
            tracingService.Trace($"Target LogicalName: {target.LogicalName}");
            tracingService.Trace($"Target Name: {target.Name ?? "NULL"}");
            tracingService.Trace($"Target KeyAttributes Count: {target.KeyAttributes?.Count ?? 0}");

            if (target.KeyAttributes != null && target.KeyAttributes.Count > 0)
            {
                tracingService.Trace("--- Target KeyAttributes ---");
                foreach (var keyAttr in target.KeyAttributes)
                {
                    var keyValue = keyAttr.Value ?? "NULL";
                    tracingService.Trace($"    {keyAttr.Key}: {keyValue} (Type: {keyAttr.Value?.GetType().Name ?? "NULL"})");
                }
            }
            else
            {
                tracingService.Trace("No KeyAttributes found on target");
            }

            tracingService.Trace("=== End Target EntityReference Details ===");
        }

        tracingService.Trace($"Retrieving entity: {entityName}");

        // Validate that we have a valid recordId
        if (recordId == Guid.Empty)
        {
            tracingService.Trace("Error: No valid record ID found in InputParameters");
            throw new InvalidPluginExecutionException("Target EntityReference is required for retrieve operation");
        }

        var bigQueryConnection = new BigQueryConnection(tracingService);

        try
        {
            string query = $"SELECT * FROM `myproject-469115.my_baseball_data.schedule` WHERE row_id = '{recordId}' LIMIT 1";
            JObject result = bigQueryConnection.ExecuteQuery(query);

            Entity entity = new Entity(entityName);
            var rows = result["rows"] as JArray;
            
            if (rows != null && rows.Count > 0)
            {
                var row = rows[0] as JObject;
                var fields = row["f"] as JArray;
                var schema = result["schema"]["fields"] as JArray;
                
                if (fields != null && schema != null)
                {
                    for (int i = 0; i < fields.Count && i < schema.Count; i++)
                    {
                        var fieldName = schema[i]["name"].ToString();
                        var fieldValue = fields[i]["v"];
                        
                        // Use the field mapping helper
                        FieldMappingHelper.MapFieldToEntity(entity, fieldName, fieldValue, tracingService);
                    }
                }
            }

            context.OutputParameters["BusinessEntity"] = entity;
            tracingService.Trace($"Retrieve operation succeeded for entity '{entityName}' with Id '{recordId}'.");
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Retrieve operation failed for entity '{entityName}' with Id '{recordId}': {ex.Message}");
            throw;
        }

        tracingService.Trace("RetrievePlugin completed successfully.");
    }
}