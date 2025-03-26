using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Application.Services;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserConfigController : ControllerBase
    {
        private readonly IUserConfigService userConfigService;

        public UserConfigController(IUserConfigService userConfigService)
        {
            this.userConfigService = userConfigService;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetUserLables(string userId)
        {
            return await userConfigService.GetUserLabels(userId);
        }

        [HttpPost]
        public async Task<ActionResult<TransObj>> Create(UserConfigModel userConfigModel)
        {
            if (userConfigModel == null)
                return BadRequest();

            var status = await userConfigService.Update(userConfigModel);

            return Ok(status);
        }

        [HttpGet]
        [Route("{userId}/{label}")]
        public async Task<ActionResult<TransObj>> SetUserLabels(string userId, string label)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(userId))
                return BadRequest();

            var status = await userConfigService.AddUserLabel(userId, label);
            return Ok(status);
        }
    }
}

