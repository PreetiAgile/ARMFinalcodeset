

using ARMCommon.Interface;
using ARMCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ARMCommon.Helpers
{
    public class API : IAPI
    {
        private readonly IConfiguration _config;
        public API(IConfiguration config) {
            _config = config;
        }

        public API()
        {
        }


        public async Task<ARMResult> GetData(string url)
        {
                  
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var request = await client.GetAsync(url);
                    request.EnsureSuccessStatusCode();

                    return new ARMResult(request.IsSuccessStatusCode, await request.Content.ReadAsStringAsync());
                }
                catch (Exception ex)
                {
                    return new ARMResult(false, ex.Message);

                }
            }
        }

        public async Task<ARMResult> POSTData(string url, string body, string Mediatype)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var request = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, Mediatype));
                    request.EnsureSuccessStatusCode();
                    var content = await request.Content.ReadAsStringAsync();
                    return new ARMResult(request.IsSuccessStatusCode, await request.Content.ReadAsStringAsync());

                }
                catch (Exception ex)
                {
                    return new ARMResult(false, ex.Message);
                }
            }
        }


        public async Task<string> CallAPI(string apiJson)
        {
            WriteMessage($"APIJSON-{apiJson}");
            var apiRequest = JsonConvert.DeserializeObject<ApiRequest>(apiJson);
            apiRequest.StartTime = DateTime.Now;
            using (HttpClient client = new HttpClient())
            {
                // Set headers
                if (apiRequest.HeaderString != null && apiRequest.HeaderString.Count > 0)
                {
                    foreach (var header in apiRequest.HeaderString)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpResponseMessage response;
                if (apiRequest.Url.IndexOf("{{") > -1)
                {
                    if (apiRequest.UrlParams != null && apiRequest.UrlParams.Count > 0)
                    {
                        foreach (var urlParam in apiRequest.UrlParams)
                        {
                            if (apiRequest.Url.ToLower().IndexOf("{{" + urlParam.Key.ToLower() + "}}") > -1)
                            {
                                apiRequest.Url = CaseInsensitiveReplace(apiRequest.Url, "{{" + urlParam.Key + "}}", urlParam.Value);
                            }

                        }
                    }
                }
                // Perform the HTTP request based on the method
                switch (apiRequest.Method.ToUpper())
                {
                    case "GET":
                        var url = $"{apiRequest.Url}";
                        if (apiRequest.ParameterString != null && apiRequest.ParameterString.Count > 0)
                        {
                            var queryString = string.Join("&", apiRequest.ParameterString.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                            url = $"{apiRequest.Url}?{queryString}";
                        }
                        else
                        {
                            url = apiRequest.Url;
                        }
                        Console.WriteLine($"GET-{url}");
                        response = await client.GetAsync(url);
                        break;
                    case "POST":
                        var content = new StringContent(JsonConvert.SerializeObject(apiRequest.RequestString), Encoding.UTF8, "application/json");
                        response = await client.PostAsync(apiRequest.Url, content);
                        break;
                    case "PUT":
                        var putContent = new StringContent(JsonConvert.SerializeObject(apiRequest.RequestString), Encoding.UTF8, "application/json");
                        response = await client.PutAsync(apiRequest.Url, putContent);
                        break;

                    default:
                        Console.WriteLine($"HTTP method {apiRequest.Method} not supported.");
                        throw new NotSupportedException($"HTTP method {apiRequest.Method} not supported.");
                }

                // Handle the response as needed
                var responseBody = await response.Content.ReadAsStringAsync();
                WriteMessage($"API Response:");
                Console.WriteLine($"Response Status Code: {response.StatusCode}");
                Console.WriteLine($"Response Body: {responseBody}");
                apiRequest.EndTime = DateTime.Now;
                apiRequest.Response = responseBody;
                if (response.IsSuccessStatusCode)
                    apiRequest.Status = "Success";
                else
                    apiRequest.Status = "Fail";

                await LogAPICall(apiRequest);
                return responseBody;
            }
        }

        public async Task LogAPICall(ApiRequest apiRequest)
        {
            if (string.IsNullOrEmpty(apiRequest.Project))
            {
                Console.WriteLine("Project details is missing in Json. Can't write logs to 'axapijobdetails' table.");
                return;
            }

            var context = new ARMCommon.Helpers.DataContext(_config);
            var redis = new RedisHelper(_config);
            Utils utils = new Utils(_config, context, redis);

            try
            {
                Dictionary<string, string> config = await utils.GetDBConfigurations(apiRequest.Project);
                string connectionString = config["ConnectionString"];
                string dbType = config["DBType"];
                string sql = Constants_SQL.INSERT_TO_APILOG;
                string currentTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                //sql = string.Format(sql, currentTime, currentTime, apiRequest.Url, apiRequest.Method, JsonConvert.SerializeObject(apiRequest.RequestString), JsonConvert.SerializeObject(apiRequest.ParameterString), JsonConvert.SerializeObject(apiRequest.HeaderString), JsonConvert.SerializeObject(apiRequest.Response), apiRequest.Status, apiRequest.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), apiRequest.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RMQ", (apiRequest.APIDesc ?? "ARMAPIService"));
                sql = string.Format(sql, currentTime, currentTime, apiRequest.Url, apiRequest.Method, (apiRequest.RequestString), (apiRequest.ParameterString), (apiRequest.HeaderString), (apiRequest.Response), apiRequest.Status, apiRequest.StartTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"), apiRequest.EndTime?.ToString("yyyy-MM-dd HH:mm:ss.fff"), "RMQ", (apiRequest.APIDesc ?? "ARMAPIService"));
                // "INSERT INTO axapijobdetails(jobid, requestid, url, method, requeststr, params, header, responsestr, status, starttime, endtime, context, servicename) as values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')"

                IDbHelper dbHelper = DBHelper.CreateDbHelper(dbType);
                var result = await dbHelper.ExecuteQueryAsync(sql, connectionString);
                WriteMessage($"API log is done. {JsonConvert.SerializeObject(result)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }

        static void WriteMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString() + " - " + message);
        }

        static JToken GetTokenIgnoreCase(JObject jObject, string propertyName)
        {
            // Find the property in a case-insensitive manner
            var property = jObject.Properties()
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            return property?.Value;
        }

        static string CaseInsensitiveReplace(string input, string searchString, string replacement)
        {
            string pattern = Regex.Escape(searchString);
            return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
        }
    }
}
