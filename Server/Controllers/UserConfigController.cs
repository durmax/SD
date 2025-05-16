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
            var userConfigs = await _userConfigService.GetByCondation(u => u.UserId == userId);
            return userConfigs.Any() ? ListStringConverter.ToList(userConfigs.First().Labels) : Enumerable.Empty<string>();
        }

        [HttpPost]
        public async Task<ActionResult<TransObj>> Create(UserConfigModel userConfigModel)
        {
            if (userConfigModel == null)
                return BadRequest();

            var status = await _userConfigService.Update(userConfigModel);

            return Ok(status);
        }

        [HttpGet]
        [Route("{userId}/{label}")]
        public async Task<ActionResult<TransObj>> SetUserLabels(string userId, string label)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(userId))
                return BadRequest();

            var status = await _userConfigService.AddUserLabel(userId, label);
            return Ok(status);
        }
    }
}

