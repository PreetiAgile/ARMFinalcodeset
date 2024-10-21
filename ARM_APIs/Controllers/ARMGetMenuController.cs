//using ARM_APIs.Interface;
//using ARM_APIs.Model;
//using ARMCommon.ActionFilter;
//using ARMCommon.Filter;
//using ARMCommon.Helpers;
//using ARMCommon.Model;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using NPOI.SS.Formula.Functions;
//using StackExchange.Redis;
//using System.Data;
//using System.Diagnostics;

//namespace ARM_APIs.Controllers
//{

//    [Authorize]
//    [Route("api/v{version:apiVersion}")]
//    [ApiVersion("1")]
//    [ServiceFilter(typeof(ValidateSessionFilter))]
//    [ServiceFilter(typeof(ApiResponseFilter))]
//    [ApiController]

//    public class ARMGetMenuController : Controller
//    {

//        private readonly IARMMenu _menu;

//        public ARMGetMenuController(IARMMenu menu)
//        {
//            _menu = menu;
//        }


//        [HttpPost("ARMGetMenu")]
//        public async Task<IActionResult> ARMGetMenu(ARMSession model)
//        {
//            //ARMResult result = new ARMResult();
//            SQLResult axpages = new SQLResult();
//            List<string> userroles = await _menu.GetallRole(model.ARMSessionId);
//            if (userroles.Count < 0)
//            {
//                return BadRequest("NORECORDINAXPAGES");
//            }
//            string allRole = String.Empty;
//            foreach (string role in userroles)
//            {
//                allRole += "'" + role + "', ";
//            }
//            allRole = allRole.Remove(allRole.Length - 2);
//            string page = "";
//            string props = "";
//            if (userroles.Contains(Constants.Roles.DEFAULT.ToString().ToLower()))
//            {
//                //string sql = Constants_SQL.ARMGETMENUQUERY.ToString();
//                //axpages = await _postGres.ExecuteSelectSql(sql, connectionString, parameters);
//                axpages = await _menu.GetMenuForDefaultRole(model.ARMSessionId);

//                if (string.IsNullOrEmpty(axpages.error))
//                {
//                    DataColumn dcolColumn = new DataColumn("url", typeof(string));
//                    axpages.data.Columns.Add(dcolColumn);

//                    foreach (DataRow row in axpages.data.Rows)
//                    {
//                        string menupath = row["menupath"].ToString();
//                        string rootnode = GetRootNode(menupath);
//                        // Rename the column as 'rootnode'
//                        row["menupath"] = rootnode;
//                        page = row["pagetype"].ToString();
//                        if (page == "web")
//                        {
//                            props = row["props"].ToString();
//                        }
//                        else
//                        {
//                            props = "";
//                        }

//                        {
//                            string pagePrefix = row["pagetype"].ToString().Substring(0, 1);
//                            string webPagePrefix = "h" + row["name"].ToString().Substring(2);

//                            if (pagePrefix == "i")
//                            {
//                                row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={page}";
//                            }
//                            else if (pagePrefix == "t")
//                            {
//                                row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={page}";
//                            }
//                            else if (pagePrefix == "w")
//                            {
//                                row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={webPagePrefix}";
//                            }
//                            else
//                            {
//                                row["url"] = "";
//                            }
//                        }
//                    }

//                    //string jsonString = string.Empty;
//                    axpages.data.Columns["menupath"].ColumnName = "rootnode";
//                    //jsonString = axpages.ToString();
//                    ARMResult result = new ARMResult();
//                    result.result.Add("message", "SUCCESS");
//                    result.result.Add("pages", axpages.data);
//                    return Ok(result);
//                }
//                else
//                {
//                    ARMResult result = new ARMResult();
//                    result.result.Add("message", axpages.error);
//                    result.result.Add("messagetype", "Custom");
//                    return BadRequest(result);
//                }
//            }
//            else
//            {
//                axpages = await _menu.GetnamesForOtherRoles(model.ARMSessionId, allRole);
//                if (axpages.data.Rows.Count > 0)
//                {
//                    string jsonString = string.Empty;
//                    jsonString = JsonConvert.SerializeObject(axpages.data, Formatting.Indented);

