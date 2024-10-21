
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI_API;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ApiController]
    public class AxpertAIController : ControllerBase
    { 
        private readonly IConfiguration _config;
        public AxpertAIController(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost]
        [Route("ARMAIGetFieldList")]
        public async Task<IActionResult> ARMAIGetFieldList([FromBody] string formtype)
        {
            if (string.IsNullOrEmpty(formtype))
            {
                return BadRequest("formtype is null or empty");
            }

            string prompt = _config["ARMAXPERTAI"]?.Replace("$formtype$", formtype);
            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("ARMAXPERTAI is null or empty");
            }

            //your OpenAI API key
            string apiKey = _config["OPENAIAPIKEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("OPENAIAPIKEY is null or empty");
            }

            string answer = string.Empty;
            var openai = new OpenAIAPI(apiKey);
            CompletionRequest completion = new CompletionRequest();
            completion.Prompt = prompt;
            completion.Model = OpenAI_API.Model.DavinciText;
            completion.MaxTokens = 4000;

            var result = await openai.Completions.CreateCompletionAsync(completion);
            if (result != null && result.Completions != null)
            {
                foreach (var item in result.Completions)
                {
                    answer = item.Text;
                }

                // Deserialize the JSON string into a list of objects
                var obj = JsonConvert.DeserializeObject<List<FieldsData>>(answer);

                // Serialize the object into an indented JSON string
                var json = JsonConvert.SerializeObject(obj, Formatting.Indented);

                // Return the indented JSON string as the response
                return Ok(json);
            }
            else
            {
                return BadRequest("Not found");
            }
        }



        //[HttpPost]
        //[Route("AxpertAI")]
        //public IActionResult GetResult([FromBody] List<FieldsData> fieldsDataList)
        //{
        //    // your OpenAI API key
        //    string apiKey = "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";
        //    string answer = string.Empty;
        //    var openai = new OpenAIAPI(apiKey);
        //    CompletionRequest completion = new CompletionRequest();

        //    // construct the prompt string from the fields data list
        //    StringBuilder promptBuilder = new StringBuilder();
        //    foreach (var fieldsData in fieldsDataList)
        //    {
        //        promptBuilder.Append(fieldsData.fieldName + ": [" + fieldsData.fieldType + "]");
        //        promptBuilder.Append(Environment.NewLine);
        //    }
        //    completion.Prompt = promptBuilder.ToString();

        //    completion.Model = OpenAI_API.Model.DavinciText;
        //    completion.MaxTokens = 4000;
        //    var result = openai.Completions.CreateCompletionAsync(completion);
        //    if (result != null)
        //    {
        //        foreach (var item in result.Result.Completions)
        //        {
        //            answer = item.Text;
        //        }
        //        return Ok(answer);
        //    }
        //    else
        //    {
        //        return BadRequest("Not found");
        //    }
        //}


    }
    public class FieldsData
    {
        public string fieldId { get; set; }
        public string fieldName { get; set; }
        public string fieldType { get; set; }
        public int fieldLength { get; set; }
    }

}




