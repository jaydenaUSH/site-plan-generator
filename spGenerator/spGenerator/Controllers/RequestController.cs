using spGenerator;

namespace spGenerator.Controllers { 
    public class RequestController : ApiController {

        private readonly SitePlanAIPOCEntities _db;
        public RequestController()
            {
            _db = new SitePlanAIPOCEntities();
            }


    }
}