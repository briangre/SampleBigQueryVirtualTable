using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

public static class FieldMappingHelper
{
    private static readonly Dictionary<string, FieldMapping> _fieldMappings = new Dictionary<string, FieldMapping>(StringComparer.OrdinalIgnoreCase)
    {
        { "row_id", new FieldMapping("new_bqscheduleid", FieldType.Guid, isPrimaryKey: true) },
        { "bq_name", new FieldMapping("new_name", FieldType.String) },
        { "attendance", new FieldMapping("new_attendance", FieldType.Integer) },
        { "awayTeamId", new FieldMapping("new_awayteamid", FieldType.String) },
        { "awayTeamName", new FieldMapping("new_awayteamname", FieldType.String) },
        { "created", new FieldMapping("new_created", FieldType.DateTime) },
        { "dayNight", new FieldMapping("new_daynight", FieldType.String) },
        { "duration", new FieldMapping("new_gameduration", FieldType.String) },
        { "duration_minutes", new FieldMapping("new_gamedurationminutes", FieldType.Integer) },
        { "gameId", new FieldMapping("new_gameid", FieldType.String) },
        { "gameNumber", new FieldMapping("new_gamenumber", FieldType.Integer) },
        { "startTime", new FieldMapping("new_gamestarttime", FieldType.DateTime) },
        { "status", new FieldMapping("new_gamestatus", FieldType.String) },
        { "homeTeamId", new FieldMapping("new_hometeamid", FieldType.String) },
        { "homeTeamName", new FieldMapping("new_hometeamname", FieldType.String) },
        { "seasonId", new FieldMapping("new_seasonid", FieldType.String) },
        { "type", new FieldMapping("new_type", FieldType.String) },
        { "year", new FieldMapping("new_year", FieldType.Integer) }
    };

    public static bool TryGetMapping(string sourceFieldName, out FieldMapping mapping)
    {
        return _fieldMappings.TryGetValue(sourceFieldName, out mapping);
    }

    public static bool TryGetMappingWithValidation(string sourceFieldName, out FieldMapping mapping, ITracingService tracingService)
    {
        // Validate field name format
        if (!System.Text.RegularExpressions.Regex.IsMatch(sourceFieldName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
        {
            tracingService.Trace($"Invalid field name format: {sourceFieldName}");
            mapping = null;
            return false;
        }
        
        return TryGetMapping(sourceFieldName, out mapping);
    }

    public static void MapFieldToEntity(Entity entity, string sourceFieldName, object fieldValue, ITracingService tracingService)
    {
        if (!TryGetMapping(sourceFieldName, out FieldMapping mapping))
        {
            tracingService.Trace($"No mapping found for field: {sourceFieldName}");
            return;
        }

        var convertedValue = ConvertValue(fieldValue, mapping.DataType, tracingService, sourceFieldName);
        if (convertedValue != null)
        {
            // Special handling for row_id to set entity.Id
            if (mapping.IsPrimaryKey && convertedValue is Guid guidValue)
            {
                entity.Id = guidValue;
                tracingService.Trace($"Set entity Id to: {entity.Id} (from primary key field: {sourceFieldName})");
            }

            entity.Attributes.Add(mapping.DestinationFieldName, convertedValue);
            tracingService.Trace($"Set attribute {mapping.DestinationFieldName} to {convertedValue} of type: {convertedValue.GetType().ToString()}");
        }
    }

    public static string GetSourceFieldName(string destinationFieldName)
    {
        foreach (var mapping in _fieldMappings)
        {
            if (string.Equals(mapping.Value.DestinationFieldName, destinationFieldName, StringComparison.OrdinalIgnoreCase))
            {
                return mapping.Key;
            }
        }
        return null; // No mapping found
    }

    /// <summary>
    /// Gets the BigQuery source field name that is marked as the primary key
    /// </summary>
    /// <returns>The BigQuery field name used as primary key, or null if not found</returns>
    public static string GetPrimaryKeyFieldName()
    {
        var primaryKeyMapping = _fieldMappings.FirstOrDefault(m => m.Value.IsPrimaryKey);
        return primaryKeyMapping.Key;
    }

    /// <summary>
    /// Gets the Dataverse destination field name for the primary key
    /// </summary>
    /// <returns>The Dataverse field name mapped to the primary key, or null if not found</returns>
    public static string GetPrimaryKeyDestinationFieldName()
    {
        var primaryKeyMapping = _fieldMappings.FirstOrDefault(m => m.Value.IsPrimaryKey);
        return primaryKeyMapping.Value?.DestinationFieldName;
    }

    private static object ConvertValue(object value, FieldType dataType, ITracingService tracingService, string fieldName)
    {
        if (value == null) return null;

        var stringValue = value.ToString();
        
        try
        {
            switch (dataType)
            {
                case FieldType.Guid:
                    return Guid.TryParse(stringValue, out Guid guidResult) ? guidResult : (Guid?)null;
                
                case FieldType.Integer:
                    return int.TryParse(stringValue, out int intResult) ? intResult : (int?)null;
                
                case FieldType.DateTime:
                    return DateTime.TryParse(stringValue, out DateTime dateResult) ? dateResult : (DateTime?)null;
                
                case FieldType.String:
                default:
                    return stringValue;
            }
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Warning: Could not convert '{stringValue}' to {dataType} for field '{fieldName}': {ex.Message}");
            return null;
        }
    }
}

public class FieldMapping
{
    public string DestinationFieldName { get; set; }
    public FieldType DataType { get; set; }
    public bool IsPrimaryKey { get; set; }

    public FieldMapping(string destinationFieldName, FieldType dataType, bool isPrimaryKey = false)
    {
        DestinationFieldName = destinationFieldName;
        DataType = dataType;
        IsPrimaryKey = isPrimaryKey;
    }
}

public enum FieldType
{
    String,
    Integer,
    Guid,
    DateTime
}