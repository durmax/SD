using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Interfaces;
using SD.Shared;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{wordId}")]
        public async Task<ActionResult<OtherPageResModel>> GetWordComments(string wordId)
        {
            try
            {
                return Ok(await _commentService.GetWordComments(wordId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpPost]
        [Route("{wordId}")]
        public async Task<ActionResult> SaveComment(string wordId, CommentModel comment)
        {
            if (await _commentService.SaveComment(wordId, comment))
                return StatusCode(StatusCodes.Status200OK);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error to save comment");
        }

        [HttpGet("{userId}/{WordId}/{commentId}")]
        public async Task<ActionResult<int>> LikeComment(string userId, string wordId, string commentId)
        {
            try
            {
                return Ok(await _commentService.Like(userId, wordId, commentId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }
        [HttpDelete("{currUsr}/{wordId}/{commentId}")]
        public async Task<ActionResult<bool>> DeleteComment(string currUsr, string wordId, string commentId)
        {
            try
            {
                return Ok(await _commentService.Delete( currUsr,  wordId,  commentId));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
