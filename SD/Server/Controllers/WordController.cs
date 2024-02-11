using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Interfaces;
using SD.Shared;
using sd.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILikeWordService _likeWord;
        private readonly WordService _wordService;
        private readonly RelationshipService _relationshipService;

        public WordController(UserService userService, ILikeWordService likeWord,
        WordService wordService, RelationshipService relationshipService)
        {
            _userService = userService;
            _likeWord = likeWord;
            _wordService = wordService;
            _relationshipService = relationshipService;
        }

        // GET: api/Word/GetWord/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WordDto>> GetWord(string id)
        {
            try
            {
                var result = await _wordService.GetWordDtoById(id);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpGet("GetWordByText/{userId}/{title}")]
        public async Task<ActionResult<WordDto>> GetWord(string userId, string title)
        {
            try
            {
                var result = await _wordService.GetWordByText(userId, title);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpGet("GetWordsContainText/{userId}/{title}")]
        public async Task<ActionResult<IEnumerable<string>>> GetWordsContainText(string userId, string title)
        {
            try
            {
                var ws = await _wordService.GetWordsContainText(userId, title);
                return Ok(ws);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("GetPageWords/{CurrentUserId}/{userId}/{pageSize}/{currentPage}")]
        public async Task<ActionResult<List<WordDto>>> GetPageWords(string CurrentUserId, string userId, int pageSize, int currentPage)
        {
            try
            {
                var res= await _wordService.GetPageWords(CurrentUserId, userId, null, pageSize, currentPage);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<string>> Create(WordDto word)
        {
            try
            {
                if (word == null)
                    return BadRequest();
                if (string.IsNullOrWhiteSpace(word.UserId) || string.IsNullOrWhiteSpace(word.Title))
                    return BadRequest();
                if (!string.IsNullOrWhiteSpace(word.WordId) && await _wordService.GetWordDtoById(word.WordId) != null)
                {
                    await _wordService.UpdateWord(word);
                    return StatusCode(StatusCodes.Status202Accepted,
                       "Updated");
                }

                var wordToInsert = await _wordService.GetWordByText(word.UserId, word.Title);

                if (wordToInsert != null)
                {
                    return StatusCode(StatusCodes.Status302Found,
                       $"{wordToInsert?.WordId}");    // returen word id that found
                }
                word.WordId = await _wordService.AddWord(word);
                int statusCode = !string.IsNullOrWhiteSpace(word.WordId) ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

                return StatusCode(statusCode, word.WordId);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error in save {word.Title}");
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> UpdateWord(WordDto updatedWord)
        {
            try
            {
                int statusCode = await _wordService.UpdateWord(updatedWord) ? StatusCodes.Status204NoContent : StatusCodes.Status500InternalServerError;
                return StatusCode(statusCode);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWord(string id)
        {
            try
            {
                await _wordService.RemoveWord(id);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

        [Authorize]
        [HttpGet("Like/{userId}/{wordId}")]
        public async Task<ActionResult<int>> Like(string userId, string wordId)
        {
            try
            {
                return await _likeWord.Like(userId, wordId);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpGet("GetLikedUsers/{CurrentUserId}/{wordId}")]
        public async Task<ActionResult<IEnumerable<UserRelationshipsWithOneUserDto>>> GetLikes(string CurrentUserId, string wordId)
        {
            try
            {
                var word = await _wordService.GetWordById(wordId);

                var foundUsers = await _userService.GetUsers(word.Likes);
                var result = await _relationshipService.GetRelationships(CurrentUserId, foundUsers);

                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }
    }
}