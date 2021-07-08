using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Services;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserConfigController : ControllerBase
    {
        private readonly UserLablesService _userLablesService;

        public UserConfigController(UserLablesService userLablesService)
        {
            _userLablesService = userLablesService;
        }

        [HttpGet("GetUserLables/{userId}")]
        public async Task<IEnumerable<string>> GetUserLables(string userId)
        {
            return await _userLablesService.GetUserLabels(userId);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<TransObj>> Create(UserConfigModel userConfigModel)
        {
            try
            {
                if (userConfigModel == null)
                    return BadRequest();

                var status = await _userLablesService.SetUserLabels(userConfigModel.UserId, userConfigModel.Labels);
                return Ok(status);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new user record");
            }
        }

    }

}

