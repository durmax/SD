using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sd.Api.Services;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationshipController : ControllerBase
    {
        private readonly RelationshipService _relationshipService;
        //private readonly UserService _userService;

        public RelationshipController(RelationshipService relationshipService)
        {
            _relationshipService = relationshipService;
            //_userService = userService;
        }

        [HttpGet("GetRelationshipById/{id}")]
        public async Task<ActionResult<RelationshipModel>> GetRelationshipById(string id)
        {
            try
            {
                var result = await _relationshipService.GetRelationshipById(id);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpPost("AddRelationship")]
        public async Task<ActionResult<bool>> AddRelationship(RelationshipModel relationship)
        {
            try
            {
                return await _relationshipService.AddRelationship(relationship);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpPost("UpdatRelationship/{oldRelationshipId}")]
        public async Task<ActionResult<bool>> UpdatRelationship(string oldRelationshipId, RelationshipModel newRelationship)
        {
            try
            {
                return await _relationshipService.UpdatRelationship(oldRelationshipId, newRelationship);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpDelete("RemoveRelationship/{id}")]
        public async Task<ActionResult> RemoveRelationship(string id)
        {
            try
            {
                await _relationshipService.RemoveRelationship(id);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

        [HttpDelete("RemoveFriendship/{UserId}/{reletion}/{friendId}")]
        public async Task<ActionResult> RemoveFriendship(string userId, Reletion reletion, string friendId)
        {
            try
            {
                var rId = await _relationshipService.GetRelationshipId(userId, reletion, friendId);
                await _relationshipService.RemoveRelationship(rId);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }


        [HttpGet("GetAllFriends/{userId}")]
        public async Task<ActionResult<Dictionary<string, string>>> GetAllFriends(string userId)
        {
            try
            {
                var result = await _relationshipService.GetAllFriends(userId);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpGet("GetFriendRequestsById/{id}")]
        public async Task<ActionResult<Dictionary<string, string>>> GetFriendRequestsById(string id)
        {
            try
            {
                var result = await _relationshipService.FrindRequestsToUser(id);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }
    }
}
