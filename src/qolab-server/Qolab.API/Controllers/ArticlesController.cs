using Microsoft.AspNetCore.Mvc;
using Qolab.API.Managers;
using Qolab.API.Models;
using static Qolab.API.Models.Enums;

namespace Qolab.API.Controllers
{
    /// <summary>
    /// Articles controller, handles all operations related to articles and its properties
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ArticlesManager _manager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager">The <see cref="ArticlesManager"/> that tals to the DB</param>
        public ArticlesController(ArticlesManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Gets an article by its internal ID
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <returns>The full article object</returns>
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

        /// <summary>
        /// Performs a Full Text Search on the article Title, Summary, Tags and Content
        /// </summary>
        /// <param name="searchTerm">The term to search</param>
        /// <returns>A collection of articles matching the search criteria</returns>
        /// <remarks><a href="https://www.compose.com/articles/mastering-postgresql-tools-full-text-search-and-phrase-search/">Full Text Search reference</a></remarks>
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

        /// <summary>
        /// Createas a new article entry on the DB
        /// </summary>
        /// <param name="article">The article to create</param>
        /// <returns>The artcile created</returns>
        [HttpPost]
        public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] ArticleDto article)
        {
            var result = await _manager.CreateArticleAsync(article);
            return CreatedAtAction(nameof(GetArticle), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates Title, Summary, Tags and Content of an existing article
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="article">The article to update</param>
        /// <returns>The updated article object</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ArticleDto>> UpdateArticle([FromRoute] Guid id, [FromBody] ArticleDto article)
        {
            if (id != article.Id)
            {
                return BadRequest();
            }

            // TODO: Add validation, only author can modify articles
            var result = await _manager.UpdateArticleAsync(article);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Deletes an existing article
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle([FromRoute] Guid id)
        {
            // TODO: Add validation, only author can delete articles
            var result = await _manager.DeleteArticleAsync(id);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds a +1 to the article likes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/up-vote")]
        public async Task<IActionResult> UpVoteArticle([FromRoute] Guid id)
        {
            var result = await _manager.VoteArticleAsync(id, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds a +1 to the article dislikes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/down-vote")]
        public async Task<IActionResult> DownVoteArticle([FromRoute] Guid id)
        {
            var result = await _manager.VoteArticleAsync(id, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds a comment to an existing article
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="comment">The comment to create</param>
        /// <returns>The updated article object</returns>
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<ArticleDto>> AddArticleComment([FromRoute] Guid id, [FromBody] CommentDto comment)
        {
            var result = await _manager.AddCommentAsync(id, comment);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Adds a reply to an existing comment
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="commentId">The existing comment internal ID</param>
        /// <param name="comment">The comment reply to create</param>
        /// <returns>The updated article object</returns>
        [HttpPost("{id}/comments/{commentId}")]
        public async Task<ActionResult<ArticleDto>> AddArticleCommentReply([FromRoute] Guid id, [FromRoute] Guid commentId, [FromBody] CommentDto comment)
        {
            var result = await _manager.AddCommentReplyAsync(id, commentId, comment);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Adds a +1 to the comment likes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="commentId">The existing comment internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/comments/{commentId}/up-vote")]
        public async Task<IActionResult> UpVoteComment([FromRoute] Guid id, [FromRoute] Guid commentId)
        {
            var result = await _manager.VoteCommentAsync(id, commentId, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds a +1 to the comment dislikes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="commentId">The existing comment internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/comments/{commentId}/down-vote")]
        public async Task<IActionResult> DownVoteComment([FromRoute] Guid id, [FromRoute] Guid commentId)
        {
            var result = await _manager.VoteCommentAsync(id, commentId, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds a question to an existing article
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="question">The question to create</param>
        /// <returns>The updated article object</returns>
        [HttpPost("{id}/questions")]
        public async Task<ActionResult<ArticleDto>> AddArticleQuestion([FromRoute] Guid id, [FromBody] QuestionDto question)
        {
            var result = await _manager.AddQuestionAsync(id, question);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Adds a +1 to the question likes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="questionId">The existing question internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/questions/{questionId}/up-vote")]
        public async Task<IActionResult> UpVoteQuestion([FromRoute] Guid id, [FromRoute] Guid questionId)
        {
            var result = await _manager.VoteQuestionAsync(id, questionId, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds a +1 to the question dislikes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="questionId">The existing question internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/questions/{questionId}/down-vote")]
        public async Task<IActionResult> DownVoteQuestion([FromRoute] Guid id, [FromRoute] Guid questionId)
        {
            var result = await _manager.VoteQuestionAsync(id, questionId, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds and answer to an existing question
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="questionId">The existing question internal ID</param>
        /// <param name="answer">The answer to create</param>
        /// <returns>The updated article object</returns>
        [HttpPost("{id}/questions/{questionId}")]
        public async Task<ActionResult<ArticleDto>> AddAnswerToQuestion([FromRoute] Guid id, [FromRoute] Guid questionId, [FromBody] AnswerDto answer)
        {
            var result = await _manager.AddAnswerAsync(id, questionId, answer);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Adds a +1 to the answer likes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="questionId">The existing question internal ID</param>
        /// <param name="answerId">The existing answer internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/questions/{questionId}/answers/{answerId}/up-vote")]
        public async Task<IActionResult> UpVoteAnswer([FromRoute] Guid id, [FromRoute] Guid questionId, [FromRoute] Guid answerId)
        {
            var result = await _manager.VoteAnswerAsync(id, questionId, answerId, Vote.UpVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Adds a +1 to the answer dislikes counter.
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="questionId">The existing question internal ID</param>
        /// <param name="answerId">The existing answer internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/questions/{questionId}/answers/{answerId}/down-vote")]
        public async Task<IActionResult> DownVoteAnswer([FromRoute] Guid id, [FromRoute] Guid questionId, [FromRoute] Guid answerId)
        {
            var result = await _manager.VoteAnswerAsync(id, questionId, answerId, Vote.DownVote);
            return result is null ? NotFound() : NoContent();
        }

        /// <summary>
        /// Marks an existing answer as accepted
        /// </summary>
        /// <param name="id">The article internal ID</param>
        /// <param name="questionId">The existing question internal ID</param>
        /// <param name="answerId">The existing answer internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpPost("{id}/questions/{questionId}/answers/{answerId}/accept")]
        public async Task<IActionResult> MarkAcceptedAnswer([FromRoute] Guid id, [FromRoute] Guid questionId, [FromRoute] Guid answerId)
        {
            // TODO: Add validation, only original poster can accept answers
            var result = await _manager.MarkAcceptedAnswerAsync(id, questionId, answerId);
            return result is null ? NotFound() : NoContent();
        }
    }
}
