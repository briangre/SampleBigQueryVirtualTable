using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

public static class BigQueryConfigurationHelper
{
    // Configuration key constants
    private const string BQ_PROJECT_ID_KEY = "mcd_bqProjectId";
    private const string BQ_DATASET_ID_KEY = "mcd_bqDatasetId";
    private const string BQ_TABLE_ID_KEY = "mcd_bqTableId";
    private const string BQ_SERVICE_ACCOUNT_JSON_KEY = "mcd_bqServiceAccountJson";
    private const string BQ_BASE_URL_KEY = "mcd_bqBaseUrl";
    private const string BQ_GOOGLE_TOKEN_URL_KEY = "mcd_bqGoogleTokenUrl";

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

            // Retrieve the data source once and reuse it - this returns an Entity
            tracingService.Trace("Retrieving entity data source...");
            Entity dataSourceEntity = dataSourceRetriever.RetrieveEntityDataSource();
            
            if (dataSourceEntity == null)
            {
                tracingService.Trace("Entity data source is null");
                throw new InvalidPluginExecutionException("Entity data source is null");
            }

            tracingService.Trace($"Entity data source retrieved successfully. Entity: {dataSourceEntity.LogicalName}, Attributes: {dataSourceEntity.Attributes.Count}");

            // Retrieve configuration values using the shared data source entity
            var projectId = GetConfigurationValue(dataSourceEntity, BQ_PROJECT_ID_KEY, tracingService);
            var datasetId = GetConfigurationValue(dataSourceEntity, BQ_DATASET_ID_KEY, tracingService);
            var tableId = GetConfigurationValue(dataSourceEntity, BQ_TABLE_ID_KEY, tracingService);
            var serviceAccountJson = GetConfigurationValue(dataSourceEntity, BQ_SERVICE_ACCOUNT_JSON_KEY, tracingService);
            var baseUrl = GetConfigurationValue(dataSourceEntity, BQ_BASE_URL_KEY, tracingService);
            var tokenUrl = GetConfigurationValue(dataSourceEntity, BQ_GOOGLE_TOKEN_URL_KEY, tracingService);

            // Validate required configuration
            ValidateConfiguration(projectId, datasetId, tableId, serviceAccountJson, baseUrl, tokenUrl);

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

    private static string GetConfigurationValue(Entity dataSourceEntity, string key, ITracingService tracingService)
    {
        try
        {
            tracingService.Trace($"Retrieving configuration value for key: {key}");
            
            string configValue = null;

            // Try to safely get and convert the value
            try
            {
                object rawValue = dataSourceEntity[key.ToLower()];
                configValue = rawValue?.ToString();
            }
            catch (KeyNotFoundException)
            {
                tracingService.Trace($"Configuration key '{key}' not found in data source");
                throw new InvalidPluginExecutionException($"Configuration key '{key}' not found");
            }
            catch (ArgumentNullException)
            {
                tracingService.Trace($"Configuration key '{key}' is null");
                throw new InvalidPluginExecutionException($"Configuration key '{key}' cannot be null");
            }
            catch (InvalidCastException)
            {
                tracingService.Trace($"Cannot convert value for key '{key}' to string");
                throw new InvalidPluginExecutionException($"Configuration value for key '{key}' is not a valid string");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Unexpected error accessing key '{key}': {ex.Message}");
                throw new InvalidPluginExecutionException($"Error accessing configuration key '{key}': {ex.Message}");
            }

            if (string.IsNullOrEmpty(configValue))
            {
                tracingService.Trace($"Configuration value for key '{key}' is null or empty");
                throw new InvalidPluginExecutionException($"Configuration value for key '{key}' is not set");
            }

            tracingService.Trace($"Successfully retrieved configuration value for key: {key}");
            return configValue;
        }
        catch (InvalidPluginExecutionException)
        {
            // Re-throw our custom exceptions
            throw;
        }
        catch (Exception ex)
        {
            tracingService.Trace($"Unexpected error retrieving configuration value for key '{key}': {ex.Message}");
            throw new InvalidPluginExecutionException($"Unexpected error retrieving configuration for key '{key}': {ex.Message}");
        }
    }    

    private static void ValidateConfiguration(string projectId, string datasetId, string tableId, string serviceAccountJson, string baseUrl, string tokenUrl)
    {
        if (string.IsNullOrEmpty(projectId))
            throw new InvalidPluginExecutionException("BigQuery Project ID is required");

        if (string.IsNullOrEmpty(datasetId))
            throw new InvalidPluginExecutionException("BigQuery Dataset ID is required");

        if (string.IsNullOrEmpty(tableId))
            throw new InvalidPluginExecutionException("BigQuery Table ID is required");

        if (string.IsNullOrEmpty(serviceAccountJson))
            throw new InvalidPluginExecutionException("BigQuery Service Account JSON is required");

        if (string.IsNullOrEmpty(baseUrl))
            throw new InvalidPluginExecutionException("BigQuery Base URL is required");

        if (string.IsNullOrEmpty(tokenUrl))
            throw new InvalidPluginExecutionException("BigQuery Token URL is required");
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