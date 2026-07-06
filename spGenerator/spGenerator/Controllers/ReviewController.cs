    using spGenerator;
    using System;
    using System.Web.Http;

    namespace spGenerator.Controllers
    {
        [RoutePrefix("/api/review")]
        public class ReviewController : ApiController
        {
            private readonly SitePlanAIPOCEntities _db;
            public ReviewController()
            {
                _db = new SitePlanAIPOCEntities(); // Replace  SitePlanAIPOCEntities to SitePlanDraft (placeholder for now)
        }
            [HttpGet]
            [Route("{id:int}/draft")]
            public IHttpActionResult getSitePlanDraft(int id) {
            var row = _db.SitePlanRequests.Find(id); // Replace SitePlanRequests to SitePlanDraft (placeholder for now) 
            if(row!= null) {
                return Ok(row);
            }
            else
            {
                return NotFound();
            }
        }
        //edit everything 
        [HttpPut]
        [Route("{id:int}/draft/edit")]
        public IHttpActionResult editDraft(int id, SitePlanRequest req) //Replace all instances of SitePlanRequest to SitePlanDraft
        {
            var ogVals = _db.SitePlanRequests.Find(id);


            return null;
        } 

        //Save the submitted structure into the final table 
        public IHttpActionResult finalizePlan(int id)
        {
            try{
                var row = _db.SitePlanRequests.Find(id); // Replace SitePlanRequests to SitePlanDraft (placeholder for now) 
                _db.SitePlanRequests.Add(row);// Replace SitePlanRequests to SitePlanFinal (placeholder for now) for all below lines
                _db.SaveChanges();


                return Ok();
                //Save reviewer, and timestamp
            }
            catch (Exception ex) {
                return InternalServerError(ex);
            }
        }

        }
}