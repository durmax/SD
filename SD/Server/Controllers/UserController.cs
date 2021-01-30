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
    public class UserController : ControllerBase
    {
        private readonly UserService _userRepository;
        private readonly RelationshipService _relationshipService;

        public UserController(UserService userService, RelationshipService relationshipService) 
        { 
            _userRepository = userService;
            _relationshipService = relationshipService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> Get()
        {
            try
            {
                return Ok(await _userRepository.GetAllUsers());
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
            List<UserModel> foundUsers;  
            try
            {
                foundUsers = await _userRepository.SearchUser(CurrentUserId, searchText);

                 var result = await _relationshipService.GetRelationships(CurrentUserId, foundUsers);
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
                var result = await _userRepository.GetUserById(id);
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

                TransObj status = await _userRepository.RegisterUserAsync(user);
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
                return Ok(await _userRepository.UpdateUser(id, updatedUser));
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
                UserModel userToDelete = await _userRepository.GetUserById(id);

                if (userToDelete == null)
                {
                    return NotFound($"User with Id = {id} not found");
                }

                return Ok(await _userRepository.RemoveUser(id));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
