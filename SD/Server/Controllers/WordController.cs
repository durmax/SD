using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SD.Shared;
using Microsoft.AspNetCore.Authorization;
using sd.Api.Infrastructure.Repositories;
using sd.Api.Repositories;


namespace sd.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IWordRepository _wordRepo;
        private readonly IRelationshipRepository _relationshipRepo;

        public WordController(IUserRepository userRepo,
        IWordRepository wordRepo, IRelationshipRepository relationshipRepo)
        {
            _userRepo = userRepo;
            _wordRepo = wordRepo;
            _relationshipRepo = relationshipRepo;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<WordDto>> GetWord(string id)
        {
            var currentUserId = (await _userRepo.GetCurrentUser(User))?.UserId;
            var wordDto = await _wordRepo.GetWordDtoById(id, currentUserId);

            if (wordDto == null) return NotFound();

            return Ok(wordDto);
        }

        [HttpGet("GetWordByText/{userId}/{title}")]
        public async Task<ActionResult<WordDto>> GetWord(string userId, string title)
        {
            var result = await _wordRepo.GetWordByText(userId, title);

            if (result == null) return NotFound();

            return result;
        }

        [HttpGet("GetWordsContainText/{title}")]
        public async Task<ActionResult<IEnumerable<string>>> GetWordsContainText(string title)
        {
            var ws = await _wordRepo.GetWordsContainText((await _userRepo.GetCurrentUser(User))?.UserId, title);
            return Ok(ws);
        }

        [AllowAnonymous]
        [HttpGet("GetPageWords/{userId}/{pageSize}/{currentPage}")]
        public async Task<ActionResult<List<WordDto>>> GetPageWords(string userId, int pageSize, int currentPage)
        {

            var res = await _wordRepo.GetPageWords((await _userRepo.GetCurrentUser(User))?.UserId, userId, null, pageSize, currentPage);
            return Ok(res);

        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<WordDto>> Create(WordDto word)
        {

            if (string.IsNullOrWhiteSpace(word?.Title))
                return BadRequest();

            word.UserId = (await _userRepo.GetCurrentUser(User))?.UserId;

            if (Guid.TryParse(word?.WordId, out Guid result) && await _wordRepo.GetWordById(word.WordId) != null)
            {
                await _wordRepo.UpdateWord(word);
                return StatusCode(StatusCodes.Status202Accepted,
                   "Updated");
            }

            var wordToInsert = await _wordRepo.GetWordByText(word.UserId, word.Title);

            if (wordToInsert != null)
            {
                return StatusCode(StatusCodes.Status302Found, wordToInsert);
            }

            word.WordId = await _wordRepo.AddWord(word);

            int statusCode = !string.IsNullOrWhiteSpace(word.WordId) ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

            return StatusCode(statusCode, word);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> UpdateWord(WordDto updatedWord)
        {
            var currentUser = (await _userRepo.GetCurrentUser(User))?.UserId;
            if (updatedWord.UserId != currentUser) return StatusCode(StatusCodes.Status401Unauthorized);

            int statusCode = await _wordRepo.UpdateWord(updatedWord) ? StatusCodes.Status204NoContent : StatusCodes.Status500InternalServerError;
            return StatusCode(statusCode);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWord(string id)
        {
            var currentUser = (await _userRepo.GetCurrentUser(User))?.UserId;
            var w = await _wordRepo.GetWordById(id);
            if (w?.UserId != currentUser) return StatusCode(StatusCodes.Status401Unauthorized);

            await _wordRepo.Delete(id);
            return StatusCode(StatusCodes.Status200OK);
        }

        [Authorize]
        [HttpGet("Like/{wordId}")]
        public async Task<ActionResult<int>> Like(string wordId)
        {
            return await _wordRepo.Like((await _userRepo.GetCurrentUser(User))?.UserId, wordId);
        }

        [AllowAnonymous]
        [HttpGet("GetLikedUsers/{wordId}")]
        public async Task<ActionResult<IEnumerable<UserRelationshipsWithOneUserDto>>> GetLikes(string wordId)
        {
            var word = await _wordRepo.GetWordById(wordId);

            var foundUsers = await _userRepo.GetUsers(word.Likes);
            var result = await _relationshipRepo.GetRelationships((await _userRepo.GetCurrentUser(User))?.UserId, foundUsers);

            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}