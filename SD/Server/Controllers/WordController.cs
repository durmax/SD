using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SD.Shared;
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
        private readonly IUserService userService;
        private readonly IWordService wordService;
        private readonly IRelationshipService relationshipService;

        public WordController(IUserService userService, IWordService wordService, IRelationshipService relationshipService)
        {
            this.userService = userService;
            this.wordService = wordService;
            this.relationshipService = relationshipService;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<WordDto>> GetWord(string id)
        {
            var currentUserId = (await userService.GetCurrentUser(User))?.UserId;
            var wordDto = await wordService.GetWordDtoById(id, currentUserId);

            if (wordDto == null) return NotFound();

            return Ok(wordDto);
        }

        [HttpGet("GetWordByText/{userId}/{title}")]
        public async Task<ActionResult<WordDto>> GetWord(string userId, string title)
        {
            var result = await wordService.GetWordByText(userId, title);

            if (result == null) return NotFound();

            return result;
        }

        [HttpGet("GetWordsContainText/{title}")]
        public async Task<ActionResult<IEnumerable<string>>> GetWordsContainText(string title)
        {
            var ws = await wordService.GetWordsContainText((await userService.GetCurrentUser(User))?.UserId, title);
            return Ok(ws);
        }

        [AllowAnonymous]
        [HttpGet("GetPageWords/{userId}/{pageSize}/{currentPage}")]
        public async Task<ActionResult<List<WordDto>>> GetPageWords(string userId, int pageSize, int currentPage)
        {
            var res = await wordService.GetPageWords((await userService.GetCurrentUser(User))?.UserId, userId, null, pageSize, currentPage);
            return Ok(res);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<WordDto>> Create(WordDto word)
        {

            if (string.IsNullOrWhiteSpace(word?.Title))
                return BadRequest();

            word.UserId = (await userService.GetCurrentUser(User))?.UserId;

            var wordMs = await wordService.GetByCondation(w => w.WordId == word.WordId);
            var wordM = wordMs.FirstOrDefault();

            if (Guid.TryParse(word?.WordId, out Guid result) && wordM != null)
            {
                await wordService.UpdateWord(word);
                return StatusCode(StatusCodes.Status202Accepted,
                   "Updated");
            }

            var wordToInsert = await wordService.GetWordByText(word.UserId, word.Title);

            if (wordToInsert != null)
            {
                return StatusCode(StatusCodes.Status302Found, wordToInsert);
            }

            word.WordId = await wordService.AddWord(word);

            int statusCode = !string.IsNullOrWhiteSpace(word.WordId) ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

            return StatusCode(statusCode, word);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> UpdateWord(WordDto updatedWord)
        {
            var currentUser = (await userService.GetCurrentUser(User))?.UserId;
            if (updatedWord.UserId != currentUser) return StatusCode(StatusCodes.Status401Unauthorized);

            int statusCode = await wordService.UpdateWord(updatedWord) ? StatusCodes.Status204NoContent : StatusCodes.Status500InternalServerError;
            return StatusCode(statusCode);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWord(string id)
        {
            var currentUser = (await userService.GetCurrentUser(User))?.UserId;
            var ws = await wordService.GetByCondation(w => w.WordId == id);
            var w = ws.First();

            if (w?.UserId != currentUser) return StatusCode(StatusCodes.Status401Unauthorized);

            await wordService.Delete(id);
            return StatusCode(StatusCodes.Status200OK);
        }

        [Authorize]
        [HttpGet("Like/{wordId}")]
        public async Task<ActionResult<int>> Like(string wordId)
        {
            return await wordService.Like((await userService.GetCurrentUser(User))?.UserId, wordId);
        }

        [AllowAnonymous]
        [HttpGet("GetLikedUsers/{wordId}")]
        public async Task<ActionResult<IEnumerable<UserRelationshipsWithOneUserDto>>> GetLikes(string wordId)
        {
            var ws = await wordService.GetByCondation(w => w.WordId == wordId);
            var word = ws.First();

            var foundUsers = await userService.GetByCondation(u => word.Likes.Contains(u.UserId));
            var result = await relationshipService.GetRelationships((await userService.GetCurrentUser(User))?.UserId, foundUsers.ToList());

            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}