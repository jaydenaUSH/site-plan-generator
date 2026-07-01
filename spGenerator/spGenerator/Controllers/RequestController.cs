using spGenerator;

namespace spGenerator.Controllers {
    [RoutePrefix("api/requests")]
    public class RequestController : ApiController {

        private readonly SitePlanAIPOCEntities _db;
        public RequestController()
            {
            _db = new SitePlanAIPOCEntities();
            }

        //Post requests

        //Get requests
        [HttpGet]
        [Route("{id}:int")]
        public IHttpActionResult GetReqByID(int id) {
        }

    }
}