using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Controllers
{
    public class StatusCodeController : Controller
    {      

        public StatusCodeController()
        {
            

        }

        // GET: /<controller>/

        [HttpGet("/StatusCode/{statusCode}")]

        public IActionResult Index(int statusCode)
        {
            return View(statusCode);
        }
    }
}
