using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Application.Services;
using sd.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OtherPageController : ControllerBase
    {
        private readonly IOtherPageService _otherPageService;
        private readonly IUserService _userService;

        public OtherPageController(IOtherPageService otherPageService, IUserService userService)
        {
            this._otherPageService = otherPageService;
            this._userService = userService;
        }

        // GET: api/OtherPage/ar/de
        [AllowAnonymous]
        [HttpGet("{fromLangCode}/{toLangCode}")]
        public async Task<ActionResult<List<DictionaryProviderDto>>> GetLinks(string fromLangCode, string toLangCode)
        {
            return Ok(await _otherPageService.GetOPResModels(fromLangCode, toLangCode));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<OtherPageModel>> GetById(string id)
        {
            if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            var result = await _otherPageService.GetById(id);
            if (result == null) return NotFound();
            return result;
        }

        [HttpGet]
        [Route("Host/{host}")]
        public async Task<IActionResult> GetByHost(string host)
        {
            if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            var cursor = await _otherPageService.GetByCondition(u => u.Host.Contains(host));
            var res = cursor.FirstOrDefault();
            
            if (res == null) return NotFound();
            
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OtherPageModel page)
        {
            if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            if (page == null)
                return BadRequest();

            var status = await _otherPageService.Create(page);
            if (status)
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new page record");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOtherPage(OtherPageModel updatedPage)
        {
            if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            var status = await _otherPageService.Update(updatedPage);
            if (status)
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                status);
            }
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteOtherPage(string id)
        {
            if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            var pageToDelete = await _otherPageService.GetById(id);

            if (pageToDelete == null)
            {
                return NotFound($"User with Id = {id} not found");
            }

            return Ok(await _otherPageService.Delete(id));
        }


        [Authorize]
        [HttpPost("GetAI/")]
        public async Task<IActionResult> GetAI([FromBody] string ExampleURL, CancellationToken cancellationToken)
        {
            var result = await _otherPageService.GetModelByAI(ExampleURL, cancellationToken);
            if (!string.IsNullOrEmpty(result.Pattern))
            return Ok(result);
            else
                return StatusCode(StatusCodes.Status302Found,
                "The same Pattern is found");
        }
    }
}