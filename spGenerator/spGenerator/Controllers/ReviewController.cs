    using spGenerator;
    using System;
    using System.Web.Http;

    namespace spGenerator.Controllers
    {
        [RoutePrefix("api/review")]
        public class ReviewController : ApiController
        {
            private readonly SitePlanAIPOCEntities _db;
            public ReviewController()
            {
                _db = new SitePlanAIPOCEntities();
        }
            [HttpGet]
            [Route("{id:int}/draft")]
            public IHttpActionResult getSitePlanDraft(int id) {
            var row = _db.SitePlanDrafts.Find(id);  
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
        public IHttpActionResult editDraft(int id, string edits) 
        {
            


            return null;
        }
        [HttpPost]
        [Route("finalize")]
        //Save the submitted structure into the final table 
        public IHttpActionResult finalizePlan(int id)
        {
            try{
                var row = _db.SitePlanDrafts.Find(id);
                SitePlanFinal finale = new SitePlanFinal
                {
                    SitePlanRequestId = row.Id,
                    SiteOverview = row.SiteOverview,
                    RecommendedLayout = row.RecommendedLayout,
                    VolunteerFlow = row.VolunteerFlow,
                    SupplyFlow = row.SupplyFlow,
                    Timeline = row.Timeline,
                    Risks = row.Risks,
                    PMReviewChecklist =row.PMReviewChecklist,
                    Reviewer = row.Reviewer,
                    AdditionalNotes = row.AdditionalNotes
                };
                _db.SitePlanFinals.Add(finale);
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