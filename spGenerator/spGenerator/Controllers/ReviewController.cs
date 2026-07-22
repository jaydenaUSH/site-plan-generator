    using Microsoft.SqlServer.Server;
    using OpenAI;
    using OpenAI.Images;
    using OpenAI.Responses;
    using spGenerator;
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    namespace spGenerator.Controllers
    {
        [RoutePrefix("api/review")]
        public class ReviewController : ApiController
        {
            private readonly ReviewServices _services;
            public ReviewController()
            {
                _services = new ReviewServices();
            }
            [HttpGet]
            [Route("{id:int}/draft")]
            public async Task<IHttpActionResult> getSitePlanDraft(int id)
            {
                if (id <0) return BadRequest("Site Plan Draft not found or missing");
                try
                {
                    var res = await _services.getDraft(id);
                    if (res == null) {
                        return NotFound(); 
                    }
                    else
                    {
                        return Ok(res);
                    }
                } catch(Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            
            }
            //edit everything 
            [HttpPut]
            [Route("{id:int}/draft/edit")]
            public async Task<IHttpActionResult> editDraft(int id, [FromBody] string edits)
            {
                if (id <0) return BadRequest("Site Plan Draft not found or missing");
                if (edits == null || edits == "") return BadRequest("No edit to be made");
                try {
                    var res = await _services.editDraft(id, edits);
                    return Ok(res);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            
            }
            [HttpPost]
            [Route("{id:int}/finalize")]
            //Save the submitted structure into the final table 
            public async Task<IHttpActionResult> finalizePlan(int id)
            {
                if (id <0) return BadRequest("Site Plan Draft not found or missing");
                try {
                    var res = await _services.finalizeDraft(id);
                    return Ok(res);

                } catch(Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            

            }
        }
    }