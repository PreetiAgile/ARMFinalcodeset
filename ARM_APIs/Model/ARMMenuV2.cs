using ARM_APIs.Interface;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;

namespace ARM_APIs.Model
{
    public class ARMMenuV2 : IARMenuV2
    {
        private readonly DataContext _context;
        private readonly IRedisHelper _redis;
        private readonly IConfiguration _config;
        private readonly IPostgresHelper _postGres;
        private readonly Utils _common;

        public ARMMenuV2(DataContext context, IRedisHelper redis, IConfiguration configuration, IPostgresHelper postGres, Utils common)
        {
            _context = context;
            _redis = redis;
            _config = configuration;
            _postGres = postGres;
            _common = common;
        }

    
        private async Task<Dictionary<string, string>> GetLoginUser(string ARMSessionId)
        {
            var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
            return dictSession;
        }

        public async Task<DataTable> GetHomePage(string sessionId, string userName)
        {
            var loginuser = await GetLoginUser(sessionId);
            if (string.IsNullOrEmpty(userName))
            {
                userName = loginuser["USERNAME"].ToString();
            }
            Dictionary<string, string> config = await _common.GetDBConfigurations(loginuser["APPNAME"]);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.HOMEPAGECARDSV2.ToString();
            string[] paramName = { "@username" };
            DbType[] paramType = { DbType.String };
            object[] paramValue = { userName };

            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                sql = Constants_SQL.HOMEPAGECARDSV2.ToString();
            }

            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            var table = await dbHelper.ExecuteQueryAsync(sql, connectionString, paramName, paramType, paramValue);
            return table;
        }

