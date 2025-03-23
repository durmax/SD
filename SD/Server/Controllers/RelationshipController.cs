using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SD.Shared;
using Microsoft.AspNetCore.Authorization;
using sd.Api.Repositories;
using sd.Api.Infrastructure.Repositories;
using System.Linq;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RelationshipController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IRelationshipRepository _relationshipRepo;

        public RelationshipController(RelationshipRepository relationshipRepo, IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _relationshipRepo = relationshipRepo;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RelationshipModel>> GetRelationshipById(string id)
        {
            var rs = await _relationshipRepo.GetByCondation(r => r.RelationshipId == id);
            var result = rs?.FirstOrDefault();

            if (result == null) return NotFound();

            return result;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<bool>> AddRelationship(RelationshipModel relationship)
        {
            relationship.UserId1 = (await _userRepo.GetCurrentUser(User))?.UserId;
            return await _relationshipRepo.AddRelationship(relationship);
        }

        [HttpPost("{oldRelationshipId}")]
        public async Task<ActionResult<bool>> UpdatRelationship(string oldRelationshipId, RelationshipModel newRelationship)
        {
            return await _relationshipRepo.Update(newRelationship);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveRelationship(string id)
        {
            await _relationshipRepo.Delete(id);
            return StatusCode(StatusCodes.Status200OK);
        }

        [Authorize]
        [HttpDelete("{UserId}/{reletion}/{friendId}")]
        public async Task<ActionResult> RemoveFriendship(Relation reletion, string friendId)
        {
            var rId = await _relationshipRepo.GetRelationshipId((await _userRepo.GetCurrentUser(User))?.UserId, reletion, friendId);
            await _relationshipRepo.Delete(rId);
            return StatusCode(StatusCodes.Status200OK);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetAllFriends()
        {
            var result = await _relationshipRepo.GetAllFriends((await _userRepo.GetCurrentUser(User))?.UserId);

            if (result == null) return NotFound();

            return result;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, string>>> GetFriendRequests()
        {
            var result = await _relationshipRepo.FriendRequestsToUser((await _userRepo.GetCurrentUser(User))?.UserId);

            if (result == null) return NotFound();

            return result;
        }
    }
}