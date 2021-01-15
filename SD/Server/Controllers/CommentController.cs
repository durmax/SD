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
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentService;
        private readonly IWordRepository _wordService;

        public CommentController(ICommentRepository commentService, IWordRepository wordService)
        {
            _commentService = commentService;
            _wordService = wordService;
        }

        [HttpPost]
        [Route("SaveComment/{WordId}")]
        public async Task<ActionResult> SaveComment(string wordId, CommentModel comment)
        {
            if (await _commentService.SaveComment(wordId, comment))
                return StatusCode(StatusCodes.Status200OK);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error to save comment");
        }

        [HttpGet("LikeComment/{userId}/{WordId}/{commentId}")]
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

        [HttpDelete("DeleteComment/{wordId}/{commentId}")]
        public async Task<ActionResult> DeleteComment(string wordId, string commentId)
        {
            try
            {
                await _commentService.RemoveComment(wordId, commentId);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting comment");
            }
        }

    }
}
