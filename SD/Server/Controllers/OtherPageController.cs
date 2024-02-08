using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Services;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtherPageController : ControllerBase
    {
        private readonly OtherPageService _otherPageService;
        public OtherPageController(OtherPageService otherPageService)
        {
            _otherPageService = otherPageService;
        }

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(OtherPageModel page)
        {
            try
            {
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

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateOtherPage(OtherPageModel updatedPage)
        {
            try
            {
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

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteOtherPage(string id)
        {
            try
            {
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