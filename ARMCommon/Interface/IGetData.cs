using ARMCommon.Model;
using System.Data;

namespace ARMCommon.Interface
{
    public interface IGetData
    {
        abstract Task<string> GetDataResponseFromRedis(ARMGetDataRequest data);
        abstract Task<bool> SaveResultToRedis(string datasource, string dataId, string data);
        abstract Task<SQLResult> GetDataSourceData(string appName, string datasource, Dictionary<string, string> sqlParams = null);
        abstract Task<Dictionary<string, string>> GetLoginUser(string ARMSessionId);
        abstract Task<ARMResult> GetDataFromAPI(APIDefinitions apiDefinitions, Dictionary<string, string> apiParams, string RefreshQueueNamebool);
        abstract Task<SQLResult> GetSQLData(SQLDataSource dataSource, Dictionary<string, string> sqlParams, string RefreshQueueNamebool, bool expiry = true);
        abstract Task<SQLResult> GetDropDownSQLData(SQLDataSource dataSource, Dictionary<string, string> sqlParams, string RefreshQueueNamebool, bool expiry = true);
        abstract Task<SQLResult> GetSQLDataFromRedis(string appname, string datasource, Dictionary<string, string> sqlParams);
        abstract Task<SQLDataSource> GetSQLDataSourceFromRedis(string datasource, string appname);
        abstract Task<APIDefinitions> GetAPIDefinitionFromRedis(string datasource, string appname);
        abstract Task<bool> ClearCacheData(string keys);
        abstract Task<bool> RefreshCacheData(string sessionId);
        abstract Task<bool> SendToDataService(ARMGetDataRequest data);
        abstract string GenerateKeyFromSqlParams(string appname, string datasource, Dictionary<string, string> sqlParams);
    }
}
