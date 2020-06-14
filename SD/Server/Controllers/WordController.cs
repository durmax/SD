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

        // GET: api/Word/GetAllWords/5e915b3a1c9d4400003f1fba
        [HttpGet("GetAllWords/{userId}")]
        public async Task<ActionResult<IEnumerable<WordModel>>> GetAllWords(string userId)
        {
            try
            {
                return Ok(await _wordService.GetAllWords(userId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ex.Message);
            }
        }

        [HttpPost]
        [Route("AddWord")]
        public async Task<ActionResult<WordModel>> Create(WordModel word)
        {
            try
            {
                if (word == null)
                    return BadRequest();

                // Add custom model validation error

                var wordToInsert = _wordService.GetWordByText(word.UserId, word.Title);

                if (wordToInsert.Result != null)
                {
                    ModelState.AddModelError("word", "You have this Word is already");
                    return BadRequest(ModelState);
                }

                var createdWord = await _wordService.AddWord(word);

                return CreatedAtAction(nameof(GetWord),
                    new { id = createdWord }, createdWord);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new word record");
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<WordModel>> UpdateWord(string id, WordModel updatedWord)
        {
            try
            {
                var wordToUpdate = await _wordService.GetWordById(id);

                if (wordToUpdate == null)
                    return NotFound($"Word with Id = {id} not found");

                return await _wordService.UpdateWord(id, updatedWord);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IEnumerable<WordModel>>> DeleteWord(string id, string userId)
        {
            try
            {
                var wordToDelete = await _wordService.GetWordById(id);

                if (wordToDelete == null)
                {
                    return NotFound($"Word with Id = {id} not found");
                }

                await _wordService.RemoveWord(id, userId);
                return Ok(await GetAllWords(userId));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
