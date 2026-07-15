using spGenerator;
using System.Threading.Tasks;
using System.Web.Http;

namespace spGenerator.Controllers {
    [RoutePrefix("api/requests")]
    public class RequestController : ApiController {

        private readonly ReqServices _services;
        public RequestController()
            {
            _services = new ReqServices();
            }

        //Post requests
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> createRequest(SitePlanRequest req) {
            string res = await _services.createReq(req);
            return Ok(res);
           
        }

        //Get requests
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetReqByID(int id) {
                var res = await _services.getReq(id);
            if (res == null)
            {
                return NotFound();
            }else  {
                return Ok(res); 
            }
            return Ok(res.row);

        }

    }
}