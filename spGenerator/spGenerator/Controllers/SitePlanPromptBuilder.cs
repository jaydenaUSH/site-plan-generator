using spGenerator;
using System.Security.Policy;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Responses;
namespace spGenerator.Controllers{
    [RoutePrefix("api/prompt")]

    public class SitePlanPromptBuilderController: ApiController
    {
        private readonly SitePlanAIPOCEntities _db;
        public SitePlanPromptBuilderController()
        {
            _db = new SitePlanAIPOCEntities();
        }
        //Generate Prompt
        [HttpPost]
        [Route("create")]
        public string GeneratePrompt(SitePlanRequest req)
        {
            var prompt = new StringBuilder();
            prompt.Append("Can you generate a site plan that plans how to setup a venue. The details for this venue are provided in a JSON structure.\n\n");
            prompt.Append(JsonConvert.SerializeObject(req));
            prompt.Append("\n\nPlease use the context from [VECTORDB] holding past site plans as a basis to understand the logic behind making site plans so " +
                            "you can generate a new site plan for this sepcific venue given the details.");
            prompt.Append("The response must include sections detailing the Site Overview, " +
                            "Recommmended Layout, Volunteer Flow, Supply Flow, Timeline, Risks, PM Review Checklist in the form of JSON.");

            return prompt.ToString();
        }
        [HttpPost]
        [Route("askAI")]
        public async  Task<System.ClientModel.ClientResult> askAI(SitePlanRequest req)
        {
            var instructions = GeneratePrompt(req);

            //Code to make AI API instance and write request
            var client = new OpenAIClient(
                        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    );

            #pragma warning disable OPENAI001
            var responseClient = client.GetResponsesClient();

            var answer = await responseClient.CreateResponseAsync(model: "gpt-5.5", userInputText: instructions);
            //Whatever the api outputs, save as response
            //return openaiRes;

            return answer;


            //Save draft to sql
            _db.SaveChanges();
            //return draft
            return null;

        }


    }


}