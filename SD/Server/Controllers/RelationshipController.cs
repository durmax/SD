using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using sd.Api.Services;
using SD.Shared;
using Microsoft.AspNetCore.Authorization;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RelationshipController : ControllerBase
    {
        private readonly CurrUsrService _currUsrService;
        private readonly RelationshipService _relationshipService;

        public RelationshipController(RelationshipService relationshipService, CurrUsrService currUsrService)
        {
            _currUsrService = currUsrService;
            _relationshipService = relationshipService;
        }

        [HttpGet("{id}")]
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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<bool>> AddRelationship(RelationshipModel relationship)
        {
            try
            {
                relationship.UserId1 = (await _currUsrService.GetCurrentUser(User))?.UserId;
                return await _relationshipService.AddRelationship(relationship);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpPost("{oldRelationshipId}")]
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

        [HttpDelete("{id}")]
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

        [Authorize]
        [HttpDelete("{UserId}/{reletion}/{friendId}")]
        public async Task<ActionResult> RemoveFriendship(Relation reletion, string friendId)
        {
            try
            {
                var rId = await _relationshipService.GetRelationshipId((await _currUsrService.GetCurrentUser(User))?.UserId, reletion, friendId);
                await _relationshipService.RemoveRelationship(rId);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetAllFriends()
        {
            try
            {
                var result = await _relationshipService.GetAllFriends((await _currUsrService.GetCurrentUser(User))?.UserId);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetFriendRequests()
        {
            try
            {
                var result = await _relationshipService.FriendRequestsToUser((await _currUsrService.GetCurrentUser(User))?.UserId);

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
