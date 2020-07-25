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
    public class WordController : ControllerBase
    {
        private readonly IWordRepository _wordService;

        public WordController(IWordRepository wordService)
        {
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

        // GET: api/Word/GetAllWords/1111/5e915b3a1c9d4400003f1fba
        [HttpGet("GetAllWords/{CurrentUserId}/{userId}")]
        public async Task<ActionResult<IEnumerable<WordModel>>> GetAllWords(string CurrentUserId, string userId)
        {
            try
            {
                return Ok(await _wordService.GetAllWords(CurrentUserId, userId));
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
                return Ok(await _wordService.Like(userId, wordId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }
    }
}
