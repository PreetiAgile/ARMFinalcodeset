using ARM_APIs.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

[Route("api/v{version:apiVersion}")]
[ApiVersion("1")]
[ApiController]
public class FcmTokenController : Controller
{
    private readonly IFirebase _firebaseTokenService;

    public FcmTokenController(IFirebase firebaseTokenService)
    {
        _firebaseTokenService = firebaseTokenService;
    }



    [HttpGet("GetFCMAccessToken")]
    public async Task<IActionResult> GetFCMAccessToken()
    {
        var accessToken = await _firebaseTokenService.GetAccessTokenAsync();
        return Ok(accessToken);
    }
    [HttpPost("SendFCMNotification")]
    public async Task<IActionResult> SendFCMNotification(FCMRequest fcmRequest)
    {
        try
        {
            var accessToken = await _firebaseTokenService.GetAccessTokenAsync();

            if (fcmRequest.message?.token == null)
            {
                return BadRequest(new { message = "Token is required." });
            }


            foreach (var token in fcmRequest.message.token)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                    var jsonRequest = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        message = new
                        {
                            token,
                            data = fcmRequest.message.data
                        }
                    });

                    var fcmUrl = "https://fcm.googleapis.com/v1/projects/axperthybrid/messages:send";
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(fcmUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new { message = "Notification sent successfully", data = responseContent });
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return Unauthorized(new { message = "Invalid token" });
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, new { message = "Error sending notification", error = responseContent });
                    }

                }
            }

            return Ok();

        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                Error = ex.Message
            });
        }
    }



}
public class FCMRequest
{
    public FCMMessage? message { get; set; }
}

public class FCMMessage
{
    public List<string>? token { get; set; }

    public Dictionary<string, string>? data { get; set; }
}



