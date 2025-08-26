using System;
using Microsoft.Xrm.Sdk;

public static class SecurityHelper
{
    public static string ValidateGuid(string input, string parameterName)
    {
        if (!Guid.TryParse(input, out Guid result))
        {
            throw new InvalidPluginExecutionException($"Invalid GUID format for {parameterName}");
        }
        return result.ToString(); // Returns normalized GUID string
    }

    public static string ValidateFieldName(string fieldName)
    {
        // Only allow alphanumeric characters and underscores
        if (!System.Text.RegularExpressions.Regex.IsMatch(fieldName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
        {
            throw new InvalidPluginExecutionException($"Invalid field name: {fieldName}");
        }
        return fieldName;
    }

    public static string EscapeStringValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "NULL";
            
        // Escape single quotes and backslashes for BigQuery
        return "'" + value.Replace("\\", "\\\\").Replace("'", "\\'") + "'";
    }

    public static string FormatValueForBigQuery(object value, FieldType dataType)
    {
        if (value == null)
            return "NULL";

        switch (dataType)
        {
            case FieldType.String:
                return EscapeStringValue(value.ToString());
            case FieldType.Integer:
                if (int.TryParse(value.ToString(), out int intVal))
                    return intVal.ToString();
                throw new InvalidPluginExecutionException($"Invalid integer value: {value}");
            case FieldType.Guid:
                return EscapeStringValue(ValidateGuid(value.ToString(), "guid"));
            case FieldType.DateTime:
                if (DateTime.TryParse(value.ToString(), out DateTime dateVal))
                    return $"DATETIME('{dateVal:yyyy-MM-dd HH:mm:ss}')";
                throw new InvalidPluginExecutionException($"Invalid datetime value: {value}");
            default:
                return EscapeStringValue(value.ToString());
        }
    }
}