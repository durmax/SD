using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Infrastructure.Repositories;
using sd.Api.Repositories;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepo;
        private readonly IRelationshipRepository _relationshipRepo;

        public UserController(IRelationshipRepository relationshipRepo, IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _relationshipRepo = relationshipRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> Get()
        {
            return Ok(await _userRepo.GetAllUsers());
        }

        // GET: api/User/GetUsersByText/Dured
        [HttpGet("GetUsersByText/{searchText}")]
        public async Task<ActionResult<IEnumerable<UserRelationshipsWithOneUserDto>>> GetUsersByText(string searchText)
        {
            List<UserModel> foundUsers;
            foundUsers = await _userRepo.SearchUser(searchText);

            IEnumerable<UserRelationshipsWithOneUserDto> result = await _relationshipRepo.GetRelationships((await _userRepo.GetCurrentUser(User))?.UserId, foundUsers);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserModel>> GetCurrentUser()
        {
            var result = await _userRepo.GetUserById((await _userRepo.GetCurrentUser(User))?.UserId);
            if (result == null) return NotFound();
            return result;
        }

        // GET: api/User/GetUserById/5
        [HttpGet("GetUserByEmail/{email}")]
        public async Task<ActionResult<UserModel>> GetUserByEmail(string email)
        {
            var result = await _userRepo.GetUserByEmail(email);
            if (result == null) return NotFound();
            return result;
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

            var registeredUser = await _userRepo.RegisterUserAsync(user.Email);
            return Ok(registeredUser);
        }

        [HttpPut("UpdateUser/{id}")]
        public async Task<ActionResult<TransObj>> UpdateUser(string id, UserModel updatedUser)
        {
            if (id != updatedUser.UserId)
            {
                return NotFound(new TransObj { BoolVar = false, SetringVar = $"Sorry, update error." });
            }
            return Ok(await _userRepo.UpdateUser(id, updatedUser));
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteUser(string id)
        {
            return Ok(await _userRepo.Delete(id));
        }
    }
}
