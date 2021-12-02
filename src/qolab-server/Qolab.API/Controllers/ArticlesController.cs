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

            // TODO: Add validation, only author can modify articles
            var result = await _manager.UpdateArticleAsync(article);
            return result is null ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle([FromRoute] Guid id)
        {
            // TODO: Add validation, only author can delete articles
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
            var result = await _manager.AddCommentAsync(id, comment);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/comments/{commentId}")]
        public async Task<ActionResult<ArticleDto>> AddArticleCommentReply([FromRoute] Guid id, [FromRoute] Guid commentId, [FromBody] CommentDto comment)
        {
            var result = await _manager.AddCommentReplyAsync(id, commentId, comment);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/comments/{commentId}/up-vote")]
        public async Task<IActionResult> UpVoteComment([FromRoute] Guid id, [FromRoute] Guid commentId)
        {
            var result = await _manager.VoteCommentAsync(id, commentId, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/comments/{commentId}/down-vote")]
        public async Task<IActionResult> DownVoteComment([FromRoute] Guid id, [FromRoute] Guid commentId)
        {
            var result = await _manager.VoteCommentAsync(id, commentId, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/questions")]
        public async Task<ActionResult<ArticleDto>> AddArticleQuestion([FromRoute] Guid id, [FromBody] QuestionDto question)
        {
            var result = await _manager.AddQuestionAsync(id, question);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/questions/{questionId}/up-vote")]
        public async Task<IActionResult> UpVoteQuestion([FromRoute] Guid id, [FromRoute] Guid questionId)
        {
            var result = await _manager.VoteQuestionAsync(id, questionId, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/questions/{questionId}/down-vote")]
        public async Task<IActionResult> DownVoteQuestion([FromRoute] Guid id, [FromRoute] Guid questionId)
        {
            var result = await _manager.VoteQuestionAsync(id, questionId, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/questions/{questionId}")]
        public async Task<ActionResult<ArticleDto>> AddAnswerToQuestion([FromRoute] Guid id, [FromRoute] Guid questionId, [FromBody] AnswerDto answer)
        {
            var result = await _manager.AddAnswerAsync(id, questionId, answer);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/questions/{questionId}/answers/{answerId}/up-vote")]
        public async Task<IActionResult> UpVoteAnswer([FromRoute] Guid id, [FromRoute] Guid questionId, [FromRoute] Guid answerId)
        {
            var result = await _manager.VoteAnswerAsync(id, questionId, answerId, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/questions/{questionId}/answers/{answerId}/down-vote")]
        public async Task<IActionResult> DownVoteAnswer([FromRoute] Guid id, [FromRoute] Guid questionId, [FromRoute] Guid answerId)
        {
            var result = await _manager.VoteAnswerAsync(id, questionId, answerId, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        [HttpPost("{id}/questions/{questionId}/answers/{answerId}/accept")]
        public async Task<IActionResult> MarkAcceptedAnswer([FromRoute] Guid id, [FromRoute] Guid questionId, [FromRoute] Guid answerId)
        {
            // TODO: Add validation, only original poster can accept answers
            var result = await _manager.MarkAcceptedAnswerAsync(id, questionId, answerId);
            return result is null ? NotFound() : NoContent();
        }
    }
}
