using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Interfaces;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtherPageController : ControllerBase
    {
        private readonly IOtherPageRepository _otherPageService;
        public OtherPageController(IOtherPageRepository otherPageService)
        {
            _otherPageService = otherPageService;
        }

        // GET: api/OtherPage/door/en/ar/de
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

        [HttpPost]
        [Route("GetById")]
        public async Task<ActionResult<OtherPageModel>> GetById(TransObj transObj)
        {
            try
            {
                var result = await _otherPageService.GetOtherPageById(transObj);
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
        [Route("Create")]
        public async Task<ActionResult<TransObj>> Create(OtherPageModel page)
        {
            try
            {
                if (page == null)
                    return BadRequest();

                TransObj status = await _otherPageService.RegisterOtherPage(page);
                return Ok(status);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new page record");
            }
        }


        [HttpPut("Update/{id}")]
        public async Task<ActionResult<TransObj>> UpdateOtherPage(string id, OtherPageModel updatedPage)
        {
            if (id != updatedPage.OtherPageId)
            {
                return NotFound(new TransObj { BoolVar = false, SetringVar = $"Sorry, update error." });
            }
            try
            {
                return Ok(await _otherPageService.UpdateOtherPage(id, updatedPage));
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
                TransObj transObj = new TransObj() { SetringVar = id };
                OtherPageModel pageToDelete = await _otherPageService.GetOtherPageById(transObj);

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