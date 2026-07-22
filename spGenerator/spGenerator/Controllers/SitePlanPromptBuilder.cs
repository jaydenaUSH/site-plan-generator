using spGenerator;
using System.Security.Policy;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Responses;
using System.Runtime.InteropServices;
using OpenAI.Images;
using System.Web.Script.Services;
using System.IO;
using Newtonsoft.Json.Linq;
using System.CodeDom.Compiler;
namespace spGenerator.Controllers{
    [RoutePrefix("api/prompt")]

    public class SitePlanPromptBuilderController: ApiController
    {
        private readonly SitePlanPromptServices _services;
        public SitePlanPromptBuilderController()
        {
            _services = new SitePlanPromptServices();
        }       
       

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> askAI(SitePlanRequest req)
        {
            if (req==null) return BadRequest("The details for the site plan venue request could not be found");
            try
            {
                var results = await _services.askAI(req);
                return Ok(results);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }


}