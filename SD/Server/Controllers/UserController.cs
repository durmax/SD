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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userService;

        public UserController(IUserRepository userService)
        {
            _userService = userService;
        }

        // GET: api/User
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
        [HttpGet("GetUsersByText/{CurrentUserId}/{searchText}")]
        public async Task<ActionResult<Dictionary<string, Tuple<string, string>>>> GetUsersByText(string CurrentUserId, string searchText)
        {
            try
            {
                var result = await _userService.SearchUser(CurrentUserId, searchText);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet("GetAllFriends/{userId}")]
        public async Task<ActionResult<Dictionary<string, string>>> GetAllFriends(string userId)
        {
            try
            {
                var result = await _userService.GetAllFriends(userId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        // GET: api/User/GetUserById/5
        [HttpGet("GetUserById/{id}")]
        public async Task<ActionResult<UserModel>> GetUserById(string id)
        {
            try
            {
                var result = await _userService.GetUserById(id);
                if (result == null) return NotFound();
                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet("GetUserInfoById/{id}")]
        public async Task<ActionResult<UserInfo>> GetUserInfoById(string id)
        {
            try
            {
                var result = await _userService.GetUserInfoById(id);
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
        public async Task<ActionResult<TransObj>> Create(UserModel user)
        {
            try
            {
                if (user == null)
                    return BadRequest();
                if (string.IsNullOrWhiteSpace(user.UserId) || string.IsNullOrWhiteSpace(user.Email))
                    return BadRequest();

                TransObj status = await _userService.RegisterUserAsync(user);
                return Ok(status);

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
                UserModel userToDelete = await _userService.GetUserById(id);

                if (userToDelete == null)
                {
                    return NotFound($"User with Id = {id} not found");
                }

                return Ok(await _userService.RemoveUser(id));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

        [HttpPost("AddFriendRequest/{userId}/{friendId}")]
        public async Task<ActionResult> AddFriendRequest(string userId, string friendId)
        {
            try
            {
                await _userService.AddFriendRequest(userId, friendId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }
        [HttpPost("RemoveFriendRequest/{UserId}/{friendId}")]
        public async Task<ActionResult> RemoveFriendRequest(string UserId, string friendId)
        {
            try
            {
                await _userService.RemoveFriendRequest(UserId, friendId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }
        [HttpPost("AddFriend/{UserId}/{friendId}")]
        public async Task<ActionResult> AddFriend(string UserId, string friendId)
        {
            try
            {
                await _userService.AddFriend(UserId, friendId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }
        [HttpPost("RemoveFriend/{UserId}/{friendId}")]
        public async Task<ActionResult> RemoveFriend(string UserId, string friendId)
        {
            try
            {
                await _userService.RemoveFriend(UserId, friendId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

    }
}
