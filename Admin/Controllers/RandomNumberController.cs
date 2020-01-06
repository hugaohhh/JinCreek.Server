using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RandomNumberController : ControllerBase
    {
        [Authorize]
        [HttpGet("Generate")]
        public ActionResult<int> Generate()
        {
            return new Random().Next(0, 1000);
        }
    }
}