//                    JArray userNameArray = JArray.Parse(jsonString);
//                    var axPagesNames = userNameArray.Select(c => c["name"]).ToList();
//                    string names = String.Empty;

//                    foreach (string sName in axPagesNames)
//                    {
//                        names += "'" + sName + "', ";
//                    }

//                    names = names.Remove(names.Length - 2);

//                    axpages = await _menu.GetMenuForOtherRole(model.ARMSessionId, names);
//                    if (axpages.data.Rows.Count > 0)
//                    {
//                        DataColumn dcolColumn = new DataColumn("url", typeof(string));
//                        axpages.data.Columns.Add(dcolColumn);

//                        foreach (DataRow row in axpages.data.Rows)
//                        {
//                            string menupath = row["menupath"].ToString();
//                            string rootnode = GetRootNode(menupath);
//                            // Rename the column as 'rootnode'
//                            row["menupath"] = rootnode;
//                            page = row["pagetype"].ToString();
//                            if (page == "web")
//                            {
//                                props = row["props"].ToString();
//                            }
//                            else
//                            {
//                                props = "";
//                            }
//                            {
//                                string pagePrefix = row["pagetype"].ToString().Substring(0, 1);
//                                string webPagePrefix = "h" + row["name"].ToString().Substring(2);

//                                if (pagePrefix == "i")
//                                {
//                                    row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={page}";
//                                }
//                                else if (pagePrefix == "t")
//                                {
//                                    row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={page}";
//                                }
//                                else if (pagePrefix == "w")
//                                {
//                                    row["url"] = $"aspx/AxMain.aspx?authKey=AXPERT-{model.ARMSessionId}&pname={webPagePrefix}";
//                                }
//                                else
//                                {
//                                    row["url"] = "";
//                                }

//                            }
//                        }


//                        axpages.data.Columns["menupath"].ColumnName = "rootnode";
//                        ARMResult result = new ARMResult();
//                        result.result.Add("message", "SUCCESS");
//                        result.result.Add("pages", axpages.data);
//                        return Ok(result);
//                    }

//                    else
//                    {

//                        return BadRequest("NORECORD");
//                    }
//                }
//                else
//                {

//                    return BadRequest("NORECORD");
//                }
//            }


//        }

//        private string GetRootNode(string menupath)
//        {
//            int startIndex = menupath.IndexOf("\\");

//            if (startIndex != -1)
//            {
//                int endIndex = menupath.IndexOf("\\", startIndex + 1);

//                if (endIndex != -1)
//                {
//                    return menupath.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
//                }
//            }
//            else
//            {
//                int lastIndex = menupath.LastIndexOf("\\");

//                if (lastIndex != -1)
//                {
//                    return menupath.Substring(1, lastIndex - 1).Trim();
//                }
//            }

//            return string.Empty;
//        }

//        [RequiredFieldsFilter("ARMSessionId")]
//        [HttpPost("ARMGetHomePageCards")]
//        public async Task<IActionResult> ARMGetHomePageCards(ARMProcessFlowTask process)
//        {
//            ARMResult result = new ARMResult();
//            var homepagedata = await _menu.GetHomePage(process.ARMSessionId, process.ToUser);
//            if (string.IsNullOrEmpty(homepagedata.error))
//            {
//                result.result.Add("message", "SUCCESS");
//                result.result.Add("data", homepagedata.data);
//                 return Ok(result);
//            }
//            else
//            {
//                result.result.Add("message", homepagedata.error);
//                result.result.Add("messagetype", "Custom");
//                return BadRequest(result);
//            }
            
//        }

//    }

//}

