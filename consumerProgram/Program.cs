// 1st payment integration
using ARMCommon.Helpers;
using ARMCommon.Helpers.RabbitMq;
using ARMCommon.Interface;
using ARMCommon.Model;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula.Functions;
using RabbitMQ.Client;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using static StackExchange.Redis.Role;

namespace Program
{
    class Program
    {
        private static IHttpClientFactory _httpClientFactory;
        private static IConfiguration configuration;
        private static string paymentprocessurl;
        private static string company;
        private static string cbcreference;

        static void Main(string[] args)
        {
            try
            {
                var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = File.ReadAllText(appSettingsPath);
                dynamic config = JsonConvert.DeserializeObject(json);
                string queueName = config.AppConfig["QueueName"];

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                configuration = builder.Build();
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection, configuration);
                var serviceProvider = serviceCollection.BuildServiceProvider();
                var rabbitMQConsumer = serviceProvider.GetService<IRabbitMQConsumer>();
                var rabbitMQProducer = serviceProvider.GetService<IRabbitMQProducer>();
                _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                IPAddress ipv4Address = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                string ipv4AddressString = ipv4Address?.ToString();



                async Task<string> OnConsuming(string message)
                {
                    try
                    {
                        WriteMessage("OnConsuming method called with message " + message);

                        JObject jobobj = JObject.Parse(message);
                        string queueData = GetTokenIgnoreCase(jobobj, "queuedata")?.ToString();

                        if (queueData != null)
                        {
                            string queueDataEscape = queueData.Replace("\r\n", "").Replace("\\", "").Trim('"');
                            jobobj = JsonConvert.DeserializeObject<JObject>(queueDataEscape);

                            string masterid = GetTokenIgnoreCase(jobobj, "payload")?["submitdata"]?["dataarray"]?["data"]?["dc1"]?["row1"]?["masterid"]?.ToString();
                            string company = GetTokenIgnoreCase(jobobj, "payload")?["submitdata"]?["dataarray"]?["data"]?["dc1"]?["row1"]?["companyId"]?.ToString();
                            string project = GetTokenIgnoreCase(jobobj, "payload")?["submitdata"]?["project"]?.ToString();
                            string _accessTokenUrl = configuration["AppConfig:AccessTokenUrl"];
                            string paymentUrl = configuration["AppConfig:PaymentRequestUrl"];
                            string paymentProcessUrl = configuration["AppConfig:PaymentProcessUrl"];
                            string sourcefrom = configuration["AppConfig:sourcefrom"];
                            string _username = configuration["AppConfig:username"];
                            string _password = configuration["AppConfig:password"];
                            string _clientid = configuration["AppConfig:client_id"];
                            string _scope = configuration["AppConfig:scope"];
                            string _granttypye = configuration["AppConfig:grant_type"];
                            string _clientsecret = configuration["AppConfig:client_secret"];
                            string logouturl = configuration["AppConfig:LogOutUrl"];
                            var requestId = Guid.NewGuid().ToString();

                            var auth = new AccessTokenRequestStruct
                            {
                                username = _username,
                                password = _password,
                                client_id = _clientid,
                                scope = _scope,
                                grant_type = _granttypye,
                                client_secret = _clientsecret
                            };

                            if (string.IsNullOrWhiteSpace(masterid) || string.IsNullOrWhiteSpace(company) || string.IsNullOrWhiteSpace(project))
                            {
                                WriteMessage("Required fields are missing in the queue data.");
                                return "Error: Missing fields";
                            }

                            // Get Token and Request Data
                            var aceesstokenresult = await GetToken(masterid, auth, _accessTokenUrl, queueDataEscape, ipv4AddressString, project, requestId, sourcefrom);
                            string token = aceesstokenresult.token;
                            string refresh_token = aceesstokenresult.refreshtoken;

                            if (string.IsNullOrWhiteSpace(token))
                            {
                                WriteMessage("Token is null or empty.");
                                return "Error: Token is missing";
                            }

                            WriteMessage($"Access Token API Result: {aceesstokenresult}");

                            // 2nd API Call
                            var getSqlData = await GetPaymentSqlData(project, masterid);
                            JObject jsonObject = JObject.Parse(getSqlData);
                            string cbcReference = jsonObject["cbcReference"]?.ToString();

                            try
                            {
                                var paymentRequestApiResult = await PaymentRequestAPI(masterid, project, token, paymentUrl, getSqlData, ipv4AddressString, sourcefrom);
                                var responseString = paymentRequestApiResult.PaymentRequestData.responsestring;
                                var statuscode = paymentRequestApiResult.PaymentRequestData?.statuscode;
                                var success = paymentRequestApiResult.PaymentRequestData?.success;


                                WriteMessage($"Payment Request API Result: {JsonConvert.SerializeObject(responseString, Formatting.Indented)}");

                                if (statuscode == 200 && success == true)
                                {
                                    try
                                    {
                                        var paymentProcessApiResult = await PaymentProcessAPI(project, masterid, token, paymentProcessUrl, company, cbcReference, ipv4AddressString, sourcefrom);

                                        var status = paymentProcessApiResult?.PaymentProcessData?.success.ToString();

                                        if (status != "fail")
                                        {
                                            var responseStringPaymentProcess = paymentProcessApiResult?.PaymentProcessData?.responsestring;
                                            string paymentProcessSerializedResponse = JsonConvert.SerializeObject(responseStringPaymentProcess, Formatting.Indented);

                                            WriteMessage($"Payment Process API Result: {paymentProcessSerializedResponse}");
                                        }
                                        else
                                        {
                                            WriteMessage("Payment process status is 'fail'; not calling the third API.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteMessage($"An error occurred while processing PaymentProcessAPI: {ex.Message}{ex.StackTrace}");
                                    }
                                }
                                else
                                {
                                    if (statuscode != 200)
                                    {
                                        WriteMessage($"Payment Request API Failed with status code {statuscode}. Skipping Payment Process API call.");
                                    }
                                    else if (success == false)
                                    {
                                        WriteMessage("Payment Request API returned success = false. Skipping Payment Process API call.");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteMessage($"An error occurred while processing PaymentRequestAPI: {ex.Message}\n{ex.StackTrace}");
                                await LogoutAPI(logouturl, _clientid, refresh_token, _clientsecret, sourcefrom, masterid, ipv4AddressString, project);
                                WriteMessage("Logout Successfully");
                            }
                            await LogoutAPI(logouturl, _clientid, refresh_token, _clientsecret, sourcefrom, masterid, ipv4AddressString, project);
                            WriteMessage("Logout Successfully");

                            return "";
                        }
                        else
                        {
                            WriteMessage("Queue data is null or not properly formatted.");
                            return "";
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteMessage("Error in OnConsuming method: " + ex.Message);
                        throw;
                    }



                }

                static JToken GetTokenIgnoreCase(JObject jObject, string propertyName)
                {
                    var property = jObject.Properties()
                        .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                    return property?.Value;
                }



                //1st api
                async Task<TokenResult> GetToken(string masterid, AccessTokenRequestStruct auth, string accessTokenUrl, string queueDataEscape, string ipv4AddressString, string project, string requestId, string sourcefrom)
                {
                    try
                    {
                        var client = _httpClientFactory.CreateClient("SecureClient");

                        var formContent = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("username", auth.username),
                            new KeyValuePair<string, string>("password", auth.password),
                            new KeyValuePair<string, string>("client_id", auth.client_id),
                            new KeyValuePair<string, string>("scope", auth.scope),
                            new KeyValuePair<string, string>("grant_type", auth.grant_type),
                            new KeyValuePair<string, string>("client_secret", auth.client_secret)
                        });


                        var requestData = new AxRequest
                        {
                            requestid = requestId,
                            requestreceivedtime = DateTime.Now,
                            sourcefrom = sourcefrom,
                            requeststring = queueDataEscape,
                            headers = formContent.Headers.ToString(),
                            @params = "NULL",
                            authz = "NULL",
                            contenttype = formContent.Headers.ContentType?.ToString(),
                            contentlength = formContent.Headers.ContentLength?.ToString(),
                            host = new Uri(accessTokenUrl).Host,
                            url = accessTokenUrl,
                            endpoint = new Uri(accessTokenUrl).AbsolutePath,
                            requestmethod = "POST",
                            username = auth.username,
                            additionaldetails = masterid,
                            sourcemachineip = ipv4AddressString,
                            apiname = "AccessToken"
                        };

                        await InsertRequestData(project, requestData.requestid, DateTimeOffset.Now, requestData.sourcefrom, requestData.requeststring, requestData.headers, requestData.@params, requestData.authz, requestData.contenttype, requestData.contentlength, requestData.host, requestData.url, requestData.endpoint, requestData.requestmethod, requestData.username, requestData.additionaldetails, requestData.sourcemachineip, requestData.apiname);

                        var startTime = DateTime.Now;
                        HttpResponseMessage response;

                        try
                        {
                            response = await client.PostAsync(accessTokenUrl, formContent);
                        }
                        catch (HttpRequestException httpEx)
                        {

                            WriteMessage("Request error: " + httpEx.Message);
                            throw;
                        }

                        var endTime = DateTime.Now;
                        var executionTime = (endTime - startTime).TotalMilliseconds.ToString() + "ms";
                        var responseContent = await response.Content.ReadAsStringAsync();


                        string accessToken = null;
                        string refreshtoken = null;
                        try
                        {
                            var json = JObject.Parse(responseContent);
                            accessToken = json["access_token"]?.ToString();
                            refreshtoken = json["refresh_token"]?.ToString();

                        }
                        catch (JsonException jsonEx)
                        {

                            WriteMessage("JSON parsing error: " + jsonEx.Message);

                            throw;
                        }
                        var responseData = new AxRequest
                        {
                            responseid = requestId,
                            responsesenttime = DateTimeOffset.Now.DateTime,
                            statuscode = (int)response.StatusCode,
                            responsestring = responseContent,
                            headers = response.Headers.ToString(),
                            contenttype = response.Content.Headers.ContentType?.ToString(),
                            contentlength = response.Content.Headers.ContentLength?.ToString(),
                            errordetails = response.IsSuccessStatusCode ? "" : response.ReasonPhrase,
                            endpoint = new Uri(accessTokenUrl).AbsolutePath,
                            requestmethod = "POST",
                            username = auth.username,
                            additionaldetails = masterid,
                            requestid = requestId,
                            executiontime = executionTime
                        };


                        await InsertResponseData(project, responseData.responseid, DateTimeOffset.Now, responseData.statuscode, responseData.responsestring, responseData.headers, responseData.contenttype, responseData.contentlength, responseData.errordetails, responseData.endpoint, responseData.requestmethod, responseData.username, responseData.additionaldetails, responseData.requestid, responseData.executiontime);

                        return new TokenResult
                        {
                            token = accessToken,
                            refreshtoken = refreshtoken,
                            RequestData = requestData
                        };
                    }
                    catch (Exception ex)
                    {
                        WriteMessage("Error in GetToken: " + ex.Message);
                        throw;
                    }
                }



                static async Task<string> GetPaymentSqlData(string project, string masterid)
                {
                    try
                    {
                        var context = new ARMCommon.Helpers.DataContext(configuration);
                        var redis = new RedisHelper(configuration);
                        var utils = new Utils(configuration, context, redis);
                        Dictionary<string, string> config = await utils.GetDBConfigurations(project);
                        string connectionString = config["ConnectionString"];
                        string dbType = config["DBType"];
                        string sql = Constants_SQL.PAYMENTSQL;
                        string[] paramNames = { "@masterid" };
                        DbType[] paramTypes = { DbType.Int64 };
                        object[] paramValues = { long.Parse(masterid) };

                        IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramNames, paramTypes, paramValues);
                        DataTable dataTable = await dbHelper.ExecuteQueryAsync(sql, connectionString, paramNames, paramTypes, paramValues);

                        if (dataTable.Rows.Count > 0)
                        {
                            string jsonString = dataTable.Rows[0]["f_get_api_json"].ToString();
                            jsonString = jsonString.Trim();

                            try
                            {
                                JObject.Parse(jsonString);
                                return jsonString;
                            }
                            catch (JsonException ex)
                            {
                                throw new InvalidOperationException("Invalid JSON format received.", ex);
                            }
                        }
                        return null;
                    }
                    catch (Exception ex)
                    {
                        WriteMessage($"Error in GetPaymentSqlData: {ex.Message}");
                        throw;
                    }
                }


                async Task InsertRequestData(string project, string requestid, DateTimeOffset requestreceivedtime, string sourcefrom, string requeststring, string headers, string @params, string authz, string contenttype, string contentlength, string host, string url, string endpoint, string requestmethod, string username, string additionaldetails, string sourcemachineip, string apiname)
                {
                    try
                    {
                        var context = new ARMCommon.Helpers.DataContext(configuration);
                        var redis = new RedisHelper(configuration);
                        var utils = new Utils(configuration, context, redis);
                        Dictionary<string, string> config = await utils.GetDBConfigurations(project);
                        string connectionString = config["ConnectionString"];
                        string dbType = config["DBType"];
                        string sql = Constants_SQL.INSERTREQUESTSTRING;
                        sql = string.Format(sql, requestid, requestreceivedtime.ToString("yyyy-MM-dd HH:mm:ss.fff"), sourcefrom, requeststring, headers, @params, authz, contenttype, contentlength, host, url, endpoint, requestmethod, username, additionaldetails, sourcemachineip, apiname);
                        IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, new string[] { }, new DbType[] { }, new object[] { });
                        await dbHelper.ExecuteQueryAsync(sql, connectionString, new string[] { }, new DbType[] { }, new object[] { });
                    }
                    catch (Exception ex)
                    {
                        WriteMessage($"Error in InsertRequestData for {apiname}: {ex.Message}");
                        throw;
                    }
                }

                async Task InsertResponseData(string project, string responseid, DateTimeOffset responsesenttime, int statuscode, string responsestring, string headers, string contenttype, string contentlength, string errordetails, string endpoint, string requestmethod, string username, string additionaldetails, string requestid, string executiontime)
                {
                    try
                    {
                        var context = new ARMCommon.Helpers.DataContext(configuration);
                        var redis = new RedisHelper(configuration);
                        var utils = new Utils(configuration, context, redis);
                        Dictionary<string, string> config = await utils.GetDBConfigurations(project);
                        string connectionString = config["ConnectionString"];
                        string dbType = config["DBType"];
                        string sql = Constants_SQL.INSERTRESPONSESTRING;
                        sql = string.Format(sql, responseid, responsesenttime.ToString("yyyy-MM-dd HH:mm:ss.fff"), statuscode, responsestring, headers, contenttype, contentlength, errordetails, endpoint, requestmethod, username, additionaldetails, requestid, executiontime);
                        IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, new string[] { }, new DbType[] { }, new object[] { });
                        await dbHelper.ExecuteQueryAsync(sql, connectionString, new string[] { }, new DbType[] { }, new object[] { });
                    }
                    catch (Exception ex)
                    {
                        WriteMessage($"Error in InsertResponseData : {ex.Message}");
                        throw;
                    }
                }


                async Task<TokenResult> PaymentRequestAPI(string masterid, string project, string token, string paymentUrl, string json2, string ipv4AddressString, string sourcefrom)
                {
                    var reqID = Guid.NewGuid().ToString();
                    var client = _httpClientFactory.CreateClient("SecureClient");

                    var requestData = new AxRequest
                    {
                        requestid = reqID,
                        requestreceivedtime = DateTime.Now,
                        sourcefrom = sourcefrom,
                        requeststring = json2,
                        headers = "NULL",
                        @params = "NULL",
                        authz = token,
                        contenttype = "application/json",
                        contentlength = json2.Length.ToString(),
                        host = new Uri(paymentUrl).Host,
                        url = paymentUrl,
                        endpoint = new Uri(paymentUrl).AbsolutePath,
                        requestmethod = "POST",
                        username = "NULL",
                        additionaldetails = masterid,
                        sourcemachineip = ipv4AddressString,
                        apiname = "PaymentRequest",
                    };

                    await InsertRequestData(project, requestData.requestid, DateTimeOffset.Now, requestData.sourcefrom, requestData.requeststring, requestData.headers, requestData.@params, requestData.authz, requestData.contenttype, requestData.contentlength, requestData.host, requestData.url, requestData.endpoint, requestData.requestmethod, requestData.username, requestData.additionaldetails, requestData.sourcemachineip, requestData.apiname);

                    HttpResponseMessage postPaymentResponse = null;
                    string postPaymentResponseContent = string.Empty;
                    string executionTime = string.Empty;
                    string errorDetails = string.Empty;

                    try
                    {

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var httpContent = new StringContent(json2, Encoding.UTF8, "application/json");

                        var startTime = DateTime.UtcNow;
                        try
                        {
                            postPaymentResponse = await client.PostAsync(paymentUrl, httpContent);
                        }
                        catch (HttpRequestException httpEx)
                        {
                            WriteMessage("Request error: " + httpEx.Message);
                            errorDetails = httpEx.Message;
                            throw;
                        }
                        var endTime = DateTime.UtcNow;
                        var executionDuration = endTime - startTime;
                        executionTime = Math.Round(executionDuration.TotalMilliseconds).ToString() + "ms";

                        postPaymentResponseContent = await postPaymentResponse.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        WriteMessage("Error in PaymentRequestAPI: " + ex.Message);
                        errorDetails = ex.Message;
                    }
                    finally
                    {
                        var responseData = new AxRequest
                        {
                            requestid = reqID,
                            requestreceivedtime = requestData.requestreceivedtime,
                            sourcefrom = requestData.sourcefrom,
                            requeststring = requestData.requeststring,
                            headers = postPaymentResponse?.Headers.ToString() ?? "",
                            authz = token,
                            @params = "NULL",
                            contenttype = postPaymentResponse?.Content.Headers.ContentType?.ToString() ?? "",
                            contentlength = postPaymentResponse?.Content.Headers.ContentLength?.ToString() ?? "",
                            host = new Uri(paymentUrl).Host,
                            url = paymentUrl,
                            endpoint = new Uri(paymentUrl).AbsolutePath,
                            requestmethod = requestData.requestmethod,
                            username = "NULL",
                            additionaldetails = masterid,
                            sourcemachineip = requestData.sourcemachineip,
                            apiname = requestData.apiname,
                            responseid = reqID,
                            responsesenttime = DateTimeOffset.Now.DateTime,
                            responsestring = postPaymentResponseContent,
                            statuscode = (int)(postPaymentResponse?.StatusCode ?? HttpStatusCode.InternalServerError),
                            executiontime = executionTime,
                            errordetails = errorDetails
                        };

                        await InsertResponseData(project, responseData.responseid, DateTimeOffset.Now, responseData.statuscode, responseData.responsestring, responseData.headers, responseData.contenttype, responseData.contentlength, responseData.errordetails, responseData.endpoint, responseData.requestmethod, responseData.username, responseData.additionaldetails, responseData.requestid, responseData.executiontime);
                    }

                    return new TokenResult
                    {
                        PaymentRequestData = new AxRequest
                        {
                            requestid = reqID,
                            responsestring = postPaymentResponseContent,
                            statuscode = (int?)postPaymentResponse?.StatusCode ?? 500
                        }
                    };
                }




                async Task<TokenResult> PaymentProcessAPI(string masterid, string project, string token, string paymentProcessUrl, string company, string cbcreference, string ipv4AddressString, string sourcefrom)
                {
                    var requestJson = new
                    {
                        COMPANY = company,
                        CBCREF = cbcreference
                    };

                    string jsonString = JObject.FromObject(requestJson).ToString();

                    var rid = Guid.NewGuid().ToString();
                    var requestData = new AxRequest
                    {
                        requestid = rid,
                        requestreceivedtime = DateTime.Now,
                        sourcefrom = sourcefrom,
                        requeststring = jsonString,
                        authz = token,
                        headers = "NULL",
                        @params = "NULL",
                        contenttype = "application/json",
                        contentlength = jsonString.Length.ToString(),
                        host = new Uri(paymentProcessUrl).Host,
                        url = paymentProcessUrl,
                        endpoint = new Uri(paymentProcessUrl).AbsolutePath,
                        requestmethod = "POST",
                        username = "NULL",
                        additionaldetails = masterid,
                        sourcemachineip = ipv4AddressString,
                        apiname = "PaymentProcess",
                    };

                    await InsertRequestData(project, requestData.requestid, DateTimeOffset.Now, requestData.sourcefrom, requestData.requeststring, requestData.headers, requestData.@params, requestData.authz, requestData.contenttype, requestData.contentlength, requestData.host, requestData.url, requestData.endpoint, requestData.requestmethod, requestData.username, requestData.additionaldetails, requestData.sourcemachineip, requestData.apiname);

                    HttpResponseMessage postPaymentResponse = null;
                    string postPaymentResponseContent = string.Empty;
                    string executionTime = string.Empty;
                    string errorDetails = string.Empty;

                    try
                    {
                        var client = _httpClientFactory.CreateClient("SecureClient");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var httpContent = new StringContent(requestData.requeststring, Encoding.UTF8, "application/json");

                        var startTime = DateTime.UtcNow;
                        try
                        {
                            postPaymentResponse = await client.PostAsync(paymentProcessUrl, httpContent);
                        }
                        catch (HttpRequestException httpEx)
                        {
                            WriteMessage("Request error: " + httpEx.Message);
                            errorDetails = httpEx.Message;
                            throw;
                        }
                        var endTime = DateTime.UtcNow;
                        var executionDuration = endTime - startTime;
                        executionTime = Math.Round(executionDuration.TotalMilliseconds).ToString() + "ms";

                        postPaymentResponseContent = await postPaymentResponse.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        WriteMessage("Error in PaymentProcessAPI: " + ex.Message);
                        errorDetails = ex.Message;
                    }
                    finally
                    {
                        var responseData = new AxRequest
                        {
                            requestid = rid,
                            requestreceivedtime = requestData.requestreceivedtime,
                            sourcefrom = requestData.sourcefrom,
                            requeststring = requestData.requeststring,
                            headers = postPaymentResponse?.Headers.ToString() ?? "",
                            authz = token,
                            @params = "NULL",
                            contenttype = postPaymentResponse?.Content.Headers.ContentType?.ToString() ?? "",
                            contentlength = postPaymentResponse?.Content.Headers.ContentLength?.ToString() ?? "",
                            host = new Uri(paymentProcessUrl).Host,
                            url = paymentProcessUrl,
                            endpoint = new Uri(paymentProcessUrl).AbsolutePath,
                            requestmethod = requestData.requestmethod,
                            username = "NULL",
                            additionaldetails = masterid,
                            sourcemachineip = requestData.sourcemachineip,
                            apiname = requestData.apiname,
                            responseid = rid,
                            responsesenttime = DateTime.Now,
                            responsestring = postPaymentResponseContent,
                            statuscode = (int)(postPaymentResponse?.StatusCode ?? HttpStatusCode.InternalServerError),
                            executiontime = executionTime,
                            errordetails = errorDetails
                        };

                        await InsertResponseData(project, responseData.responseid, DateTimeOffset.Now, responseData.statuscode, responseData.responsestring, responseData.headers, responseData.contenttype, responseData.contentlength, responseData.errordetails, responseData.endpoint, responseData.requestmethod, responseData.username, responseData.additionaldetails, responseData.requestid, responseData.executiontime);
                    }

                    return new TokenResult
                    {
                        PaymentProcessData = new AxRequest
                        {
                            requestid = rid,
                            responsestring = postPaymentResponseContent,
                            statuscode = (int)(postPaymentResponse?.StatusCode ?? HttpStatusCode.InternalServerError)
                        }
                    };
                }



                async Task<TokenResult> LogoutAPI(string logouturl, string _clientid, string refresh_token, string _clientsecret, string sourcefrom, string masterid, string ipv4AddressString, string project)
                {
                    var rid = Guid.NewGuid().ToString();
                    var requestData = new AxRequest
                    {
                        requestid = rid,
                        requestreceivedtime = DateTime.Now,
                        sourcefrom = sourcefrom,
                        requeststring = "NULL",
                        authz = "NULL",
                        headers = "NULL",
                        @params = "NULL",
                        contenttype = "application/x-www-form-urlencoded",
                        contentlength = "NULL",
                        host = new Uri(logouturl).Host,
                        url = logouturl,
                        endpoint = new Uri(logouturl).AbsolutePath,
                        requestmethod = "POST",
                        username = "NULL",
                        additionaldetails = masterid,
                        sourcemachineip = ipv4AddressString,
                        apiname = "LogoutAPI",
                    };


                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", _clientid),
                        new KeyValuePair<string, string>("refresh_token", refresh_token),
                        new KeyValuePair<string, string>("client_secret", _clientsecret)
                    });

