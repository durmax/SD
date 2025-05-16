using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sd.Shared;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using sd.Api.Application.Services;


namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWordService _wordService;
        private readonly IRelationshipService _relationshipService;

        public WordController(IUserService userService, IWordService wordService, IRelationshipService relationshipService)
        {
            _userService = userService;
            _wordService = wordService;
            _relationshipService = relationshipService;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<WordDto>> GetWord(string id)
        {
            var currentUserId = (await _userService.GetCurrentUser(User))?.UserId;
            var wordDto = await _wordService.GetWordDtoById(id, currentUserId);

            if (wordDto == null) return NotFound();

            return Ok(wordDto);
        }

        [HttpGet("GetWordByText/{userId}/{title}")]
        public async Task<ActionResult<WordDto>> GetWord(string userId, string title)
        {
            var result = await _wordService.GetWordByText(userId, title);

            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("GetWordsContainText/{title}")]
        public async Task<ActionResult<IEnumerable<string>>> GetWordsContainText(string title)
        {
            var ws = await _wordService.GetWordsContainText((await _userService.GetCurrentUser(User))?.UserId, title);
            return Ok(ws);
        }

        [AllowAnonymous]
        [HttpGet("GetPageWords/{userId}/{pageSize}/{currentPage}")]
        public async Task<ActionResult<List<WordDto>>> GetPageWords(string userId, int pageSize, int currentPage)
        {
            var res = await _wordService.GetPageWords((await _userService.GetCurrentUser(User))?.UserId, userId, null, pageSize, currentPage);
            return Ok(res);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<WordDto>> Create(WordDto word)
        {
            if (string.IsNullOrWhiteSpace(word?.Title))
                return BadRequest();

            word.UserId = (await _userService.GetCurrentUser(User))?.UserId;

            var wordMs = await _wordService.GetByCondation(w => w.WordId == word.WordId);
            var wordM = wordMs.FirstOrDefault();

            if (Guid.TryParse(word?.WordId, out Guid result) && wordM != null)
            {
                if (wordM.UserId != word.UserId) return StatusCode(StatusCodes.Status401Unauthorized);

                wordM.Title = word.Title;
                wordM.Explain = word.Explain;
                wordM.ShareWith = (int)word.ShareWith;
                wordM.WordLang = word.WordLang;
                wordM.ToLang = word.ToLang;
                wordM.Score = word.Score;

                var res = await _wordService.Update(wordM);
                return Accepted(word);
            }

            var wordToInsert = await _wordService.GetWordByText(word.UserId, word.Title);

            if (wordToInsert != null)
            {
                return StatusCode(StatusCodes.Status302Found, wordToInsert);
            }

            word.WordId = await _wordService.AddWord(word);

            int statusCode = !string.IsNullOrWhiteSpace(word.WordId) ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

            return StatusCode(statusCode, word);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> UpdateWord(WordModel updatedWord)
        {
            var currentUser = (await _userService.GetCurrentUser(User))?.UserId;
            if (updatedWord.UserId != currentUser) return StatusCode(StatusCodes.Status401Unauthorized);

            int statusCode = await _wordService.Update(updatedWord) ? StatusCodes.Status204NoContent : StatusCodes.Status500InternalServerError;
            return StatusCode(statusCode);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWord(string id)
        {
            var currentUser = (await _userService.GetCurrentUser(User))?.UserId;
            var ws = await _wordService.GetByCondation(w => w.WordId == id);
            var w = ws.First();

            if (w?.UserId != currentUser) return StatusCode(StatusCodes.Status401Unauthorized);

            await _wordService.Delete(id);
            return StatusCode(StatusCodes.Status200OK);
        }

        [Authorize]
        [HttpGet("Like/{wordId}")]
        public async Task<ActionResult<int>> Like(string wordId)
        {
            return await _wordService.Like((await _userService.GetCurrentUser(User))?.UserId, wordId);
        }

        [AllowAnonymous]
        [HttpGet("GetLikedUsers/{wordId}")]
        public async Task<ActionResult<IEnumerable<UserRelationshipsWithOneUserDto>>> GetLikes(string wordId)
        {
            var ws = await _wordService.GetByCondation(w => w.WordId == wordId);
            var word = ws.First();

            var foundUsers = await _userService.GetByCondation(u => word.Likes.Contains(u.UserId));
            var result = await _relationshipService.GetRelationships((await _userService.GetCurrentUser(User))?.UserId, foundUsers.ToList());

            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}