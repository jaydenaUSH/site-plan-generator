
using System.Threading.Tasks;

namespace spGenerator
{
    public class ReqServices
    {
        private readonly SitePlanAIPOCEntities _db;
        public ReqServices()
        {
            _db = new SitePlanAIPOCEntities();
        }

        public async Task<string> createReq(SitePlanRequest req) {
            _db.SitePlanRequests.Add(req);
            try
            {
                _db.SaveChanges();
                return "Request successfully added";
            }
            catch
            {
                return "Error adding user";
            }
        }
        public async Task<dynamic> getReq(int id)
        {
            var row = _db.SitePlanRequests.Find(id);
            return row;
        }
    }
}