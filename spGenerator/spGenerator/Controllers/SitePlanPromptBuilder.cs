using spGenerator;
using System.Security.Policy;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
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
            prompt.Append("Can you generate a site plan that plans how to setup a venue. The details for this venue are provided in a JSON structure.\n");
            prompt.Append(JsonConvert.SerializeObject(req));
            prompt.Append("\nPlease use the context from [VECTORDB] with past site plans to generate your new site plan for these details following similar logic as the ones provided from context.");
            prompt.Append("The response must include sections detailing the site overivew, " +
                "recommmended layout, volunteer flow, supply flow, timeline, risks, PM Review checklsit.");

            return prompt.ToString();
        }

        public void askAI(SitePlanRequest req)
        {
            var instructions = GeneratePrompt(req);

            //Insert code for AI API
            var response = new SitePlanRequest();// 

            //Save draft to sql
            _db.SitePlanRequests.Add(response);
            _db.SaveChanges();
            //return draft

        }


    }


}