                    // Update content length
                    requestData.contentlength = content.Headers.ContentLength?.ToString() ?? "0";

                    HttpResponseMessage response = null;
                    string responseContent = string.Empty;
                    string executionTime = string.Empty;
                    string errorDetails = string.Empty;

                    // Insert request data
                    await InsertRequestData(project, requestData.requestid, DateTimeOffset.Now, requestData.sourcefrom, requestData.requeststring, requestData.headers, requestData.@params, requestData.authz, requestData.contenttype, requestData.contentlength, requestData.host, requestData.url, requestData.endpoint, requestData.requestmethod, requestData.username, requestData.additionaldetails, requestData.sourcemachineip, requestData.apiname);


                    try
                    {
                        var client = _httpClientFactory.CreateClient("SecureClient");

                        // Send the request and measure execution time
                        var startTime = DateTime.UtcNow;
                        response = await client.PostAsync(logouturl, content);
                        var endTime = DateTime.UtcNow;
                        executionTime = Math.Round((endTime - startTime).TotalMilliseconds).ToString() + "ms";

                        // Ensure a successful response
                        response.EnsureSuccessStatusCode();
                        responseContent = await response.Content.ReadAsStringAsync();
                    }
                    catch (HttpRequestException httpEx)
                    {
                        errorDetails = httpEx.Message;

                        throw;
                    }
                    finally
                    {
                        var responseData = new AxRequest
                        {
                            requestid = rid,
                            requestreceivedtime = requestData.requestreceivedtime,
                            sourcefrom = requestData.sourcefrom,
                            requeststring = requestData.requeststring,
                            headers = "NULL",
                            authz = "NULL", // Adjust as necessary
                            @params = "NULL",
                            contenttype = response?.Content.Headers.ContentType?.ToString() ?? "",
                            contentlength = response?.Content.Headers.ContentLength?.ToString() ?? "",
                            host = new Uri(logouturl).Host,
                            url = logouturl,
                            endpoint = new Uri(logouturl).AbsolutePath,
                            requestmethod = requestData.requestmethod,
                            username = "NULL",
                            additionaldetails = masterid,
                            sourcemachineip = ipv4AddressString,
                            apiname = requestData.apiname,
                            responseid = rid,
                            responsesenttime = DateTime.Now,
                            responsestring = responseContent,
                            statuscode = (int)(response?.StatusCode ?? HttpStatusCode.InternalServerError),
                            executiontime = executionTime,
                            errordetails = errorDetails
                        };


                        await InsertResponseData(project, responseData.responseid, DateTimeOffset.Now, responseData.statuscode, responseData.responsestring, responseData.headers, responseData.contenttype, responseData.contentlength, responseData.errordetails, responseData.endpoint, responseData.requestmethod, responseData.username, responseData.additionaldetails, responseData.requestid, responseData.executiontime);
                    }

