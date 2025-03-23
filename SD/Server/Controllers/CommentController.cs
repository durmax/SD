using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Infrastructure.Repositories;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IWordRepository _wordRepo;

        public CommentController(IUserRepository userRepo, IWordRepository wordRepo)
        {
            _userRepo = userRepo;
            _wordRepo = wordRepo;
        }

        [HttpGet("{wordId}")]
        public async Task<ActionResult<OtherPageResModel>> GetWordComments(string wordId)
        {
            return Ok(await _wordRepo.GetWordComments(wordId));
        }

        [Authorize]
        [HttpPost]
        [Route("{wordId}")]
        public async Task<ActionResult> SaveComment(string wordId, CommentModel comment)
        {
            if (string.IsNullOrEmpty(comment?.UserId)) comment.UserId = (await _userRepo.GetCurrentUser(User))?.UserId;
            if (string.IsNullOrEmpty(comment?.CommentOwnerName)) comment.CommentOwnerName = (await _userRepo.GetCurrentUser(User))?.Name;

            if (await _wordRepo.SaveComment(wordId, comment))
                return StatusCode(StatusCodes.Status200OK);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error to save comment");
        }

        [Authorize]
        [HttpGet("{WordId}/{commentId}")]
        public async Task<ActionResult<int>> LikeComment(string wordId, string commentId)
        {
            return Ok(await _wordRepo.LikeComment((await _userRepo.GetCurrentUser(User))?.UserId, wordId, commentId));
        }

        [Authorize]
        [HttpDelete("{wordId}/{commentId}")]
        public async Task<ActionResult<bool>> DeleteComment(string wordId, string commentId)
        {
            return Ok(await _wordRepo.DeleteComment((await _userRepo.GetCurrentUser(User))?.UserId, wordId, commentId));
        }
    }
}
