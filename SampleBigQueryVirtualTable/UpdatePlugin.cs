using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

public class UpdatePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.Get<IPluginExecutionContext>();
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        tracingService.Trace("UpdatePlugin started.");

        string entityName = context.PrimaryEntityName;
        Entity inputEntity = (Entity)context.InputParameters["Target"];
        Guid recordId = inputEntity.Id;

        var bigQueryConnection = new BigQueryConnection(tracingService);

        try
        {
            var updateFields = new StringBuilder();
            
            // Use FieldMappingHelper to convert Dataverse fields to BigQuery fields
            foreach (var attr in inputEntity.Attributes)
            {
                // Use FieldMappingHelper for reverse mapping
                string bigQueryFieldName = FieldMappingHelper.GetSourceFieldName(attr.Key);
                if (!string.IsNullOrEmpty(bigQueryFieldName))
                {
                    var value = attr.Value?.ToString().Replace("'", "''"); // Escape single quotes
                    updateFields.AppendFormat("{0} = '{1}', ", bigQueryFieldName, value);
                    tracingService.Trace($"Mapped field: {attr.Key} -> {bigQueryFieldName} = {value}");
                }
                else
                {
                    tracingService.Trace($"No mapping found for Dataverse field: {attr.Key}");
                }
            }
            
            if (updateFields.Length > 2)
                updateFields.Length -= 2; // Remove trailing comma

            string query = $"UPDATE `myproject-469115.my_baseball_data.schedule` SET {updateFields} WHERE row_id = '{recordId}'";
            tracingService.Trace($"Update query: {query}");
            
            JObject result = bigQueryConnection.ExecuteQuery(query);

            tracingService.Trace($"Update operation succeeded for entity '{entityName}' with Id '{recordId}'.");
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Update operation failed for entity '{entityName}' with Id '{recordId}': {ex.Message}");
            throw;
        }

        tracingService.Trace("UpdatePlugin completed successfully.");
    }
}