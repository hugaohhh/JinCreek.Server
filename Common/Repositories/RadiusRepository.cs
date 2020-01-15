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
        /// SimDevice 認証後の　Radreplyの更新
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
            else if (!flag) reRadreply.Value = reRadreply.Value;
            return _dbContext.SaveChanges();
        }

        public int Create(Radreply radreply)
        {
            _dbContext.Radreply.Add(radreply);
            return _dbContext.SaveChanges();
        }

        /// <summary>
        /// 多要素認証後の　Radreplyの更新
        /// </summary>
        /// <param name="factor">多要素認証失敗するときに　NULLです</param>
        /// <param name="flag"> 多要素 認証がNGとOkの表示</param>
        public int MultiFactorAuthUpdateRadreply(SimDevice simDevice,FactorCombination factor, bool flag)
        {
            var reRadreply = _dbContext.Radreply
                .Where(r => r.Username == simDevice.Sim.UserName)
                .FirstOrDefault();
            reRadreply.Attribute = "Framed-IP-Address";
            if (flag)
            {
                reRadreply.Value = factor.NwAddress;
            }
            else if (!flag) reRadreply.Value = reRadreply.Value;

            return _dbContext.SaveChanges();
        }
    }
}
