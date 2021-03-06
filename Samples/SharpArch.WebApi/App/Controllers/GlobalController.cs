﻿using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace SharpArch.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalController : ControllerBase
    {
        /// <summary>
        ///     Uses global default isolation level
        ///     <see cref="Startup.ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)" />
        /// </summary>
        [HttpGet("default")]
        public ActionResult<string> Default()
        {
            Log.Information("default");
            return "ok";
        }
    }
}
