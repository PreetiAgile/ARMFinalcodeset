﻿//using ARMCommon.ActionFilter;
//using ARMCommon.Interface;
//using ARMCommon.Model;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using System.Collections.Concurrent;

//namespace ARM_APIs.Controllers
//{
  
//    [Route("api/v{version:apiVersion}")]
//    [ApiVersion("1")]
//    [ServiceFilter(typeof(ApiResponseFilter))]
//    [ApiController]
//    public class ARMPushNotificationController : ControllerBase
//    {
//        private readonly IHubContext<NotificationHub> _hubContext;
//        private readonly IRedisHelper _redis;
//        private readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

//        public ARMPushNotificationController(IHubContext<NotificationHub> hubContext, IRedisHelper redis)
//        {
//            _hubContext = hubContext;
//            _redis = redis;
//        }


//        [HttpPost("SendSignalR")]
//        public async Task<IActionResult> SendSignalR([FromBody] SendSignalRRequest request)
//        {
             
//            string connectionId= _redis.StringGet(request.UserId);
//            if (!string.IsNullOrEmpty(connectionId))
//            {
//                   await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", request.Message);
//                    return Ok("SUCCESS");
                 
//            }
//            else
//            {
//                return BadRequest("INAVALIDUSERID");
//            }
//        }
//    }

//} 
