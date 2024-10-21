
using ARMCommon.ActionFilter;
using ARMCommon.Filter;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.POIFS.FileSystem;
using NPOI.POIFS.NIO;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Twilio.TwiML.Messaging;
using Twilio.TwiML.Voice;
using static ARMCommon.Helpers.Constants;

namespace ARM_APIs.Controllers
{
    //[Authorize]
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ApiController]
    public class ARMGetDataController : ControllerBase
    {

        private readonly IRedisHelper _redis;
        private readonly IGetData _getdata;

        public ARMGetDataController(IRedisHelper redis, IGetData getData)
        {
            _redis = redis;
            _getdata = getData;
        }

        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [RequiredFieldsFilter("datasource")]
        [HttpPost("ARMGetDataRequest")]
        public async Task<IActionResult> ARMGetDataRequest(ARMGetDataRequest data)
        {

            ARMResult result = new ARMResult();
            var sendData = _getdata.SendToDataService(data);
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", sendData);
            return Ok(result);
        }

        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [RequiredFieldsFilter("datasource")]
        [HttpPost("ARMGetDataResponse")]
        public async Task<IActionResult> ARMGetDataResponse(ARMGetDataRequest data)
        {
            string key = string.Empty;
            string redisdata = string.Empty;
            if (!string.IsNullOrEmpty(data.dataId))
            {
                key = data.datasource + data.dataId.ToString();
                redisdata = await _redis.StringGetAsync(key);
            }

            ARMResult result = new ARMResult();
            if (string.IsNullOrEmpty(redisdata))
            {
                var loginuser = await _getdata.GetLoginUser(data.ARMSessionId);
                SQLResult getDataFromDB = await _getdata.GetDataSourceData(loginuser["APPNAME"], data.datasource, data.sqlParams);
                result.result.Add("message", "SUCCESS");
                result.result.Add("data", getDataFromDB.data);
                return Ok(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("data", redisdata);
                return Ok(result);
            }
        }

        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [RequiredFieldsFilter("datasource", "ARMSessionId")]
        [HttpPost("ARMGetDataFromAPI")]
        public async Task<IActionResult> ARMGetDataFromAPI(ARMGetDataRequest data)
        {
            var loginuser = await _getdata.GetLoginUser(data.ARMSessionId);
            var fetchAPIDefination = await _getdata.GetAPIDefinitionFromRedis(data.datasource, loginuser["APPNAME"]);
            if (fetchAPIDefination == null)
            {
                return BadRequest("NODATAFOUNDINAPIDEFINITION");
            }
            var apiresult = await _getdata.GetDataFromAPI(fetchAPIDefination, data.apiparams, data.RefreshQueueName);
            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", apiresult);
            return Ok(result);

        }

        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [RequiredFieldsFilter("datasource", "ARMSessionId")]
        [HttpPost("ARMGetDataFromSQL")]
        public async Task<IActionResult> ARMGetDataFromSQL(ARMGetDataRequest data)
        {
            SQLResult getDataFromRedis = new SQLResult();
            getDataFromRedis = await _getdata.GetSQLDataFromRedis(data.AppName, data.datasource, data.sqlParams);
            if (getDataFromRedis == null || getDataFromRedis.data == null || getDataFromRedis.data.Rows.Count == 0)
            {
                var sqlDataSource = await _getdata.GetSQLDataSourceFromRedis(data.datasource, data.AppName);
                if (sqlDataSource == null)
                {
                    return BadRequest("NODATAFOUNDINSQLDATASOURCE");
                }
                var sqlresult = await _getdata.GetSQLData(sqlDataSource, data.sqlParams, data.RefreshQueueName);
                if (string.IsNullOrEmpty(sqlresult.error))
                {
                    ARMResult result = new ARMResult();
                    result.result.Add("message", "SUCCESS");
                    result.result.Add("data", sqlresult.data);
                    return Ok(result);
                }
                else
                {
                    ARMResult result = new ARMResult();
                    result.result.Add("message", sqlresult.error);
                    result.result.Add("messagetype", "Custom");
                    return BadRequest(result);
                }
            }
            else
            {
                ARMResult result = new ARMResult();
                result.result.Add("message", "SUCCESS");
                result.result.Add("data", getDataFromRedis.data);
                return Ok(result);
            }
        }

        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [RequiredFieldsFilter("ARMSessionId")]
        [HttpPost("ARMClearData")]
        public async Task<IActionResult> ARMClearData(ARMGetDataRequest data)
        {
            var loginuser = await _getdata.GetLoginUser(data.ARMSessionId);
            var cleardata = _getdata.ClearCacheData(loginuser["USERNAME"]);
            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", cleardata);
            return Ok(result);
        }

        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [RequiredFieldsFilter("ARMSessionId")]
        [HttpPost("ARMRefreshData")]
        public async Task<IActionResult> ARMRefreshData(ARMGetDataRequest data)
        {
            var cleardata = _getdata.RefreshCacheData(data.ARMSessionId);
            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", cleardata);
            return Ok(result);
        }

        [ServiceFilter(typeof(ValidateSessionFilter))]
        [RequiredFieldsFilter("datasource", "ARMSessionId")]
        [HttpPost("ARMGetDatafromAxpert")]
        public async Task<IActionResult> ARMGetDatafromAxpert(ARMGetDataRequest data)
        {
            ARMResult result = new ARMResult();
            {
                var loginuser = await _getdata.GetLoginUser(data.ARMSessionId);
                var getDataFromAxpert = await _getdata.GetDataSourceData(loginuser["APPNAME"], data.datasource, data.sqlParams);
                result.result.Add("message", "SUCCESS");
                result.result.Add("data", getDataFromAxpert);
                return Ok(result);

            }

        }

        [AllowAnonymous]
        [RequiredFieldsFilter("datasource")]
        [HttpPost("ARMGetAxpertDropDownDataFromSQL")]
        public async Task<IActionResult> ARMGetAxpertDropDownDataFromSQL(ARMGetDataRequest data)
        {
            SQLResult getDataFromRedis = new SQLResult();
            string userName = data.UserName ?? "";
            string appName = data.AppName ?? ""; ;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(appName))
            {
                var loginuser = await _getdata.GetLoginUser(data.ARMSessionId);
                userName = loginuser["USERNAME"];
                appName = loginuser["APPNAME"];
            }

            string cacheKey = _getdata.GenerateKeyFromSqlParams(appName, data.datasource, data.sqlParams);

            string cacheData = await _redis.StringGetAsync(cacheKey);

            if (string.IsNullOrEmpty(cacheData))
            {
                var sqlDataSource = await _getdata.GetSQLDataSourceFromRedis(data.datasource, appName);
                if (sqlDataSource == null)
                {
                    return BadRequest("NODATAFOUNDINSQLDATASOURCE");
                }
                var sqlresult = await _getdata.GetDropDownSQLData(sqlDataSource, data.sqlParams, data.RefreshQueueName);
                if (string.IsNullOrEmpty(sqlresult.error))
                {
                    List<Dictionary<string, object>> pickdataList = new List<Dictionary<string, object>>();
                    pickdataList.Add(new Dictionary<string, object> { { "rcount", sqlresult.data.Rows.Count.ToString() } });
                    pickdataList.Add(new Dictionary<string, object> { { "fname", sqlresult.data.Columns[0].ColumnName } });

                    var columnNames = sqlresult.data.Columns.Cast<DataColumn>()
                       .Skip(1)
                       .Select(column => column.ColumnName);

                    string concatenatedColumnNames = string.Join("^", columnNames);
                    pickdataList.Add(new Dictionary<string, object> { { "dfname", concatenatedColumnNames } });

                    sqlresult.data.Columns.Add("d");

                    DataTable dt = new DataTable();
                    dt.Columns.Add("i", typeof(string));

                    if (sqlresult.data.Columns.Count > 1)
                    {
                        dt.Columns.Add("d", typeof(string));

                        for (int rowIndex = 0; rowIndex < sqlresult.data.Rows.Count; rowIndex++)
                        {
                            string depVals = GetConcatenatedValues(sqlresult.data.Rows[rowIndex], 1, sqlresult.data.Columns.Count - 1);

                            DataRow newRow = dt.NewRow();
                            newRow["i"] = sqlresult.data.Rows[rowIndex][0];
                            newRow["d"] = depVals;
                            dt.Rows.Add(newRow);
                        }
                    }
                    else
                    {
                        for (int rowIndex = 0; rowIndex < sqlresult.data.Rows.Count; rowIndex++)
                        {
                            DataRow newRow = dt.NewRow();
                            newRow["i"] = sqlresult.data.Rows[rowIndex][0];
                            dt.Rows.Add(newRow);
                        }
                    }

                    pickdataList.Add(new Dictionary<string, object> { { "data", dt } });

                    int expirytime = sqlDataSource.expiry ?? 0;
                    var dataResult = new {
                        result = pickdataList
                    };
                    var result = JsonConvert.SerializeObject(dataResult);
                    await _redis.StringSetAsync(cacheKey, result, expirytime);
                    return Ok(result);
                }
                else
                {
                    ARMResult result = new ARMResult();
                    result.result.Add("message", sqlresult.error);
                    result.result.Add("messagetype", "Custom");
                    return BadRequest(result);
                }
            }
            else
            {
                return Ok(cacheData);
            }
        }


        [AllowAnonymous]
        [HttpGet("ARMGetAxpertDropDownDataFromSQL")]
        public async Task<IActionResult> ARMGetAxpertDropDownDataFromSQL()
        {
            ARMGetDataRequest data = new ARMGetDataRequest(); ;
            data.datasource = HttpContext.Request.Query["datasource"].ToString();
            data.AppName = HttpContext.Request.Query["appname"].ToString();
            data.UserName = HttpContext.Request.Query["userName"].ToString();
            string strParams = HttpContext.Request.Query["sqlparams"].ToString();

            string[] paramPairs = strParams.Split('|');
            Dictionary<string, string> sqlParams = new Dictionary<string, string>();

            foreach (string pair in paramPairs)
            {
                string[] keyValue = pair.Split('~');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0];
                    string value = keyValue[1];
                    sqlParams[key] = value;
                }
            }
            data.sqlParams = sqlParams;
            string userName = data.UserName ?? "";
            string appName = data.AppName ?? ""; ;
            string cacheKey = _getdata.GenerateKeyFromSqlParams(appName, data.datasource, data.sqlParams);

            string cacheData = await _redis.StringGetAsync(cacheKey);

            if (string.IsNullOrEmpty(cacheData))
            {
                var sqlDataSource = await _getdata.GetSQLDataSourceFromRedis(data.datasource, appName);
                if (sqlDataSource == null)
                {
                    return BadRequest("NODATAFOUNDINSQLDATASOURCE");
                }
                var sqlresult = await _getdata.GetDropDownSQLData(sqlDataSource, data.sqlParams, data.RefreshQueueName);
                if (string.IsNullOrEmpty(sqlresult.error))
                {
                    List<Dictionary<string, object>> pickdataList = new List<Dictionary<string, object>>();
                    //pickdataList.Add(new Dictionary<string, object> { { "rcount", sqlresult.data.Rows.Count.ToString() } });
                    pickdataList.Add(new Dictionary<string, object> { { "fname", sqlresult.data.Columns[0].ColumnName.ToLower() } });

                    var columnNames = sqlresult.data.Columns.Cast<DataColumn>()
                       .Skip(1)
                       .Select(column => column.ColumnName);

                    string concatenatedColumnNames = string.Join("^", columnNames);
                    pickdataList.Add(new Dictionary<string, object> { { "dfname", concatenatedColumnNames.ToLower() } });

                    sqlresult.data.Columns.Add("d");

                    DataTable dt = new DataTable();
                    dt.Columns.Add("i", typeof(string));

                    if (sqlresult.data.Columns.Count > 1)
                    {
                        dt.Columns.Add("d", typeof(string));

                        for (int rowIndex = 0; rowIndex < sqlresult.data.Rows.Count; rowIndex++)
                        {
                            string depVals = GetConcatenatedValues(sqlresult.data.Rows[rowIndex], 1, sqlresult.data.Columns.Count - 1);

                            DataRow newRow = dt.NewRow();
                            newRow["i"] = sqlresult.data.Rows[rowIndex][0];
                            newRow["d"] = depVals;
                            dt.Rows.Add(newRow);
                        }
                    }
                    else
                    {
                        for (int rowIndex = 0; rowIndex < sqlresult.data.Rows.Count; rowIndex++)
                        {
                            DataRow newRow = dt.NewRow();
                            newRow["i"] = sqlresult.data.Rows[rowIndex][0];
                            dt.Rows.Add(newRow);
                        }
                    }

                    pickdataList.Add(new Dictionary<string, object> { { "data", dt } });

                    int expirytime = sqlDataSource.expiry ?? 0;
                    var dataResult = new
                    {
                        result = pickdataList
                    };
                    var result = JsonConvert.SerializeObject(dataResult);
                    await _redis.StringSetAsync(cacheKey, result, expirytime);
                    return Ok(result);
                }
                else
                {
                    ARMResult result = new ARMResult();
                    result.result.Add("message", sqlresult.error);
                    result.result.Add("messagetype", "Custom");
                    return BadRequest(result);
                }
            }
            else
            {
                return Ok(cacheData);
            }
        }
        static string GetConcatenatedValues(DataRow row, int startColumnIndex, int endColumnIndex)
        {
            string result = "";

            // Iterate through the columns and concatenate the values
            for (int i = startColumnIndex; i <= endColumnIndex; i++)
            {
                if (row.Table.Columns[i] != null)
                {
                    if (i > startColumnIndex)
                    {
                        // Add delimiter (^) if it's not the first value
                        result += "^";
                    }

                    // Add the column value to the result
                    result += row[i].ToString();
                }
            }

            return result;
        }


    }
}
