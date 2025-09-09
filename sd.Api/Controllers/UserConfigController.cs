using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Application.Services;
using sd.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserConfigController : ControllerBase
    {
        private readonly IUserConfigService _userConfigService;

        public UserConfigController(IUserConfigService userConfigService)
        {
            this._userConfigService = userConfigService;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetUserLables(string userId)
        {
            var userConfig = await _userConfigService.GetById(userId);
            return userConfig is null ? ListStringConverter.ToList(userConfig?.Labels) : Enumerable.Empty<string>();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserConfigModel userConfigModel)
        {
            if (userConfigModel == null)
                return BadRequest();

            var status = await _userConfigService.Update(userConfigModel);

            return Ok(status);
        }

        [HttpGet]
        [Route("{userId}/{label}")]
        public async Task<IActionResult> SetUserLabels(string userId, string label)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(userId))
                return BadRequest();

            var status = await _userConfigService.AddUserLabel(userId, label);
            return Ok(status);
        }
    }
}

