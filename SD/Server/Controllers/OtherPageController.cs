using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Services;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OtherPageController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly OtherPageService _otherPageService;
        public OtherPageController(UserService userService, OtherPageService otherPageService)
        {
            _userService = userService;
            _otherPageService = otherPageService;
        }

        [AllowAnonymous]
        // GET: api/OtherPage/ar/de
        [HttpGet("{fromLangCode}/{toLangCode}")]
        public async Task<ActionResult<OtherPageResModel>> GetLinks(string fromLangCode, string toLangCode)
        {
            try
            {
                return Ok(await _otherPageService.GetOPResModels(fromLangCode, toLangCode));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<OtherPageModel>> GetById(string id)
        {
            try
            {
                var result = await _otherPageService.GetOtherPageById(id);
                if (result == null) return NotFound();
                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(OtherPageModel page)
        {
            try
            {
                if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

                if (page == null)
                    return BadRequest();

                TransObj status = await _otherPageService.RegisterOtherPage(page);
                if (status.BoolVar)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new page record");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new page record");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOtherPage(OtherPageModel updatedPage)
        {
            try
            {
                if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

                TransObj status = await _otherPageService.UpdateOtherPage(updatedPage.OtherPageId, updatedPage);
                if (status.BoolVar)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                    status.SetringVar);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteOtherPage(string id)
        {
            try
            {
                if ((await _userService.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

                OtherPageModel pageToDelete = await _otherPageService.GetOtherPageById(id);

                if (pageToDelete == null)
                {
                    return NotFound($"User with Id = {id} not found");
                }

                return Ok(await _otherPageService.RemoveOtherPage(id));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}