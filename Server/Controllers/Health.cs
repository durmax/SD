using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sd.Api.Controllers
{
    [AllowAnonymous]
    [Route("api")]
    [ApiController]
    public class Health : ControllerBase
    {
        [HttpGet]
        public ActionResult<bool> Index()
        {
            return Ok(true);
        }
    }
}
