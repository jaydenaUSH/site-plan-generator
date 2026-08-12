using spGenerator;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
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
        [HttpPost]
        [Route("uploadFile")]
        public async Task<IHttpActionResult> uploadFile()
        {
            var file = HttpContext.Current.Request.Files["file"];

            if (file == null) return BadRequest("No file attatched");
            if(Path.GetExtension(file.FileName)!= "pdf"&& Path.GetExtension(file.FileName) != "png"&& Path.GetExtension(file.FileName)!= "jpg" && Path.GetExtension(file.FileName) != "jpeg"){
                return BadRequest("Invalid file type");
            }
                var res = await _services.uploadFile(file);
            return Ok(res);
        }
    }
}