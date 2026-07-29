using spGenerator;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace spGenerator.Controllers
{
    [RoutePrefix("api/requests")]
    public class RequestController : ApiController
    {

        private readonly ReqServices _services;
        public RequestController()
        {
            _services = new ReqServices();
        }

        //Post requests
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> createRequest(SitePlanRequest req)
        {
            if (req == null) return BadRequest("No site plan request venue information provided");
            try
            {
                var res = await _services.createReq(req);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        //Get by id requests
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetReqByID(int id)
        {
            if (id < 0) return BadRequest("No request to search for");
            try
            {
                var res = await _services.getReq(id);
                if (res == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(res);
                }
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            ;

        }
        [HttpGet]
        [Route("getAll")]
        public async Task<IHttpActionResult> getAllReqs()
        {
            var res = await _services.getAllReqs();
            if (res == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(res);
            }

        }

    }
}