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

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUser(string id)
        {
            try
            {
                var result = await _userService.GetUser(id);
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
                UserModel userToDelete = await _userService.GetUser(id);

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

    }
}
