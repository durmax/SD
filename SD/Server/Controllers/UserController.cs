using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Services;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly RelationshipService _relationshipService;

        public UserController(UserService userService, RelationshipService relationshipService)
        {
            _userService = userService;
            _relationshipService = relationshipService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> Get()
        {
            try
            {
                return Ok(await _userService.GetAllUsers());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        // GET: api/User/GetUsersByText/Dured
        [HttpGet("GetUsersByText/{searchText}")]
        public async Task<ActionResult<IEnumerable<UserRelationshipsWithOneUserDto>>> GetUsersByText(string searchText)
        {
            List<UserModel> foundUsers;
            try
            {
                foundUsers = await _userService.SearchUser(searchText);

                IEnumerable<UserRelationshipsWithOneUserDto> result = await _relationshipService.GetRelationships((await _userService.GetCurrentUser(User))?.UserId, foundUsers);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserModel>> GetCurrentUser()
        {
            try
            {
                var result = await _userService.GetUserById((await _userService.GetCurrentUser(User))?.UserId);
                if (result == null) return NotFound();
                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        // GET: api/User/GetUserById/5
        [HttpGet("GetUserByEmail/{email}")]
        public async Task<ActionResult<UserModel>> GetUserByEmail(string email)
        {
            try
            {
                var result = await _userService.GetUserByEmail(email);
                if (result == null) return NotFound();
                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Create")]
        public async Task<ActionResult<UserModel>> Create()
        {
            UserModel user = new();
            try
            {
                if (User !=null && User.Identity.IsAuthenticated)
                {
                    user.Name = User.Identity.Name;
                    user.Email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
                }

                var foundUser = await _userService.RegisterUserAsync(user);
                return Ok(foundUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new user record");
            }
        }

        [HttpPut("UpdateUser/{id}")]
        public async Task<ActionResult<TransObj>> UpdateUser(string id, UserModel updatedUser)
        {
            if (id != updatedUser.UserId)
            {
                return NotFound(new TransObj { BoolVar = false, SetringVar = $"Sorry, update error." });
            }
            try
            {
                return Ok(await _userService.UpdateUser(id, updatedUser));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteUser(string id)
        {
            try
            {
                return Ok(await _userService.RemoveUser(id));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
