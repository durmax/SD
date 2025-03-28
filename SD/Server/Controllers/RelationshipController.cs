using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using sd.Api.Application.Services;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RelationshipController : ControllerBase
    {
        private readonly IRelationshipService _relationshipService;
        private readonly IUserService _userService;

        public RelationshipController(IRelationshipService relationshipService, IUserService userService)
        {
            _relationshipService = relationshipService;
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RelationshipModel>> GetRelationshipById(string id)
        {
            var rs = await _relationshipService.GetByCondation(r => r.RelationshipId == id);
            var result = rs?.FirstOrDefault();

            if (result == null) return NotFound();

            return result;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<bool>> AddRelationship(RelationshipModel relationship)
        {
            relationship.UserId1 = (await _userService.GetCurrentUser(User))?.UserId;
            return await _relationshipService.AddRelationship(relationship);
        }

        [HttpPost("{oldRelationshipId}")]
        public async Task<ActionResult<bool>> UpdatRelationship(string oldRelationshipId, RelationshipModel newRelationship)
        {
            return await _relationshipService.Update(newRelationship);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveRelationship(string id)
        {
            await _relationshipService.Delete(id);
            return StatusCode(StatusCodes.Status200OK);
        }

        [Authorize]
        [HttpDelete("{UserId}/{reletion}/{friendId}")]
        public async Task<ActionResult> RemoveFriendship(Relation reletion, string friendId)
        {
            var r = await _relationshipService.GetRelationship((await _userService.GetCurrentUser(User))?.UserId, reletion, friendId);
            await _relationshipService.Delete(r.RelationshipId);
            return StatusCode(StatusCodes.Status200OK);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetAllFriends()
        {
            var result = await _relationshipService.GetAllFriends((await _userService.GetCurrentUser(User))?.UserId);

            if (result == null) return NotFound();

            return result;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetFriendRequests()
        {
            var result = await _relationshipService.FriendRequestsToUser((await _userService.GetCurrentUser(User))?.UserId);

            if (result == null) return NotFound();

            return result;
        }
    }
}