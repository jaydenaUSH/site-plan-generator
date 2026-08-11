
using Microsoft.AspNetCore.Http;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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

        public async Task<dynamic> createReq(SitePlanRequest req)
        {
            req.CreatedAtUtc = DateTime.UtcNow;
            req.RoomBlueprintFilePath = req.RoomBlueprintFilePath + Guid.NewGuid().ToString("N");
            _db.SitePlanRequests.Add(req);
            try
            {
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return req;
        }
        public async Task<dynamic> getReq(int id)
        {
            return await _db.SitePlanRequests.FindAsync(id);
        }

        public async Task<dynamic> getAllReqs()
        {
            return await _db.SitePlanRequests.OrderByDescending(w => w.Deadline).ToListAsync();
        }
        public async Task<dynamic> uploadFile(HttpPostedFile file)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blueprints", "ogInput", Path.GetFileName(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.InputStream.CopyToAsync(stream);
            }

            return "File sucessfully uploaded";
        }
    }
}