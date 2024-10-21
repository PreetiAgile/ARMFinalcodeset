using ARM_APIs.Interface;
using ARMCommon.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ApiController]
    public class ARMCheckServiceController : ControllerBase
    {
        private readonly IARMCheckService _armService;

        public ARMCheckServiceController(IARMCheckService armService)
        {
            _armService = armService;
        }

        [HttpGet("ARMCheckService")]
        public async Task<ActionResult> ARMCheckService()
        {
            try
            {
                await _armService.ProcessLogsAndSendEmails();
                return Ok("Process completed.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
