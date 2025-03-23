using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Infrastructure.Repositories;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserConfigController : ControllerBase
    {
        private readonly IUserConfigRepository _userConfigRepo;

        public UserConfigController(UserConfigRepository userConfigRepo)
        {
            _userConfigRepo = userConfigRepo;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetUserLables(string userId)
        {
            return await _userConfigRepo.GetUserLabels(userId);
        }

        [HttpPost]
        public async Task<ActionResult<TransObj>> Create(UserConfigModel userConfigModel)
        {
            if (userConfigModel == null)
                return BadRequest();

            var status = await _userConfigRepo.SetUserConfigs(userConfigModel);

            return Ok(status);
        }

        [HttpGet]
        [Route("{userId}/{label}")]
        public async Task<ActionResult<TransObj>> SetUserLabels(string userId, string label)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(userId))
                return BadRequest();

            var status = await _userConfigRepo.AddUserLabel(userId, label);
            return Ok(status);
        }
    }
}

