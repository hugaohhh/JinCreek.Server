using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    public class RandomNumberController : Controller
    {
        [Authorize]
        [HttpGet("Generate")]
        public ActionResult<Int32> Generate()
        {
            return new Random().Next(0, 1000);
        }
    }
}
