using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Infrastructure.Repositories;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OtherPageController : ControllerBase
    {
        private readonly IOtherPageRepository _otherPageRepo;
        private readonly IUserRepository _userRepo;

        public OtherPageController(OtherPageRepository otherPageRepo, IUserRepository userRepo)
        {
            _otherPageRepo = otherPageRepo;
            this._userRepo = userRepo;
        }

        // GET: api/OtherPage/ar/de
        [AllowAnonymous]
        [HttpGet("{fromLangCode}/{toLangCode}")]
        public async Task<ActionResult<List<OtherPageResModel>>> GetLinks(string fromLangCode, string toLangCode)
        {
            return Ok(await _otherPageRepo.GetOPResModels(fromLangCode, toLangCode));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<OtherPageModel>> GetById(string id)
        {
            if ((await _userRepo.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            var result = await _otherPageRepo.GetOtherPageById(id);
            if (result == null) return NotFound();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OtherPageModel page)
        {
            if ((await _userRepo.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            if (page == null)
                return BadRequest();

            TransObj status = await _otherPageRepo.Create(page);
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

        [HttpPut]
        public async Task<IActionResult> UpdateOtherPage(OtherPageModel updatedPage)
        {
            if ((await _userRepo.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            TransObj status = await _otherPageRepo.Update(updatedPage.OtherPageId, updatedPage);
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

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteOtherPage(string id)
        {
            if ((await _userRepo.GetCurrentUser(User))?.Role != Role.Owner) return StatusCode(StatusCodes.Status401Unauthorized);

            OtherPageModel pageToDelete = await _otherPageRepo.GetOtherPageById(id);

            if (pageToDelete == null)
            {
                return NotFound($"User with Id = {id} not found");
            }

            return Ok(await _otherPageRepo.Delete(id));
        }
    }
}