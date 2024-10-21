//using ARM_APIs.Model;
//using ARMCommon.ActionFilter;
//using ARMCommon.Helpers;
//using ARMCommon.Interface;
//using ARMCommon.Model;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace ARM_APIs.Controllers
//{
//    [Route("api/v{version:apiVersion}")]
//    [ApiVersion("1")]
//    [ApiController]
//    public class ARMPushToQueueController : Controller
//    {
//        private readonly IConfiguration _config;
//        private readonly IRabbitMQProducer _iMessageProducer;
//        public ARMPushToQueueController(IConfiguration configuration, IRabbitMQProducer iMessageProducer)
//        {
//            _config = configuration;
//            _iMessageProducer = iMessageProducer;
//        }

//        [RequiredFieldsFilter("queuename")]
//        [ServiceFilter(typeof(ApiResponseFilter))]
//        [HttpPost("ARMPushToQueue")]
//        public IActionResult ARMPushToQueue(ARMQueueData queuedata)
//        {            
//            if (_iMessageProducer.SendMessages(JsonConvert.SerializeObject(queuedata), queuedata.queuename, Convert.ToBoolean(queuedata.trace), Convert.ToInt32(queuedata.timespandelay)))
//            {
//                return Ok("QUEUEDATAINSERTEDSUCESSFULLY");
//            }
//            else
//            {
//                return BadRequest("QUEUEDATAINSERTIONFAILED");
//            }

//        }

//        [RequiredFieldsFilter("queuedata", "queuename")]
//        [ServiceFilter(typeof(ApiResponseFilter))]
//        [HttpPost("ARMPushJobsToQueue")]
//        public IActionResult ARMPushJobsToQueue(ARMQueueData queuedata)
//        {
//            queuedata.queuename = queuedata.queuename;
//            JObject jsonObject = JObject.Parse(queuedata.queuedata);

//            // Add the new node "fromapi" with the value "true"
//            jsonObject["fromapi"] = true;
//            queuedata.queuedata = jsonObject.ToString();

//            if (queuedata.trace == null)
//            {
//                queuedata.trace = false;
//            }

//            if (queuedata.timespandelay == null)
//            {
//                queuedata.timespandelay = 0;
//            }
//            if (string.IsNullOrEmpty(queuedata.signalrclient))
//            {
//                queuedata.signalrclient = "";
//            }
//            if (_iMessageProducer.SendMessages(JsonConvert.SerializeObject(queuedata), queuedata.queuename, Convert.ToBoolean(queuedata.trace), Convert.ToInt32(queuedata.timespandelay)))
//            {
//                return Ok("QUEUEDATAINSERTEDSUCESSFULLY");
//            }
//            else
//            {
//                return BadRequest("QUEUEDATAINSERTIONFAILED");
//            }

//        }

//    }
//}
