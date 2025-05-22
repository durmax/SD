using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Application.Services;
using sd.Api.Repositories;
using sd.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly IRelationshipService relationshipService;

        public UserController(IUserService userService, IRelationshipService relationshipService)
        {
            _userService = userService;
            this.relationshipService = relationshipService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> Get()
        {
            return Ok(await _userService.GetByCondation(u => true));
        }

        // GET: api/User/GetUsersByText/Dured
        [HttpGet("GetUsersByText/{searchText}")]
        public async Task<ActionResult<IEnumerable<UserRelationshipsWithOneUserDto>>> GetUsersByText(string searchText)
        {
            var res = await _userService.GetByCondation(u => u.Name.ToLower().Contains(searchText.ToLower()));
            List<UserModel> foundUsers = res.ToList();

            IEnumerable<UserRelationshipsWithOneUserDto> result = await relationshipService.GetRelationships((await _userService.GetCurrentUser(User))?.UserId, foundUsers);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserModel>> GetCurrentUser()
        {
            var cu =await _userService.GetCurrentUser(User);

            var result = await _userService.GetById(cu.UserId);
            if (result == null) return NotFound();
            return result;
        }

        // GET: api/User/GetUserById/5
        [HttpGet("GetUserByEmail/{email}")]
        public async Task<ActionResult<UserModel>> GetUserByEmail(string email)
        {
            var result = await _userService.GetByCondation(u => u.Email == email);
            if (result.First() == null) return NotFound();
            return result.First();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<UserModel>> Create(UserModel user)
        {
            if (User != null && User.Identity.IsAuthenticated)
            {
                user.Name = User.Identity.Name;
                user.Email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            }

            var registeredUser = await _userService.Create(user.Email);
            return Ok(registeredUser);
        }

        [HttpPut("UpdateUser/{id}")]
        public async Task<ActionResult<bool>> UpdateUser(string id, UserModel updatedUser)
        {
            if (id != updatedUser.UserId)
            {
                return NotFound(new TransObj { BoolVar = false, SetringVar = $"Sorry, update error." });
            }
            return Ok(await _userService.Update(updatedUser));
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteUser(string id)
        {
            return Ok(await _userService.Delete(id));
        }
    }
}
