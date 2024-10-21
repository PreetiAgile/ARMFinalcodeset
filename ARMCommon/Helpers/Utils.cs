﻿using AgileConnect.EncrDecr.cs;
using ARM_APIs.Model;
using ARMCommon.Interface;
using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ARMCommon.Helpers
{
    public class Utils
    {
        private readonly IConfiguration _config;
        private readonly DataContext _context;
        private readonly IRedisHelper _redis;
        public Dictionary<string, string> _dictdbConnection = new Dictionary<string, string>();
        

        public Utils(IConfiguration config, DataContext context,  IRedisHelper redis)
        {
            _config = config;
            _context = context;
            _redis = redis;  
        }

        public Utils() { 
        
        }

        public string GetEncryptedSecret(string secretKey) {
            AES aes = new AES();
            string currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            return aes.EncryptString(currentTime, secretKey);
        }
        public string GetAppList()
        {
            var appList = _context.ARMApps.Select(u => u.AppName).ToList();

            string appnames = (string.Join(",", appList.Select(x => x.ToString()).ToArray()));

            return appnames;
        }
        public string GetDBConnections(string dbType, string dbuser, string password, string serverConnection, string appname, string ConnectionName = "")
        {
            string connectionString = "";
            string database = dbuser;

            if (dbType.ToLower() == "oracle") 
            {
                database = (ConnectionName != "" ? ConnectionName : appname);
                connectionString = @"Data Source=" + serverConnection + "/" + database + ";User Id=" + dbuser + ";Password=" + password + ";Pooling=False;";
                return connectionString;

            }


            else if (dbType.ToLower() == "postgresql" || dbType.ToLower() == "postgre")
            {
                if (serverConnection.Contains(":"))
                {
                    if (dbuser.Contains("\\") || database.Contains("\\"))
                    {

                        string[] userDtls = dbuser.Split('\\');
                        string[] databaseDtls = database.Split('\\');
                        if (userDtls.Length > 1 && userDtls[1] != "")
                            if ((userDtls.Length > 1 && userDtls[1] != "") && (databaseDtls.Length > 1 && databaseDtls[1] != ""))
                            {
                                dbuser = userDtls[0];
                                database = databaseDtls[1];


                            }
                            else
                            {
                                dbuser = userDtls[0];
                                database = databaseDtls[0];
                            }

                    }
                    else
                    {
                        database = ConnectionName;

                    }

                    //Default port for postgres is 5432. if it was changed we need to pass the port no seperately in the Conn Str. 
                    string serverPort = serverConnection.Substring(serverConnection.IndexOf(':') + 1);
                    connectionString = @"Server=" + serverConnection.Substring(0, serverConnection.IndexOf(':')) + "; Port=" + serverPort + "; Database=" + database + ";Username=" + dbuser + ";Pwd=" + password + ";Pooling=false;";
                    return connectionString;

                }
                else
                {
                    if (dbuser.Contains("\\") || database.Contains("\\"))
                    {
                        string[] userDtls = dbuser.Split('\\');
                        string[] databaseDtls = database.Split('\\');
                        if ((userDtls.Length > 1 && userDtls[1] != "") && (databaseDtls.Length > 1 && databaseDtls[1] != ""))
                        {
                            dbuser = userDtls[0];
                            database = databaseDtls[1];

                        }
                        else
                        {
                            dbuser = userDtls[0];
                            database = databaseDtls[0];

                        }

                    }
                    else
                    {
                        database = (ConnectionName != "" ? ConnectionName : appname);

                    }
                    connectionString = @"Server=" + serverConnection + ";Database=" + database + ";Username=" + dbuser + ";Pwd=" + password + ";Pooling=false;";
                    return connectionString;


                }


            }

            else return connectionString;
        }


        public string DecryptSecret(string cipherText, string key)
        {
            string result = "";
            AES aes = new AES();
            try
            {
                result = aes.DecryptString(cipherText, key);

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public bool IsValidAPITime(string inputTime)
        {
            if (inputTime.Length != 14 || !DateTime.TryParseExact(inputTime, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return false;
            }

            DateTime inputDateTime = DateTime.ParseExact(inputTime, "yyyyMMddHHmmss", null);
            DateTime currentTime = DateTime.Now;

            if (inputDateTime > currentTime)
                return false;

            TimeSpan timeDifference = currentTime - inputDateTime;
            return timeDifference.TotalSeconds < 180;
        }
        public string GetRedisConnections(string host, string password)
        {

            var redisConfig = new
            {
                RedisIP = host,
                Password = password
            };
            var json = JsonConvert.SerializeObject(redisConfig);
            return json;
        }


        public async Task<bool> ModulePageAccess(string pagename, string usergroup)
        {
            try
            {
                var page = await _context.AxModulePages.FirstOrDefaultAsync(p => p.PageName == pagename);
                if (page == null)
                {
                    return false;
                }
                List<string> accessControl = page.AcessControl;
                //if (accessControl.Contains(usergroup))
                if (accessControl.Any(s => s.Equals(usergroup, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public  bool HasNullOrEmptyValues(string json, params string[] fields)
        {
            JObject jObject = JsonConvert.DeserializeObject<JObject>(json);

            foreach (var field in fields)
            {
                if (jObject[field] == null || string.IsNullOrEmpty(jObject[field].ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<string> GetDBConfiguration(string appName)
        {
            string ConnectionString = "";
            try
            {
                string key = $"{Constants.DB_PREFIX.ARMConnectionString.ToString()}_{appName}";

                if (_redis.KeyExists(key))
                {
                    ConnectionString = GetARMDbConfiguration(appName);
                    _dictdbConnection.Add(appName, ConnectionString);
                    await _redis.KeyDeleteAsync(key);
                }
                else
                {
                    if (_dictdbConnection.ContainsKey(appName))
                    {
                        ConnectionString = GetARMDbConfiguration(appName);
                        _dictdbConnection[appName] = ConnectionString;
                    }
                    else
                    {
                        ConnectionString = GetARMDbConfiguration(appName);
                        _dictdbConnection.Add(appName, ConnectionString);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return ConnectionString;
        }
        public async Task<Dictionary<string, string>> GetDBConfigurations(string appName)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            try
            {
                string key = $"{Constants.DB_PREFIX.ARMConnectionString.ToString()}_{appName}";
                if (_redis.KeyExists(key))
                {
                    config = JsonConvert.DeserializeObject<Dictionary<string, string>>(_redis.StringGet(key));
                    return config;
                }

                string connectionString = _config[$"ConnectionStrings:{appName}_connectionstring"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = GetARMDbConfiguration(appName);
                }

                string dbType = _config[$"ConnectionStrings:{appName}_dbtype"];
                if (string.IsNullOrEmpty(dbType))
                {
                    var app = _context.ARMApps.FirstOrDefault(p => p.AppName.ToLower() == appName.ToLower());
                    dbType = EncrDecr.Decrypt(app.DataBase, app.PrivateKey);
                }

                config.Add("ConnectionString", connectionString);
                config.Add("DBType", dbType);
                await _redis.StringSetAsync(key, JsonConvert.SerializeObject(config));

                if (_dictdbConnection.ContainsKey(appName))
                {
                    _dictdbConnection[appName] = connectionString;
                }
                else
                {
                    _dictdbConnection.Add(appName, connectionString);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            return config;
        }

        public string GetARMDbConfiguration(string appName)
        { 
            var userGroup = _context.ARMApps.FirstOrDefault(p => p.AppName.ToLower() == appName.ToLower());
            if (userGroup == null)
                throw new KeyNotFoundException("App not found");
            var Key = userGroup.PrivateKey;
            if (Key != null)
            {
                try
                {
                    string ConnectionName = EncrDecr.Decrypt(userGroup.ConnectionName, Key);
                    string DataBase = EncrDecr.Decrypt(userGroup.DataBase, Key);
                    string DBVersion = EncrDecr.Decrypt(userGroup.DBVersion, Key);
                    string UserName = EncrDecr.Decrypt(userGroup.UserName, Key);
                    string Password = EncrDecr.Decrypt(userGroup.Password, Key);
                    string ARMAppsconnectionString = GetDBConnections(DataBase,UserName,Password,DBVersion, ConnectionName);
                    return ARMAppsconnectionString;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            }
            else
            {
                return Constants.RESULTS.NO_RECORDS.ToString();
            }

        }

        public string GetARMRedisConfiguration(string appName)
        {
            var app = _context.ARMApps.FirstOrDefault(p => p.AppName.ToLower() == appName.ToLower());
            if (app == null)
                throw new KeyNotFoundException("App not found");
            var Key = app.PrivateKey;
            if (Key != null)
            {
                try
                {
                    string ConnectionName = EncrDecr.Decrypt(app.RedisIP, Key);
                    string DataBase = EncrDecr.Decrypt(app.DataBase, Key);
                    var redisdata = new
                    {
                        connectionName = ConnectionName,
                        dataBase = DataBase,
                    };
                    return JsonConvert.SerializeObject(redisdata);
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            }
            else
            {
                return Constants.RESULTS.NO_RECORDS.ToString();
            }

        }

        public async Task<string> GetDBConfigurationBySessionId(string ARMSessionId)
        {
            var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
            string connectionstring = await GetDBConfiguration(dictSession["APPNAME"]);
            return connectionstring;
        }

        public async Task<string> GetAxpertWebURL(string appName)
        {
            string AxpertWeb_URL = "";
            try
            {
                var app = _context.ARMApps.Find(appName);
                if (app == null)
                    throw new KeyNotFoundException("App not found");
                AxpertWeb_URL = app.AxpertWebUrl;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return AxpertWeb_URL;
        }

        public async Task<string> GetAxpertWebURLBySessionId(string ARMSessionId)
        {
            string AxpertWeb_URL = "";
            try
            {
                var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
                var app = _context.ARMApps.Find(dictSession["APPNAME"]);
                if (app == null)
                    throw new KeyNotFoundException("App not found");
                AxpertWeb_URL = app.AxpertWebUrl;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return AxpertWeb_URL;
        }
        public async Task<string> AxpertWebScriptsURL(string appName)
        {
            string AxpertWebScripts_URL = "";

            string key = $"ARM_{Constants.AXPERT.AXPERT_WEB_URL.ToString()}_{appName}";
            AxpertWebScripts_URL = await _redis.StringGetAsync(key);
            if (!string.IsNullOrEmpty(AxpertWebScripts_URL))
                return AxpertWebScripts_URL;

            try
            {
                var app = _context.ARMApps.Find(appName);
                if (app == null)
                    throw new KeyNotFoundException("App not found");
                AxpertWebScripts_URL = app.AxpertScriptsUrl;
                await _redis.StringSetAsync(key, AxpertWebScripts_URL);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return AxpertWebScripts_URL;
        }

        public async Task<string> AxpertWebScriptsURLBySessionId(string ARMSessionId)
        {
            string AxpertWebScripts_URL = "";
            try
            {
                var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
                var app = _context.ARMApps.Find(dictSession["APPNAME"]);
                if (app == null)
                    throw new KeyNotFoundException("App not found");
                AxpertWebScripts_URL = app.AxpertScriptsUrl;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return AxpertWebScripts_URL;
        }

        public ARMResult GetOTPResult(string messagecode, string regId)
        {
            ARMResult result = new ARMResult();
            result.result.Add("message", messagecode);
            result.result.Add("regid", regId);
            result.result.Add("otplength", _config["OTPLength"]);
            result.result.Add("otpattemptsleft", _config["OTPMaximumattempt"]);
            return result;
        }
        public ARMResult OTPFailureResult(string messagecode, int totalAttempt)
        {
            ARMResult result = new ARMResult();
            result.result.Add("message", messagecode);
            result.result.Add("attemptsleft", totalAttempt - 1);
            return result;
        }

        public async Task<JObject> ParseRMQMessage(string message)
        {
            JObject messageData = JObject.Parse(message);
            string queueData = messageData["queuedata"].ToString();
            string messageWithoutEscape = queueData.Replace("\r\n", "").Replace("\\", "").Trim('"');
            JObject messageDatas = JsonConvert.DeserializeObject<JObject>(messageWithoutEscape);
            return messageDatas;
        }

        public string GetParametersFromSQL(string sql)
        {
            var regex = new Regex(@"@(?<Parameter>\w+)");
            var matchCollection = regex.Matches(sql);
            var parameters = matchCollection.Cast<Match>().Select(x => x.Groups["Parameter"].Value);
            return string.Join(",", parameters);
        }

        public (string[] paramNames, DbType[] paramTypes, object[] paramValues) GetSQLParams(Dictionary<string, string> sqlParams, string sql)
        {
            var paramNames = new List<string>();
            var paramTypes = new List<DbType>();
            var paramValues = new List<object>();

            var sqlParamNames = GetParametersFromSQL(sql);

            foreach (var sqlParam in sqlParams)
            {
                var paramName = sqlParam.Key.Split("~")[0];

                if (sqlParamNames.Contains(paramName))
                {
                    paramNames.Add("@" + paramName);
                    paramTypes.Add(DbType.String);
                    paramValues.Add(sqlParam.Value);
                }
            }

            return (paramNames.ToArray(), paramTypes.ToArray(), paramValues.ToArray());
        }

        public bool AreEmailAddressesValid(MimeMessage emailMessage)
        {
            var addresses = new List<MailboxAddress>();

            if (emailMessage.From != null)
            {
                addresses.AddRange(emailMessage.From.Mailboxes);
            }

            if (emailMessage.To != null)
            {
                addresses.AddRange(emailMessage.To.Mailboxes);
            }

            if (emailMessage.Cc != null)
            {
                addresses.AddRange(emailMessage.Cc.Mailboxes);
            }

            if (emailMessage.Bcc != null)
            {
                addresses.AddRange(emailMessage.Bcc.Mailboxes);
            }

            foreach (var address in addresses)
            {
                if (!IsValidEmailAddress(address.Address))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsValidEmailAddress(string emailAddress)
        {
            try
            {
                var address = new MailAddress(emailAddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public void UpdateOrAddJsonKey(ref JObject jsonObject, string key, JToken value)
        {
            bool keyExists = false;

            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    property.Value = value;
                    keyExists = true;
                    break;
                }
            }

            if (!keyExists)
            {
                jsonObject[key] = value;
            }
        }

        public void AddJsonKey(ref JObject jsonObject, string key, JToken value)
        {
            bool keyExists = false;
            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    keyExists = true;
                    break;
                }
            }

            if (!keyExists)
            {
                jsonObject[key] = value;
            }
        }

        public void RemoveJsonKey(ref JObject jsonObject, string key)
        {
            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    jsonObject.Remove(property.Name);
                    break;
                }
            }
        }



    }
}
