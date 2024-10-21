using ARMCommon.Helpers;
using ARMCommon.Model;
using ARMCommon.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;

namespace ARMCommon.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration _configuration;
        private readonly ARMLogger _armLogger;
        public ErrorHandlingMiddleware(RequestDelegate next,IConfiguration configuration, ARMLogger armLogger)
        {
            this.next = next;
            this._configuration = configuration;
            this._armLogger = armLogger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }

            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync(context);
            }

            if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                await HandleNotFoundAsync(context);
            }

            if (context.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
            {
                await HandleInternalServerErrorAsync(context);
            }
        }

        private  async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            
           await LogException(context, ex);
            ARMResult errorResponse = new ARMResult();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            errorResponse.result.Add("statuscode", context.Response.StatusCode);
            errorResponse.result.Add("message", ex.Message);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
        }

        public async Task LogException(HttpContext context, Exception ex)
        {
            Guid? instanceId = _armLogger.InstanceId;
            if (!instanceId.HasValue)
            {
                return;
            }
            ExceptionModel exceptionModel = new ExceptionModel();
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;
            MemoryStream requestBody = new MemoryStream();
            await context.Request.Body.CopyToAsync(requestBody);
            requestBody.Seek(0, SeekOrigin.Begin);
            string requesttext = await new StreamReader(requestBody).ReadToEndAsync();
            requestBody.Seek(0, SeekOrigin.Begin);

            exceptionModel.HttpMethod = context.Request.Method;
            exceptionModel.Requestbody = requesttext;
            exceptionModel.QueryStringValue = context.Request.QueryString.Value;
            exceptionModel.ExceptionMessage = ex.Message;
            exceptionModel.StackSTrace = ex.StackTrace;
            exceptionModel.InnerExceptionMessage = ex.InnerException?.Message;
            exceptionModel.InnerExceptionStackTrace = ex.InnerException?.StackTrace;
            exceptionModel.logtype = "exceptionLog";
            exceptionModel.Module = context.Request.Method.ToString();
            exceptionModel.InstanceId = instanceId;
            RabbitMQProducer Rabbitmq = new RabbitMQProducer(_configuration);
            Rabbitmq.SendMessages(JsonConvert.SerializeObject(exceptionModel), "logQueue");
        }

        private Task HandleUnauthorizedAsync(HttpContext context)
        {
            ARMResult errorResponse = new ARMResult();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            errorResponse.result.Add("statuscode", context.Response.StatusCode);
            errorResponse.result.Add("message", "Unauthorized Error");
            return context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
        }

        private  Task HandleNotFoundAsync(HttpContext context)
        {
            ARMResult errorResponse = new ARMResult();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            errorResponse.result.Add("statuscode", context.Response.StatusCode);
            errorResponse.result.Add("message", "Not Found");
            return context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
        }

        private static Task HandleInternalServerErrorAsync(HttpContext context)
        {
            ARMResult errorResponse = new ARMResult();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            errorResponse.result.Add("statuscode", context.Response.StatusCode);
            errorResponse.result.Add("message", "Internal Server Error");
            return context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
        }
    }

}