                    return new TokenResult
                    {
                        LogoutData = new AxRequest
                        {
                            requestid = rid,
                            responsestring = responseContent,
                            statuscode = (int)(response?.StatusCode ?? HttpStatusCode.InternalServerError)
                        }
                    };
                }

                rabbitMQConsumer.DoConsume(queueName, OnConsuming);
            }
            catch (Exception ex)
            {
                WriteMessage("Unhandled exception: " + ex.Message);
            }
        }




        static void WriteMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + " - " + message);
        }




        static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
            services.AddSingleton<IRabbitMQProducer, RabbitMQProducer>();

            services.AddHttpClient("SecureClient")
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            return true;
                        }
                    };
                });

            services.AddHttpClient();
        }
    }
    public class AccessTokenRequestStruct
    {
        public string client_secret { get; set; }
        public string client_id { get; set; }
        public string grant_type { get; set; }
        public string password { get; set; }
        public string scope { get; set; }
        public string username { get; set; }
    }




    public class TokenResult
    {
        public string token { get; set; }

        public string refreshtoken { get; set; }
        public AxRequest RequestData { get; set; }
        public AxRequest PaymentRequestData { get; set; }
        public AxRequest PaymentProcessData { get; set; }

        public AxRequest LogoutData { get; set; }
    }


    public class PaymentRequestData
    {
        public string? status { get; set; }
        public string? success { get; set; }
    }


}