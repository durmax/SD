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

        [HttpGet("GetComment/{CommentId}")]
        public async Task<ActionResult<CommentModel>> GetComment(string CommentId)
        {
            try
            {
                return Ok(await _commentService.GetComment(CommentId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpGet("GetAllComments/{WordId}")]
        public async Task<ActionResult<IEnumerable<CommentModel>>> GetAllComments(string WordId)
        {
            try
            {
                return Ok(await _commentService.GetAllComments(WordId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpPost]
        [Route("AddComment/{WordId}")]
        public async Task<ActionResult> AddComment(CommentModel comment, string WordId)
        {
            try
            {
                int statusCode = await _commentService.AddComment(comment) ? 200 : 500;
                if (statusCode ==200)
                {
                 WordModel wordModel=   await _wordService.GetWordById(WordId);

                    if (wordModel.Comments == null)
                    {
                        wordModel.Comments = new List<string>();
                    }
                    wordModel.Comments.Add(comment.CommentId);
                    await _wordService.UpdateWord(WordId, wordModel);
                }
                return StatusCode(statusCode);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpPut]
        [Route("UpdateComment")]
        public async Task<ActionResult> UpdateComment(CommentModel comment)
        {
            try
            {
                int statusCode = await _commentService.UpdateComment(comment) ? 200 : 500;
                return StatusCode(statusCode);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpGet("LikeComment/{userId}/{commentId}")]
        public async Task<ActionResult<int>> LikeComment(string userId, string commentId)
        {
            try
            {
                return Ok(await _commentService.Like(userId, commentId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpDelete("DeleteComment/{id}")]
        public async Task<ActionResult> DeleteComment(string id)
        {
            try
            {
                await _commentService.RemoveComment(id);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

    }
}
