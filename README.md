# 🔗 BigQuery Virtual Tables for Microsoft Dataverse

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.6.2-blue)](https://dotnet.microsoft.com/download/dotnet-framework)
[![C#](https://img.shields.io/badge/C%23-7.3-green)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> **Seamlessly integrate Google BigQuery data into Microsoft Dataverse using Virtual Tables**

Transform your BigQuery datasets into native Dataverse entities with full CRUD operations, real-time data access, and enterprise-grade security. Perfect for organizations wanting to leverage BigQuery's analytical power within their Power Platform ecosystem.

## 🌟 **Why This Project?**

- **🚀 Real-time Integration**: Query BigQuery data directly from Dataverse without ETL pipelines
- **🔒 Enterprise Security**: Secure authentication using Google Service Account credentials
- **⚡ Performance Optimized**: Efficient caching and connection management
- **🎯 Production Ready**: Comprehensive error handling, logging, and configuration management
- **🔧 Developer Friendly**: Clean architecture with extensive documentation and examples

## 📊 **What It Does**

This virtual table provider enables you to:

- **Read** BigQuery data as if it were native Dataverse records
- **Write** new records directly to BigQuery from Dataverse
- **Update** existing BigQuery records through Dataverse operations  
- **Delete** BigQuery records via Dataverse delete operations
- **Configure** multiple BigQuery projects/datasets through Dataverse configuration
- **Map** BigQuery schemas to Dataverse field types automatically

## 🏗️ **Architecture Overview**

```
Dataverse ←→ Virtual Table Provider ←→ BigQuery REST API ←→ Google Cloud
    ↓              ↓                      ↓                ↓
Power Apps    Plugin Framework      JWT Authentication   BigQuery Data
Power BI      CRUD Operations       Service Account      Analytics Ready
Dynamics 365  Field Mapping         OAuth 2.0           Petabyte Scale
```

## 🛠️ **Core Components**

### **Plugin Architecture**
- `RetrievePlugin.cs` - Single record retrieval with comprehensive tracing
- `RetrieveMultiplePlugin.cs` - Bulk data operations with pagination support
- `CreatePlugin.cs` - New record creation with GUID generation
- `UpdatePlugin.cs` - Record modification with field-level mapping
- `DeletePlugin.cs` - Safe record deletion with validation

### **Advanced Features**
- **Smart Field Mapping**: Automatic type conversion between BigQuery and Dataverse
- **Configuration Management**: Secure credential storage via `IEntityDataSourceRetrieverService`
- **JWT Authentication**: Self-contained Google Cloud authentication
- **Connection Pooling**: Optimized HTTP client management
- **Comprehensive Logging**: Detailed trace information for debugging

## 📚 **Reference Sample**

### **Prerequisites**
- Microsoft Dataverse environment
- Google Cloud Project with BigQuery API enabled
- Visual Studio 2019+ or VS Code
- .NET Framework 4.6.2 SDK

### **Using This Sample**

> **⚠️ Important Note**: This is a reference implementation and sample code designed to demonstrate BigQuery Virtual Table integration patterns. 
It is not intended for direct production use without proper customization, testing, and security review.

**This sample demonstrates:**
- Complete CRUD operations for BigQuery virtual entities
- Secure authentication patterns using Google Service Account
- Field mapping between BigQuery and Dataverse schemas
- Configuration management through Dataverse services

**Before adapting for production:**
1. **Review Security**: Update authentication and credential management for your security requirements
2. **Customize Field Mappings**: Modify `FieldMappingHelper.cs` to match your BigQuery schema
3. **Update Configuration**: Replace sample configuration keys with your environment-specific values
4. **Test Thoroughly**: Validate all CRUD operations with your data
5. **Performance Tuning**: Optimize queries and connection management for your scale

**Key Files to Review:**
- `BigQueryConnection.cs` - Authentication and API communication patterns
- `FieldMappingHelper.cs` - Schema mapping examples
- `BigQueryConfigurationHelper.cs` - Configuration management approach
- Plugin files (`*Plugin.cs`) - CRUD operation implementations

## 📖 **Step-by-Step Implementation Guide**

### **Phase 1: Provider Development**
- [ ] Set up development environment
- [ ] Create plugin classes
- [ ] Implement field mappings
- [ ] Add configuration management

### **Phase 2: Provider Registration**
- [ ] Build and package solution
- [ ] Register plugins in Dataverse

### **Phase 3: Data Source Configuration**
- [ ] Create Virtual Table Data Source
- [ ] Configure parameters such as Project ID, Dataset ID, Table ID, and Service Account JSON

### **Phase 4: Virtual Table Creation**
- [ ] Create Virtual Table entity
- [ ] Add field definitions
- [ ] Test CRUD operations

### **Phase 5: Validation**
- [ ] Verify data retrieval
- [ ] Test record creation
- [ ] Update and delete records
- [ ] Monitor plugin trace logs for errors

## 📋 **Configuration Parameters**

**The following configuration parameters must be set in your Dataverse environment:**

| Parameter | Description | Required |
|-----------|-------------|----------|
| `mcd_bqProjectId` | Google Cloud Project ID | ✅ |
| `mcd_bqDatasetId` | BigQuery Dataset Name | ✅ |
| `mcd_bqTableId` | BigQuery Table Name | ✅ |
| `mcd_bqServiceAccountJson` | Service Account Credentials | ✅ |
| `mcd_bqBaseUrl` | BigQuery API Endpoint | ❌ (default provided) |
| `mcd_googleTokenUrl` | OAuth Token Endpoint | ❌ (default provided) |

The parameter names will likely be different for your environment, but they need to be set as columns in the data 
source virtual table as referenced in this blog post: 
https://www.itaintboring.com/dynamics-crm/how-do-we-pass-configuration-settings-to-the-virtual-entity-plugins/ by Alex Shlega.

## 💡 **Example Usage**

### **Field Mapping Configuration**

The following examples will differ in your environment based on your BigQuery schema and the columns you create 
in your virtual table as documented here: 
https://learn.microsoft.com/en-us/power-apps/developer/data-platform/virtual-entities/sample-ve-provider-crud-operations#step-3-creating-a-virtual-table-in-dataverse-environment

```csharp
// BigQuery → Dataverse mapping
{ "row_id", new FieldMapping("new_bqscheduleid", FieldType.Guid) },
{ "game_date", new FieldMapping("new_gamedate", FieldType.DateTime) },
{ "home_team", new FieldMapping("new_hometeam", FieldType.String) },
{ "attendance", new FieldMapping("new_attendance", FieldType.Integer) }
```
In particular, ensure that your primary key field in Dataverse (e.g., `new_bqscheduleid`) is mapped to a unique identifier in BigQuery.

## 🔧 **Advanced Configuration**

### **Custom Field Mappings**
Extend `FieldMappingHelper.cs` to support your specific BigQuery schema:

```csharp
private static readonly Dictionary<string, FieldMapping> _fieldMappings = 
    new Dictionary<string, FieldMapping>(StringComparer.OrdinalIgnoreCase)
{
    { "your_bigquery_field", new FieldMapping("new_dataversefield", FieldType.String) },
    // Add your mappings here
};
```

### **Error Handling & Monitoring**
- All operations include comprehensive tracing
- Failed operations provide detailed error context
- Connection health monitoring and automatic retry logic

## 🤝 **Contributing**

We welcome contributions! Here's how to get started:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow the existing code patterns and add tests
4. Commit your changes (`git commit -m 'Add amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

### **Development Guidelines**
- Follow C# coding conventions
- Add comprehensive tracing for new features
- Update field mappings documentation
- Include error handling for all external calls

## 📚 **Documentation**

- [BigQuery REST API Reference](https://cloud.google.com/bigquery/docs/reference/rest)
- [Dataverse Virtual Tables Guide](https://docs.microsoft.com/en-us/powerapps/developer/data-platform/virtual-entities/)
- [Plugin Development Best Practices](https://docs.microsoft.com/en-us/powerapps/developer/data-platform/best-practices/)

## 🚨 **Security Considerations**

- **Credential Management**: Service account credentials are securely stored in Dataverse configuration
- **Sandbox Compatibility**: All code runs within Dataverse sandbox restrictions
- **JWT Security**: Implements RS256 signing with proper token expiration
- **Input Validation**: All BigQuery operations include SQL injection protection

## 📈 **Performance Optimization**

- **Connection Reuse**: HTTP client pooling for BigQuery connections
- **Token Caching**: JWT tokens cached until near expiration
- **Efficient Queries**: Optimized BigQuery SQL generation
- **Batch Operations**: Support for bulk data operations

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙋‍♂️ **Support**

- 📧 Create an issue for bug reports or feature requests
- 💬 Discussions for questions and community support
- 📖 Check the [Wiki](../../wiki) for detailed documentation

---

**⭐ If this project helps you, please consider giving it a star!**

*Built with ❤️ for the Power Platform community*
