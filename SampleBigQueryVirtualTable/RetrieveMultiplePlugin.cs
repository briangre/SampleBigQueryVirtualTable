using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Xrm.Sdk.Extensions;

public class RetrieveMultiplePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.Get<IPluginExecutionContext>();
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        tracingService.Trace("RetrieveMultiplePlugin started.");

        string entityName = context.PrimaryEntityName;
        var bigQueryConnection = new BigQueryConnection(tracingService);

        try
        {
            string query = $"select * from `myproject-469115.my_baseball_data.schedule`";
            JObject result = bigQueryConnection.ExecuteQuery(query);

            EntityCollection entities = new EntityCollection();
            var rows = result["rows"] as JArray;

            if (rows != null)
            {
                foreach (JObject row in rows)
                {
                    Entity entity = new Entity(entityName);
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
                        entities.Entities.Add(entity);
                    }
                }

                tracingService.Trace($"Total records retrieved: {entities.Entities.Count}");
                tracingService.Trace($"Entity name: {entityName}");
                
                context.OutputParameters["BusinessEntityCollection"] = new EntityCollection(entities.Entities);
                
            }
        }
        catch (Exception ex)
        {
            tracingService.Trace($"RetrieveMultiple operation failed for entity '{entityName}': {ex.Message}");
            throw;
        }

        tracingService.Trace("RetrieveMultiplePlugin completed successfully.");
    }
}