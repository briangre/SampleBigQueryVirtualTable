using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

public class BigQueryConnection
{
    private readonly ITracingService _tracingService;
    private readonly HttpClient _httpClient;
    private string _accessToken;
    private DateTime _tokenExpiry;

    // Configuration loaded from IEntityDataSourceRetrieverService
    private readonly BigQueryConfiguration _configuration;

    public BigQueryConnection(IServiceProvider serviceProvider, ITracingService tracingService)
    {
        _tracingService = tracingService;
        _httpClient = new HttpClient();
        
        try
        {
            // Load configuration from IEntityDataSourceRetrieverService
            _configuration = BigQueryConfigurationHelper.GetConfiguration(serviceProvider, tracingService);
            
            InitializeAuthentication();
            _tracingService.Trace("BigQuery REST connection initialized successfully from configuration service.");
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"BigQuery REST connection initialization failed: {ex.Message}");
            throw;
        }
    }

    private void InitializeAuthentication()
    {
        _accessToken = GetAccessToken();
    }

    private string GetAccessToken()
    {
        try
        {
            // Check if token is still valid (with 5 min buffer)
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            {
                return _accessToken;
            }

            var serviceAccountInfo = JsonConvert.DeserializeObject<JObject>(_configuration.ServiceAccountJson);
            var clientEmail = serviceAccountInfo["client_email"].ToString();
            var privateKey = serviceAccountInfo["private_key"].ToString();

            // Generate JWT
            var jwt = GenerateJwtToken(clientEmail, privateKey);
            
            // Exchange JWT for access token
            var accessToken = ExchangeJwtForAccessToken(jwt);
            
            _tracingService.Trace("Access token obtained successfully.");
            return accessToken;
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"Failed to get access token: {ex.Message}");
            throw;
        }
    }

    private string GenerateJwtToken(string clientEmail, string privateKey)
    {
        try
        {
            // JWT Header
            var header = new
            {
                alg = "RS256",
                typ = "JWT"
            };

            // JWT Payload
            var now = DateTimeOffset.UtcNow;
            var payload = new
            {
                iss = clientEmail,
                scope = "https://www.googleapis.com/auth/bigquery",
                aud = _configuration.TokenUrl, // Now using configuration value
                exp = now.AddHours(1).ToUnixTimeSeconds(),
                iat = now.ToUnixTimeSeconds()
            };

            var headerJson = JsonConvert.SerializeObject(header);
            var payloadJson = JsonConvert.SerializeObject(payload);

            var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
            var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            var unsignedToken = $"{headerBase64}.{payloadBase64}";

            // Sign the token using RSA
            var signature = SignData(unsignedToken, privateKey);
            var signatureBase64 = Base64UrlEncode(signature);

            return $"{unsignedToken}.{signatureBase64}";
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"JWT generation failed: {ex.Message}");
            throw;
        }
    }

    private byte[] SignData(string data, string privateKeyPem)
    {
        try
        {
            // Parse the private key from PEM format
            var rsa = DecodeRSAPrivateKey(privateKeyPem);
            
            // Sign the data
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"RSA signing failed: {ex.Message}");
            throw;
        }
    }

    private RSA DecodeRSAPrivateKey(string privateKeyPem)
    {
        // Remove PEM headers and decode base64
        var privateKeyText = privateKeyPem
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "");

        var privateKeyBytes = Convert.FromBase64String(privateKeyText);

        // Parse PKCS#8 format manually for .NET Framework 4.6.2
        var rsa = RSA.Create();
        
        try
        {
            // For .NET Framework 4.6.2, we need to manually parse the PKCS#8 format
            var rsaParameters = ParsePkcs8PrivateKey(privateKeyBytes);
            rsa.ImportParameters(rsaParameters);
            return rsa;
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"Failed to parse private key: {ex.Message}");
            rsa.Dispose();
            throw;
        }
    }

    private RSAParameters ParsePkcs8PrivateKey(byte[] pkcs8)
    {
        // This is a simplified PKCS#8 parser for the specific format used by Google Service Accounts
        // Based on the gist implementation for .NET Framework compatibility
        
        try
        {
            var reader = new BinaryReader(new MemoryStream(pkcs8));
            
            // Skip PKCS#8 wrapper
            reader.ReadByte(); // SEQUENCE
            ReadLength(reader); // Total length
            reader.ReadByte(); // INTEGER (version)
            reader.ReadByte(); // Length of version
            reader.ReadByte(); // Version value (0)
            
            // Algorithm identifier
            reader.ReadByte(); // SEQUENCE
            var algIdLength = ReadLength(reader);
            reader.ReadBytes(algIdLength); // Skip algorithm identifier
            
            // Private key
            reader.ReadByte(); // OCTET STRING
            var privateKeyLength = ReadLength(reader);
            var privateKeyBytes = reader.ReadBytes(privateKeyLength);
            
            // Now parse the actual RSA private key
            return ParseRSAPrivateKey(privateKeyBytes);
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"PKCS#8 parsing failed: {ex.Message}");
            throw;
        }
    }

    private RSAParameters ParseRSAPrivateKey(byte[] rsaPrivateKey)
    {
        var reader = new BinaryReader(new MemoryStream(rsaPrivateKey));
        
        reader.ReadByte(); // SEQUENCE
        ReadLength(reader); // Total length
        reader.ReadByte(); // INTEGER (version)
        reader.ReadByte(); // Length
        reader.ReadByte(); // Version value (0)
        
        var modulus = ReadInteger(reader);
        var publicExponent = ReadInteger(reader);
        var privateExponent = ReadInteger(reader);
        var prime1 = ReadInteger(reader);
        var prime2 = ReadInteger(reader);
        var exponent1 = ReadInteger(reader);
        var exponent2 = ReadInteger(reader);
        var coefficient = ReadInteger(reader);
        
        return new RSAParameters
        {
            Modulus = modulus,
            Exponent = publicExponent,
            D = privateExponent,
            P = prime1,
            Q = prime2,
            DP = exponent1,
            DQ = exponent2,
            InverseQ = coefficient
        };
    }

    private byte[] ReadInteger(BinaryReader reader)
    {
        reader.ReadByte(); // INTEGER tag
        var length = ReadLength(reader);
        var data = reader.ReadBytes(length);
        
        // Remove leading zero if present
        if (data.Length > 1 && data[0] == 0x00)
        {
            var result = new byte[data.Length - 1];
            Array.Copy(data, 1, result, 0, result.Length);
            return result;
        }
        
        return data;
    }

    private int ReadLength(BinaryReader reader)
    {
        var length = reader.ReadByte();
        if ((length & 0x80) == 0)
        {
            return length;
        }
        
        var lengthBytes = length & 0x7F;
        var result = 0;
        for (int i = 0; i < lengthBytes; i++)
        {
            result = (result << 8) | reader.ReadByte();
        }
        
        return result;
    }

    private string ExchangeJwtForAccessToken(string jwt)
    {
        try
        {
            var requestBody = new Dictionary<string, string>
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"},
                {"assertion", jwt}
            };

            var formContent = new FormUrlEncodedContent(requestBody);

            // Now using configuration value for token URL
            var response = _httpClient.PostAsync(_configuration.TokenUrl, formContent).GetAwaiter().GetResult();
            var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonConvert.DeserializeObject<JObject>(responseContent);
                var accessToken = tokenResponse["access_token"].ToString();
                var expiresIn = int.Parse(tokenResponse["expires_in"].ToString());
                
                _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
                _accessToken = accessToken;
                
                return accessToken;
            }
            else
            {
                throw new Exception($"Token exchange failed: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"Token exchange failed: {ex.Message}");
            throw;
        }
    }

    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    public void Trace(string message)
    {
        _tracingService?.Trace(message);
    }

    public async Task<JObject> ExecuteQueryAsync(string query)
    {
        try
        {
            // Ensure we have a valid access token
            _accessToken = GetAccessToken();
            
            _tracingService.Trace($"Executing query: {query}");
            
            var requestBody = new
            {
                query = query,
                useLegacySql = false
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Now using configuration value for base URL
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_configuration.BaseUrl}/projects/{_configuration.ProjectId}/queries")
            {
                Content = content
            };
            
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                _tracingService.Trace("Query executed successfully.");
                return JsonConvert.DeserializeObject<JObject>(responseContent);
            }
            else
            {
                _tracingService.Trace($"Query failed: {response.StatusCode} - {responseContent}");
                throw new Exception($"BigQuery API error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"Query execution failed: {ex.Message}");
            throw;
        }
    }

    public async Task<JObject> InsertRowAsync(string tableName, JObject rowData)
    {
        try
        {
            // Ensure we have a valid access token
            _accessToken = GetAccessToken();
            
            _tracingService.Trace($"Inserting row into table: {tableName}");
            
            var requestBody = new
            {
                rows = new[]
                {
                    new { json = rowData }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Now using configuration value for base URL
            var request = new HttpRequestMessage(HttpMethod.Post, 
                $"{_configuration.BaseUrl}/projects/{_configuration.ProjectId}/datasets/{_configuration.DatasetId}/tables/{tableName}/insertAll")
            {
                Content = content
            };
            
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                _tracingService.Trace("Row inserted successfully.");
                return JsonConvert.DeserializeObject<JObject>(responseContent);
            }
            else
            {
                _tracingService.Trace($"Insert failed: {response.StatusCode} - {responseContent}");
                throw new Exception($"BigQuery API error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _tracingService.Trace($"Row insertion failed: {ex.Message}");
            throw;
        }
    }

    // Synchronous wrappers for compatibility
    public JObject ExecuteQuery(string query)
    {
        return ExecuteQueryAsync(query).GetAwaiter().GetResult();
    }

    public void InsertRow(string tableName, JObject rowData)
    {
        InsertRowAsync(tableName, rowData).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    // Public properties to access configuration values if needed
    public string GetProjectId() => _configuration.ProjectId;
    public string GetDatasetId() => _configuration.DatasetId;
    public string GetTableId() => _configuration.TableId;
    public string GetBaseUrl() => _configuration.BaseUrl;
    public string GetTokenUrl() => _configuration.TokenUrl;
}