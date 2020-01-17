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
        public int UpdateRadreply(Sim sim,bool flag)
        {
            var reRadreply = _dbContext.Radreply
                .Where(r => r.Username == sim.UserName)
                .FirstOrDefault();
            if (reRadreply != null)
            {
                reRadreply.Attribute = "Framed-IP-Address";
                reRadreply.Value = flag ? sim.SimDevice.Nw2AddressPool : reRadreply.Value;
            }
            return _dbContext.SaveChanges();
        }

        public int Create(Radreply radreply)
        {
            _dbContext.Radreply.Add(radreply);
            return _dbContext.SaveChanges();
        }

        public Radreply GetRadreply(string username)
        {
            return _dbContext.Radreply
                .Where(r => r.Username == username).FirstOrDefault();
        }

        /// <summary>
        /// 多要素認証後の　Radreplyの更新
        /// </summary>
        /// <param name="simDevice"></param>
        /// <param name="factor">多要素認証失敗するときに　NULLです</param>
        /// <param name="flag"> 多要素 認証がNGとOkの表示</param>
        public int UpdateRadreply(SimDevice simDevice,FactorCombination factor, bool flag)
        {
            var reRadreply = _dbContext.Radreply
                .Where(r => r.Username == simDevice.Sim.UserName)
                .FirstOrDefault();
            if (reRadreply != null)
            {
                reRadreply.Attribute = "Framed-IP-Address";
                reRadreply.Value = flag ? factor.NwAddress : reRadreply.Value;
            }
            return _dbContext.SaveChanges();
        }
    }
}
