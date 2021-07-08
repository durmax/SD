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
        private readonly UserConfigService _userConfigService;

        public UserConfigController(UserLablesService userLablesService, UserConfigService userConfigService)
        {
            _userLablesService = userLablesService;
            _userConfigService = userConfigService;
        }

        [HttpGet("GetUserLables")]
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

                var status = await _userConfigService.SetUserConfigs(userConfigModel);

                return Ok(status);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new user record");
            }
        }

        [HttpGet]
        [Route("SetUserLabels/{userId}/{labels}")]
        public async Task<ActionResult<TransObj>> SetUserLabels(string userId, string labels)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(labels) || string.IsNullOrWhiteSpace(userId))
                    return BadRequest();

                var status = await _userLablesService.SetUserLabels(userId, labels);
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

