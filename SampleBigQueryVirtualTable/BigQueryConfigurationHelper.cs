using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

public static class BigQueryConfigurationHelper
{
    // Configuration key constants
    private const string BQ_PROJECT_ID_KEY = "bqProjectId";
    private const string BQ_DATASET_ID_KEY = "bqDatasetId";
    private const string BQ_TABLE_ID_KEY = "bqTableId";
    private const string BQ_SERVICE_ACCOUNT_JSON_KEY = "bqServiceAccountJson";
    private const string BQ_BASE_URL_KEY = "bqBaseUrl";
    private const string GOOGLE_TOKEN_URL_KEY = "googleTokenUrl";

    public static BigQueryConfiguration GetConfiguration(IServiceProvider serviceProvider, ITracingService tracingService)
    {
        try
        {
            // Get the IEntityDataSourceRetrieverService from the service provider
            var dataSourceRetriever = (IEntityDataSourceRetrieverService)serviceProvider.GetService(typeof(IEntityDataSourceRetrieverService));
            
            if (dataSourceRetriever == null)
            {
                tracingService.Trace("IEntityDataSourceRetrieverService is not available");
                throw new InvalidPluginExecutionException("IEntityDataSourceRetrieverService is not available");
            }

            // Retrieve configuration values
            var projectId = GetConfigurationValue(dataSourceRetriever, BQ_PROJECT_ID_KEY, tracingService);
            var datasetId = GetConfigurationValue(dataSourceRetriever, BQ_DATASET_ID_KEY, tracingService);
            var tableId = GetConfigurationValue(dataSourceRetriever, BQ_TABLE_ID_KEY, tracingService);
            var serviceAccountJson = GetConfigurationValue(dataSourceRetriever, BQ_SERVICE_ACCOUNT_JSON_KEY, tracingService);
            
            // Retrieve URL configuration values with defaults
            var baseUrl = GetConfigurationValueWithDefault(dataSourceRetriever, BQ_BASE_URL_KEY, "https://bigquery.googleapis.com/bigquery/v2", tracingService);
            var tokenUrl = GetConfigurationValueWithDefault(dataSourceRetriever, GOOGLE_TOKEN_URL_KEY, "https://oauth2.googleapis.com/token", tracingService);

            // Validate required configuration
            ValidateConfiguration(projectId, datasetId, tableId, serviceAccountJson);

            tracingService.Trace($"Configuration loaded successfully - Project: {projectId}, Dataset: {datasetId}, Table: {tableId}");
            tracingService.Trace($"URLs - BaseUrl: {baseUrl}, TokenUrl: {tokenUrl}");

            return new BigQueryConfiguration
            {
                ProjectId = projectId,
                DatasetId = datasetId,
                TableId = tableId,
                ServiceAccountJson = serviceAccountJson,
                BaseUrl = baseUrl,
                TokenUrl = tokenUrl
            };
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Failed to load BigQuery configuration: {ex.Message}");
            throw;
        }
    }

    private static string GetConfigurationValue(IEntityDataSourceRetrieverService dataSourceRetriever, string key, ITracingService tracingService)
    {
        try
        {
            tracingService.Trace($"Retrieving configuration value for key: {key}");
            
            // The RetrieveEntityChanges method is used to get configuration values
            // The key is passed as a parameter and the configuration value is returned
            var configValue = dataSourceRetriever.RetrieveEntityChanges(key);
            
            if (string.IsNullOrEmpty(configValue))
            {
                tracingService.Trace($"Configuration value for key '{key}' is null or empty");
                throw new InvalidPluginExecutionException($"Configuration value for key '{key}' is not set");
            }

            tracingService.Trace($"Successfully retrieved configuration value for key: {key}");
            return configValue;
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Error retrieving configuration value for key '{key}': {ex.Message}");
            throw;
        }
    }

    private static string GetConfigurationValueWithDefault(IEntityDataSourceRetrieverService dataSourceRetriever, string key, string defaultValue, ITracingService tracingService)
    {
        try
        {
            tracingService.Trace($"Retrieving configuration value for key: {key} (with default: {defaultValue})");
            
            var configValue = dataSourceRetriever.RetrieveEntityChanges(key);
            
            if (string.IsNullOrEmpty(configValue))
            {
                tracingService.Trace($"Configuration value for key '{key}' is null or empty, using default value: {defaultValue}");
                return defaultValue;
            }

            tracingService.Trace($"Successfully retrieved configuration value for key: {key}");
            return configValue;
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Error retrieving configuration value for key '{key}', using default: {defaultValue}. Error: {ex.Message}");
            return defaultValue;
        }
    }

    private static void ValidateConfiguration(string projectId, string datasetId, string tableId, string serviceAccountJson)
    {
        if (string.IsNullOrEmpty(projectId))
            throw new InvalidPluginExecutionException("BigQuery Project ID is required");

        if (string.IsNullOrEmpty(datasetId))
            throw new InvalidPluginExecutionException("BigQuery Dataset ID is required");

        if (string.IsNullOrEmpty(tableId))
            throw new InvalidPluginExecutionException("BigQuery Table ID is required");

        if (string.IsNullOrEmpty(serviceAccountJson))
            throw new InvalidPluginExecutionException("BigQuery Service Account JSON is required");
    }
}

public class BigQueryConfiguration
{
    public string ProjectId { get; set; }
    public string DatasetId { get; set; }
    public string TableId { get; set; }
    public string ServiceAccountJson { get; set; }
    public string BaseUrl { get; set; }
    public string TokenUrl { get; set; }
}