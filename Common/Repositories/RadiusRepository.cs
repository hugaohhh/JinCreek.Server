using System.Linq;
using JinCreek.Server.Common.Models;

namespace JinCreek.Server.Common.Repositories
{
    public class RadiusRepository
    {
        private readonly RadiusDbContext _dbContext;

        public RadiusRepository(RadiusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// SimDevice 認証後のRadreplyの更新
        /// </summary>
        /// <param name="sim"></param>
        /// <param name="flag">SimDevice 認証がNGとOkの表示</param>
        /// <returns></returns>
        public int SimDeviceAuthUpdateRadreply(Sim sim,bool flag)
        {
            var reRadreply = _dbContext.Radreply
                .Where(r => r.Username == sim.UserName)
                .FirstOrDefault();
            reRadreply.Attribute = "Framed-IP-Address";

            if (flag)
            {
                reRadreply.Value = sim.SimDevice.Nw2AddressPool;
            }
            reRadreply.Value = reRadreply.Value;
            return _dbContext.SaveChanges();
        }

        public int Create(Radreply radreply)
        {
            _dbContext.Radreply.Add(radreply);
            return _dbContext.SaveChanges();
        }
    }
}
