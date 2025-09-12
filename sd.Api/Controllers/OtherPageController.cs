using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Application.Services;
using sd.Shared;

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
        public async Task<ActionResult<List<OtherPageResModel>>> GetLinks(string fromLangCode, string toLangCode)
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
    }
}