using JinCreek.Server.Common.Models;
using JinCreek.Server.Common.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DomainsController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly UserRepository _userRepository;
        private readonly MainDbContext _context;

        public DomainsController(IAuthorizationService authorizationService, UserRepository userRepository,
            MainDbContext context)
        {
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _context = context;
        }

        // GET: api/Domains
        [HttpGet]
        public IEnumerable<Domain> GetDomains()
        {
            return _userRepository.GetDomain();
        }

        // GET: api/Domains/5
        [HttpGet("{id}")]
        public ActionResult<Domain> GetDomain(Guid id)
        {
            return _userRepository.GetDomain(id);
        }
    }
}
