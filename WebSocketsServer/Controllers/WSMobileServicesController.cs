using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.MobileServices;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebSocketsServer.Controllers
{
    [Route("/api/WSMobileServices/{game}")]
    public class WSMobileServicesController : Controller
    {
        readonly Models.MobileServiceSettings _mobileSettings;

        public WSMobileServicesController(IOptions<Models.MobileServiceSettings> mobileSettings)
        {
            _mobileSettings = mobileSettings.Value;
        }

        [HttpGet]
        public async Task<IEnumerable<Models.Leaderboard>> GetAll(string game)
        {
            var query = Table.CreateQuery().Where(q => q.game == game);
            var items = await query?.ToListAsync();
            return items;
        }

        [HttpDelete]
        public async Task<bool> Delete(string game, Models.Leaderboard leader)
        {
            leader.game = game;
            await Table.DeleteAsync(leader);
            return true;
        }

        [HttpPost]
        public async Task<Models.Leaderboard> Post(string game, [FromBody]Models.Leaderboard leader)
        {
            leader.game = game;
            await Table.InsertAsync(leader);
            return leader;
        }

        [HttpPost]
        [Route("update")]
        public async Task<bool> Update([FromBody]Models.Leaderboard leader)
        {
            await Table.UpdateAsync(leader);
            return true;
        }

        IMobileServiceTable<Models.Leaderboard> Table
        {
            get
            {
                MobileServiceClient client = new MobileServiceClient(
                    new Uri(_mobileSettings.applicationUri),
                    new[] { new ZumoHandler() }
                );

                var table = client.GetTable<Models.Leaderboard>();
                return table;
            }
        }
    }
}
