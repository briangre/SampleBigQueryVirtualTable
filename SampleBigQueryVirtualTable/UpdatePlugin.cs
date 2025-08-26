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

        var bigQueryConnection = new BigQueryConnection(serviceProvider, tracingService);

        try
        {
            var updateFields = new StringBuilder();
            
            foreach (var attr in inputEntity.Attributes)
            {
                string bigQueryFieldName = FieldMappingHelper.GetSourceFieldName(attr.Key);
                if (!string.IsNullOrEmpty(bigQueryFieldName))
                {
                    // Validate field name
                    string validatedFieldName = SecurityHelper.ValidateFieldName(bigQueryFieldName);
                    
                    // Get the field mapping to determine data type
                    if (FieldMappingHelper.TryGetMapping(bigQueryFieldName, out FieldMapping mapping))
                    {
                        // Use type-safe formatting
                        string formattedValue = SecurityHelper.FormatValueForBigQuery(attr.Value, mapping.DataType);
                        updateFields.AppendFormat("{0} = {1}, ", validatedFieldName, formattedValue);
                        tracingService.Trace($"Secured field: {attr.Key} -> {validatedFieldName} = {formattedValue}");
                    }
                }
            }
            
            if (updateFields.Length > 2)
                updateFields.Length -= 2; // Remove trailing comma

            // Validate primary key and record ID
            string primaryKeyField = SecurityHelper.ValidateFieldName(FieldMappingHelper.GetPrimaryKeyFieldName());
            string sanitizedRecordId = SecurityHelper.ValidateGuid(recordId.ToString(), "recordId");
            
            string tableReference = $"`{bigQueryConnection.GetProjectId()}.{bigQueryConnection.GetDatasetId()}.{bigQueryConnection.GetTableId()}`";
            string query = $"UPDATE {tableReference} SET {updateFields} WHERE {primaryKeyField} = '{sanitizedRecordId}'";
            tracingService.Trace($"Update query: {query}");
            tracingService.Trace($"Using primary key field: {primaryKeyField}");
            
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