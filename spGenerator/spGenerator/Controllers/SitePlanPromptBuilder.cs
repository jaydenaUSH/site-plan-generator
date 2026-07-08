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
            prompt.Append("Can you generate a site plan that plans how to setup a venue. The details for this venue are provided in a JSON structure pasted at the end of the message.\n");
            prompt.Append("\nPlease use the context from [VECTORDB] holding past site plans as a basis to understand the logic behind making site plans so " +
                            "you can generate a new site plan for this sepcific venue given the details.");
            prompt.Append("The response must include sections detailing the Site Overview, " +
                            "Recommmended Layout, Volunteer Flow, Supply Flow, Timeline, Risks, PM Review Checklist in the form of JSON plus a visualization as a pdf of the site plan.\n\n");
            prompt.Append(JsonConvert.SerializeObject(req));


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
            // answer.output[x].content[x].text   (Access text from api call)
           
            //Convert answer to format SQL needs if required and add and save to db
           
            return answer;


            //Save draft to sql
            _db.SaveChanges();
            //return draft
            return null;

        }


    }


}