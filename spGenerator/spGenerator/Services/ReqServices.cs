
using System;
using System.Threading.Tasks;

namespace spGenerator
{
    public class ReqServices
    {
        private readonly SitePlanAIPOCEntities _db;
        public ReqServices()
        {
            _db = new SitePlanAIPOCEntities();
            _db.Configuration.ProxyCreationEnabled = false;

        }

        public async Task<dynamic> createReq(SitePlanRequest req) {
            _db.SitePlanRequests.Add(req);
            await _db.SaveChangesAsync();
            return req;
        }
        public async Task<dynamic> getReq(int id)
        {
            var row = await _db.SitePlanRequests.FindAsync(id);
            return row;
        }
    }
}