using Microsoft.AspNetCore.Mvc;
using Qolab.API.Managers;
using Qolab.API.Models;
using static Qolab.API.Models.Enums;

namespace Qolab.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ArticlesManager _manager;

        public ArticlesController(ArticlesManager manager)
        {
            _manager = manager;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDto>> GetArticle([FromRoute] Guid id)
        {
            var article = await _manager.GetArticleAsync(id);

            if (article is null)
            {
                return NotFound();
            }

            return article;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleShortDto>>> SearchArticles([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest();
            }

            var result = await _manager.SearchArticlesAsync(searchTerm);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] ArticleDto article)
        {
            var result = await _manager.CreateArticleAsync(article);
            return CreatedAtAction(nameof(GetArticle), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle([FromRoute] Guid id, [FromBody] ArticleDto article)
        {
            if (id != article.Id)
            {
                return BadRequest();
            }

            // TODO: Add article owner validation

            var result = await _manager.UpdateArticleAsync(article);
            return result is null ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle([FromRoute] Guid id)
        {
            // TODO: Add article owner validation
            var result = await _manager.DeleteArticleAsync(id);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/up-vote")]
        public async Task<IActionResult> UpVoteArticle([FromRoute] Guid id)
        {
            var result = await _manager.VoteArticleAsync(id, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/down-vote")]
        public async Task<IActionResult> DownVoteArticle([FromRoute] Guid id)
        {
            var result = await _manager.VoteArticleAsync(id, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/comments")]
        public async Task<ActionResult<ArticleDto>> AddArticleComment([FromRoute] Guid id, [FromBody] CommentDto comment)
        {
            // TODO: Add article owner validation

            var result = await _manager.AddArticleCommentAsync(id, comment);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