        public async Task<DataTable> GETMENUFORDEFAULTROLE_V2(string sessionId)
        {
            ParamsDetails parameters = new ParamsDetails();
            parameters.ParamsNames = new List<ConnectionParamsList>();

            var loginuser = await GetLoginUser(sessionId);
            Dictionary<string, string> config = await _common.GetDBConfigurations(loginuser["APPNAME"]);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.GETMENUFORDEFAULTROLE_V2.ToString();
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, new string[] { }, new DbType[] { }, new object[] { });
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                sql = Constants_SQL.GETMENUFORDEFAULTROLE_V2.ToString();
            }
            var table = await dbHelper.ExecuteQueryAsync(sql, connectionString, new string[] { }, new DbType[] { }, new object[] { });
            return table;
        }

        public async Task<DataTable> GETMENUFOROTHERROLE_V2(string sessionId, string allRole)
        {
            ParamsDetails parameters = new ParamsDetails();
            parameters.ParamsNames = new List<ConnectionParamsList>();
            var loginuser = await GetLoginUser(sessionId);
            Dictionary<string, string> config = await _common.GetDBConfigurations(loginuser["APPNAME"]);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.GETMENUFOROTHERROLE_V2.ToString().Replace("$allRole$", allRole);
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, new string[] { }, new DbType[] { }, new object[] { });
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                sql = Constants_SQL.GETMENUFOROTHERROLE_V2.ToString().Replace("$allRole$", allRole);
            }

            var table = await dbHelper.ExecuteQueryAsync(sql, connectionString, new string[] { }, new DbType[] { }, new object[] { });
            return table;
        }

        public async Task<List<string>> GetallRole(string sessionId)
        {
            string sessionValues = await _redis.HashGetAsync(sessionId, Constants.SESSION_DATA.USER_ROLES.ToString());
            List<string> userroles = sessionValues.Split(',').ToList();
            return userroles;
        }

        public async Task<ARMResult> GetHomePageCards(ARMProcessFlowTask process)
        {
            List<string> userroles = await GetallRole(process.ARMSessionId);

            if (userroles.Count == 0)
            {
                throw new Exception("NORECORDINAXPAGES");
            }

            string allRole = string.Join(", ", userroles.Select(role => $"'{role}'"));

            DataTable MenuList;
            if (userroles.Contains("default"))
            {
                MenuList = await GETMENUFORDEFAULTROLE_V2(process.ARMSessionId);
            }
            else
            {
                MenuList = await GETMENUFOROTHERROLE_V2(process.ARMSessionId, allRole);
            }

            var Homepagecardslist = await GetHomePage(process.ARMSessionId, process.ToUser);
            var axPagesView = new DataView(MenuList);
            var homePageCardsView = new DataView(Homepagecardslist);

            var axPagesTable = axPagesView.ToTable();
            var homePageCardsTable = homePageCardsView.ToTable();

            var homepagedata = from config in homePageCardsTable.AsEnumerable()
                               join page in axPagesTable.AsEnumerable()
                               on config["stransid"] equals page["pagetype"]
                               where config["stransid"].ToString() == page["pagetype"].ToString()
                                     && page["visible"].ToString() == "T"
                               select new
                           {
                               caption = config["caption"],
                               carddesc = config["carddesc"],
                               cardid = config["cardid"],
                               pagecaption = config["pagecaption"],
                               displayicon = config["displayicon"],
                               stransid = config["stransid"],
                               datasource = config["datasource"],
                               moreoption = config["moreoption"],
                               colorcode = config["colorcode"],
                               groupfolder = config["groupfolder"],
                               grouppageid = config["grppageid"],
                               cardhide = config["cardhide"],
                               

                           };
            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", homepagedata);
            return result;
        }

        public async Task<ARMResult> GetMenu(ARMSession model)
        {
            DataTable axpages = new DataTable();
            List<string> userroles = await GetallRole(model.ARMSessionId);

            if (userroles.Count == 0)
            {
                throw new Exception("NORECORDINAXPAGES");
            }

            string allRole = string.Join(", ", userroles.Select(role => $"'{role}'"));

            if (userroles.Contains(Constants.Roles.DEFAULT.ToString().ToLower()))
            {
                axpages = await GETMENUFORDEFAULTROLE_V2(model.ARMSessionId);
            }
            else
            {
                axpages = await GETMENUFOROTHERROLE_V2(model.ARMSessionId, allRole);
            }

            if (axpages.Rows.Count > 0)
            {
                DataColumn dcolColumn = new DataColumn("url", typeof(string));
                axpages.Columns.Add(dcolColumn);

                foreach (DataRow row in axpages.Rows)
                {
                    string pageType = row["pagetype"]?.ToString();
                    if (!string.IsNullOrEmpty(pageType) && pageType.Length >= 1)
                    {
                        string pagePrefix = pageType.Substring(0, 1);
                        string webPagePrefix = "h" + row["name"]?.ToString().Substring(2);

                        if (pagePrefix == "i" || pagePrefix == "t")
                        {
                            row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={pageType}";
                        }
                        else if (pagePrefix == "w")
                        {
                            row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={webPagePrefix}";
                        }
                        else
                        {
                            row["url"] = "";
                        }
                    }
                }
            }

            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", axpages);
            return result;
        }



        public async Task<ARMResult> ARMConnectionTestDetails()
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = await System.IO.File.ReadAllTextAsync(appSettingsPath);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            string host = configuration.GetConnectionString("WebApiDatabase");
            string Redisport = configuration["AppConfig:RedisHost"];
            string Redispass = configuration["AppConfig:RedisPassword"];
            string Rabbitmq = configuration["AppConfig:RMQIP"];
            string axpertredis = configuration["AppConfig:AxpertRedisHost"];
            string axpertpass = configuration["AppConfig:AxpertRedisHost"];

            var responseObject = new
            {
                WebApiDatabase = host,
                RedisHost = Redisport,
                RedisPassword = Redispass,
                RMQIP = Rabbitmq,
                AxpertRedisHost = axpertredis,
                AxpertRedisPassword = axpertpass
            };


            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", responseObject);
            return result;

        }
    }
}
