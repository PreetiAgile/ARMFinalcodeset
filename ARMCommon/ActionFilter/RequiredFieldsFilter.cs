using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Reflection;

namespace ARMCommon.ActionFilter
{
    public class RequiredFieldsFilter : ActionFilterAttribute
    {
        private readonly string[] _fields;

        public RequiredFieldsFilter(params string[] fields)
        {
            _fields = fields;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var type = context.ActionArguments.First().Value.GetType();
            var missingFields = new List<string>();

            foreach (var field in _fields)
            {
                var property = type.GetProperty(field);
                if (property == null || string.IsNullOrEmpty(property.GetValue(context.ActionArguments.First().Value)?.ToString()))
                {
                    missingFields.Add(field);
                }
            }

            if (missingFields.Count > 0)
            { 
                ARMResult result = new ARMResult();
                result.result.Add("status", false);
                result.result.Add("message", "Required field(s) : '" + string.Join(", ", missingFields) + "' cannot be empty.");
                context.Result = new BadRequestObjectResult(JsonConvert.SerializeObject(result));
            }
        }
    }

    public class RequiredFieldsFilterAxpertResult : ActionFilterAttribute
    {
        private readonly string[] _fields;

        public RequiredFieldsFilterAxpertResult(params string[] fields)
        {
            _fields = fields;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var type = context.ActionArguments.First().Value.GetType();
            var missingFields = new List<string>();

            foreach (var field in _fields)
            {
                var property = type.GetProperty(field);
                if (property == null || string.IsNullOrEmpty(property.GetValue(context.ActionArguments.First().Value)?.ToString()))
                {
                    missingFields.Add(field);
                }
            }

            if (missingFields.Count > 0)
            {
                ARMResult result = new ARMResult();
                result.result.Add("status", false);
                result.result.Add("message", "Error. Required field(s) : '" + string.Join(", ", missingFields) + "' cannot be empty.");
                context.Result = new BadRequestObjectResult(JsonConvert.SerializeObject(result.result));
            }
        }
    }



}
