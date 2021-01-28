using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Api.Interfaces;
using SD.Shared;
using sd.Api.Services;

namespace sd.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {
       // private readonly IWordRepository _wordRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILikeWordService _likeWord;
        private readonly WordService _wordService;

        public WordController(IUserRepository userService, ILikeWordService likeWord, WordService wordService)
        {
            //_wordRepository = wordRepository;
            _userRepository = userService;
            _likeWord = likeWord;
            _wordService = wordService;
        }

        // GET: api/Word/GetWord/5
        [HttpGet("GetWord/{id}")]
        public async Task<ActionResult<WordModel>> GetWord(string id)
        {
            try
            {
                var result = await _wordService.GetWordById(id);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        // GET: api/Word/GetWord/5
        [HttpGet("GetWordByText/{userId}/{title}")]
        public async Task<ActionResult<WordModel>> GetWord(string userId, string title)
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

        [HttpGet("GetPageWordsFromUserID/{CurrentUserId}/{userId}/{pageSize}/{currentPage}")]
        public async Task<ActionResult<Tuple<int, List<WordModel>>>> GetPageWordsFromUserID(string CurrentUserId, string userId, int pageSize, int currentPage)
        {
            try
            {
                //return Ok(await _wordRepository.GetAllWords(CurrentUserId, userId, pageSize, currentPage));
                return Ok(await _wordService.GetPageWords(CurrentUserId, userId,null, pageSize, currentPage));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }


        [HttpGet("GetPageWordsFromAllUseres/{CurrentUserId}/{pageSize}/{currentPage}")]
        public async Task<ActionResult<Tuple<int, List<WordModel>>>> GetPageWordsFromAllUseres(string CurrentUserId, int pageSize, int currentPage)
        {
            try
            {
                //return Ok(await _wordRepository.GetAllWords(CurrentUserId, userId, pageSize, currentPage));
                return Ok(await _wordService.GetPageWords(CurrentUserId, null, null, pageSize, currentPage));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpPost]
        [Route("AddWord")]
        public async Task<ActionResult<string>> Create(WordModel word)
        {
            try
            {
                if (word == null)
                    return BadRequest();
                if (string.IsNullOrWhiteSpace(word.UserId) || string.IsNullOrWhiteSpace(word.WordId) || string.IsNullOrWhiteSpace(word.Title))
                    return BadRequest();

                if (await _wordService.GetWordById(word.WordId) != null)
                {
                    await _wordService.UpdateWord(word.WordId, word);
                    return StatusCode(StatusCodes.Status202Accepted,
                       "Updated");
                }

                var wordToInsert = await _wordService.GetWordByText(word.UserId, word.Title);

                if (wordToInsert != null)
                {
                    return StatusCode(StatusCodes.Status302Found,
                       $"{wordToInsert?.WordId}");    // returen word id that found
                }

                int statusCode = await _wordService.AddWord(word) ? 200 : 500;

                return StatusCode(statusCode);

                //return CreatedAtAction(nameof(GetWord),
                //    new { id = createdWord }, createdWord);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error to save new {word.Title}");
            }
        }

        [HttpPut]
        [Route("UpdateWord")]
        public async Task<ActionResult> UpdateWord(WordModel updatedWord)
        {
            string id = updatedWord.WordId;
            try
            {
                var wordToUpdate = await _wordService.GetWordById(id);

                if (wordToUpdate == null)
                    return StatusCode(StatusCodes.Status404NotFound,
                      $"{updatedWord.Title} is not found");

                int statusCode = await _wordService.UpdateWord(id, updatedWord) ? 200 : 500;

                return StatusCode(statusCode);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpDelete("DeleteWord/{id}")]
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

        [HttpGet("Like/{userId}/{wordId}")]
        public async Task<ActionResult<int>> Like(string userId, string wordId)
        {
            try
            {
                    return await _likeWord.Like(userId,wordId);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpGet("GetLikedUsers/{CurrentUserId}/{wordId}")]
        public async Task<ActionResult<Dictionary<string, Tuple<string, string>>>> GetLikes(string CurrentUserId, string wordId)
        {
            try
            {
                WordModel word = await _wordService.GetWordById(wordId);
                var result = await _userRepository.GetUsersWithRelationship(CurrentUserId, word.Likes);
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
