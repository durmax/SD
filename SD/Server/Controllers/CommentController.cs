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

        public CommentController(ICommentRepository commentService)
        {
            _commentService = commentService;
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
    }
}
