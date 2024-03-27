using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Interfaces;
using sd.Api.Services;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        private readonly CurrUsrService _currUsrService;

        public CommentController(ICommentService commentService, CurrUsrService currUsrService)
        {
            _currUsrService = currUsrService;
            _commentService = commentService;
        }

        [HttpGet("{wordId}")]
        public async Task<ActionResult<OtherPageResModel>> GetWordComments(string wordId)
        {
            return Ok(await _commentService.GetWordComments(wordId));
        }

        [Authorize]
        [HttpPost]
        [Route("{wordId}")]
        public async Task<ActionResult> SaveComment(string wordId, CommentModel comment)
        {
            if (string.IsNullOrEmpty(comment?.UserId)) comment.UserId = (await _currUsrService.GetCurrentUser(User))?.UserId;
            if (string.IsNullOrEmpty(comment?.CommentOwnerName)) comment.CommentOwnerName = (await _currUsrService.GetCurrentUser(User))?.Name;

            if (await _commentService.SaveComment(wordId, comment))
                return StatusCode(StatusCodes.Status200OK);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error to save comment");
        }

        [Authorize]
        [HttpGet("{WordId}/{commentId}")]
        public async Task<ActionResult<int>> LikeComment(string wordId, string commentId)
        {
            return Ok(await _commentService.Like((await _currUsrService.GetCurrentUser(User))?.UserId, wordId, commentId));
        }

        [Authorize]
        [HttpDelete("{wordId}/{commentId}")]
        public async Task<ActionResult<bool>> DeleteComment(string wordId, string commentId)
        {
            return Ok(await _commentService.Delete((await _currUsrService.GetCurrentUser(User))?.UserId, wordId, commentId));
        }
    }
}
