using ConsoleAppFramework;
using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace JinCreek.Server.Batch
{
    public class RadiusSync : ConsoleAppBase
    {
        // configurationを扱う定型としたいため
        // ReSharper disable once NotAccessedField.Local
        private readonly IConfiguration _configuration;
        private readonly ILogger<RadiusSync> _logger;
        private readonly AuthenticationRepository _authenticationRepository;
        private readonly RadiusRepository _radiusRepository;

        public RadiusSync(ILogger<RadiusSync> logger,IConfiguration configuration, AuthenticationRepository authenticationRepository, RadiusRepository radiusRepository)
        {
            this._logger = logger;
            this._configuration = configuration;
            this._authenticationRepository = authenticationRepository;
            this._radiusRepository = radiusRepository;
        }

        [Command("radius_sync")]
        public void Main(
            [Option("organization_code", "organization code")]
            int organizationCode
            )
        {
            _logger.LogInformation($"{GetType().FullName} Start");
            try
            {
                var organization = _authenticationRepository.GetOrganization(organizationCode);
                if (organization.StartDate > DateTime.Now.Date
                    || (organization.EndDate != null && organization.EndDate < DateTime.Now.Date))
                {
                    _logger.LogError($"Organization is not target {organization.Code} {organization.StartDate} {organization.EndDate}");
                    return;
                }
                var sims = _authenticationRepository.GetSim(organizationCode);
                var simGroups = _authenticationRepository.GetSimGroup(organizationCode);
                
                SimGroupSync(simGroups);
                SimSync(sims);

                _logger.LogInformation($"{GetType().FullName} Success");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogInformation($"{GetType().FullName} Error");
            }
        }

        //SimGroupに対応の radgroupcheck radgroupreply同期
        private void SimGroupSync(List<SimGroup> simGroups)
        {
            foreach (var simGroup in simGroups)
            {
                var radgroupcheckList = _radiusRepository.GetRadgroupcheckList(simGroup.Id);
                if (radgroupcheckList != null && radgroupcheckList.Count > 0) // 更新
                {
                    UpdateRadgroupcheck(radgroupcheckList, simGroup);
                }
                else //登録
                {
                    CreateRadgroupcheck(simGroup);
                }
                var radgroupreplyList = _radiusRepository.GetRadgroupreplyList(simGroup.Id);
                if (radgroupreplyList != null && radgroupreplyList.Count > 0) // 更新
                {
                    UpdateRadgroupreply(radgroupreplyList, simGroup);
                }
                else //登録
                {
                    CreateRadgroupreply(simGroup);
                }
                RadippoolSync(simGroup);
            }
        }

        //SimGroupに対応のRadippool同期
        private void RadippoolSync(SimGroup simGroup)
        {
            using (var transaction = _radiusRepository._dbContext.Database.BeginTransaction())
            {
                var radippools = _radiusRepository.GetRadippool(simGroup.IsolatedNw1IpPool);
                var network = IPNetwork.Parse(simGroup.IsolatedNw1IpRange);
                // ReSharper disable once RedundantCast
                if ((int)network.Cidr == 32) //32の場合
                {
                    if (radippools != null && radippools.Count > 0) //更新
                    {
                        radippools[0].Framedipaddress = network.Network.ToString();
                        _radiusRepository.UpdateRadippool(radippools[0]);
                    }
                    else //登録
                    {
                        CreateRadippool(network.Network, simGroup);
                    }
                }
                // 32以外の場合
                var ipAddresses = network.ListIPAddress(FilterEnum.Usable);
                if (radippools != null && radippools.Count > 0) //更新
                {
                    if (ipAddresses.Count >= radippools.Count) // 更新後のIpAddressは増えた
                    {
                        for (int i = 0; i < radippools.Count; i++)
                        {
                            radippools[i].Framedipaddress = ipAddresses[i].ToString();
                            _radiusRepository.UpdateRadippool(radippools[i]);
                        }

                        for (int i = radippools.Count; i < ipAddresses.Count; i++) //増えたIPAddress追加
                        {
                            CreateRadippool(ipAddresses[i], simGroup);
                        }
                    }
                    else //更新後のIpAddressは減った
                    {
                        for (int i = 0; i < ipAddresses.Count; i++)
                        {
                            radippools[i].Framedipaddress = ipAddresses[i].ToString();
                            _radiusRepository.UpdateRadippool(radippools[i]);
                        }
                        //減ったの削除
                        _radiusRepository.DeleteRadippool(radippools.GetRange((int)ipAddresses.Count, radippools.Count - (int)ipAddresses.Count));
                    }
                }
                else//登録
                {
                    foreach (var ip in ipAddresses)
                    {
                        CreateRadippool(ip, simGroup);
                    }
                }
                transaction.Commit();
            }
        }

        // Simに対応のradcheck　radusergroupの同期
        private void SimSync(List<Sim> sims)
        {
            foreach (var sim in sims)
            {
                var radcheckList = _radiusRepository.GetRadcheckList(sim.UserName + "@" + sim.SimGroup.UserNameSuffix);
                if (radcheckList != null && radcheckList.Count > 0) //更新
                {
                    UpdateRadcheck(radcheckList, sim);
                }
                else //登録
                {
                    CreateRadcheck(sim);
                }
                var radusergroup = _radiusRepository.GetRadusergroup(sim.UserName + "@" + sim.SimGroup.UserNameSuffix);
                if (radusergroup != null) //更新
                {
                    radusergroup.Groupname = sim.SimGroup.Id.ToString();
                    _radiusRepository.UpdateRadusergroup(radusergroup);
                }
                else //登録
                {
                    CreateRadusergroup(sim);
                }
            }
        }

        private void CreateRadippool(IPAddress ip, SimGroup simGroup)
        {
            var radippool = new Radippool()
            {
                Framedipaddress = ip.ToString(),
                Nasipaddress = "",
                Calledstationid = "",
                Callingstationid = "",
                Username = "",
                Poolkey = "",
                PoolName = simGroup.IsolatedNw1IpPool
            };
            _radiusRepository.CreateRadippool(radippool);
        }

        private void CreateRadusergroup(Sim sim)
        {
            var radusergroup = new Radusergroup()
            {
                Username = sim.UserName + "@" + sim.SimGroup.UserNameSuffix,
                Groupname = sim.SimGroup.Id.ToString()
            };
            _radiusRepository.CreateRadusergroup(radusergroup);
        }

        private void CreateRadcheck(Sim sim)
        {
            var radcheck1 = new Radcheck()
            {
                Username = sim.UserName + "@" + sim.SimGroup.UserNameSuffix,
                Attribute = "Cleartext-Password",
                Op = ":=",
                Value = sim.Password
            };
            var radcheck2 = new Radcheck()
            {
                Username = sim.UserName + "@" + sim.SimGroup.UserNameSuffix,
                Attribute = "Calling-Station-Id",
                Op = "==",
                Value = sim.Msisdn
            };
            _radiusRepository.CreateRadcheck(radcheck1, radcheck2);
        }

        private void UpdateRadcheck(List<Radcheck> radchecks, Sim sim)
        {
            foreach (var radcheck in radchecks)
            {
                if (radcheck.Attribute.Equals("Cleartext-Password") && radcheck.Op.Equals(":="))
                {
                    radcheck.Value = sim.Password;
                }
                if (radcheck.Attribute.Equals("Calling-Station-Id") && radcheck.Op.Equals("=="))
                {
                    radcheck.Value = sim.Msisdn;
                }

                _radiusRepository.UpdateRadcheck(radcheck);
            }
        }

        private void UpdateRadgroupreply(List<Radgroupreply> radgroupreplyList, SimGroup simGroup)
        {
            foreach (var radgroupreply in radgroupreplyList)
            {
                if (radgroupreply.Attribute.Equals("MS-Primary-DNS-Server") && radgroupreply.Op.Equals(":="))
                {
                    radgroupreply.Value = simGroup.PrimaryDns;
                }
                if (radgroupreply.Attribute.Equals("MS-Secondary-DNS-Server") && radgroupreply.Op.Equals(":="))
                {
                    radgroupreply.Value = simGroup.SecondaryDns;
                }
                _radiusRepository.UpdateRadgroupcheck(radgroupreply);
            }
        }

        private void UpdateRadgroupcheck(List<Radgroupcheck> radgroupcheckList, SimGroup simGroup)
        {
            foreach (var radgroupcheck in radgroupcheckList)
            {
                if (radgroupcheck.Attribute.Equals("Pool-Name") && radgroupcheck.Op.Equals(":="))
                {
                    radgroupcheck.Value = simGroup.IsolatedNw1IpPool;
                }
                if (radgroupcheck.Attribute.Equals("Called-Station-Id") && radgroupcheck.Op.Equals("=="))
                {
                    radgroupcheck.Value = simGroup.Apn;
                }
                if (radgroupcheck.Attribute.Equals("NAS-IP-Address") && radgroupcheck.Op.Equals("=="))
                {
                    radgroupcheck.Value = simGroup.NasIp;
                }
                _radiusRepository.UpdateRadgroupcheck(radgroupcheck);
            }
        }

        private void CreateRadgroupreply(SimGroup simGroup)
        {
            var radgroupreply1 = new Radgroupreply()
            {
                Attribute = "MS-Primary-DNS-Server",
                Groupname = simGroup.Id.ToString(),
                Op = ":=",
                Value = simGroup.PrimaryDns
            };
            var radgroupreply2 = new Radgroupreply()
            {
                Attribute = "MS-Secondary-DNS-Server",
                Groupname = simGroup.Id.ToString(),
                Op = ":=",
                Value = simGroup.SecondaryDns
            };
            _radiusRepository.CreateRadgroupreply(radgroupreply1, radgroupreply2);
        }

        private void CreateRadgroupcheck(SimGroup simGroup)
        {
            var radgroupcheck1 = new Radgroupcheck()
            {
                Attribute = "Pool-Name",
                Groupname = simGroup.Id.ToString(),
                Op = ":=",
                Value = simGroup.IsolatedNw1IpPool
            };
            var radgroupcheck2 = new Radgroupcheck()
            {
                Attribute = "Called-Station-Id",
                Groupname = simGroup.Id.ToString(),
                Op = "==",
                Value = simGroup.Apn
            };
            var radgroupcheck3 = new Radgroupcheck()
            {
                Attribute = "NAS-IP-Address",
                Groupname = simGroup.Id.ToString(),
                Op = "==",
                Value = simGroup.NasIp
            };
            _radiusRepository.CreateRadgroupcheck(radgroupcheck1, radgroupcheck2, radgroupcheck3);
        }
    }
}
