using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qolab.API.Data;
using Qolab.API.Entities;
using Qolab.API.Models;
using System.Text.RegularExpressions;
using static Qolab.API.Models.Enums;

namespace Qolab.API.Managers
{
    public class ArticlesManager
    {
        private readonly ILogger<ArticlesManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public ArticlesManager(ILogger<ArticlesManager> logger, IConfiguration configuration, DataContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task<ArticleDto?> GetArticleAsync(Guid id)
        {
            var article = await _context.Articles
                                  .Include(article => article.Paper)
                                  .Include(article => article.CreatedBy)
                                  .Include(article => article.Comments)
                                      .ThenInclude(comment => comment.CreatedBy)
                                  .Include(article => article.Questions)
                                  .ThenInclude(question => question.CreatedBy)
                                  .Include(article => article.Questions)
                                      .ThenInclude(question => question.Answers)
                                      .ThenInclude(answer => answer.CreatedBy)
                                  .FirstOrDefaultAsync(x => x.Id == id);

            return article?.ToDto();
        }

        public async Task<IEnumerable<ArticleShortDto>> SearchArticlesAsync(string searchTerm)
        {
            var query = @"SELECT a.id, a.title, a.summary, a.tags, a.likes, a.dislikes, a.last_updated,
                          p.id, p.title, p.authors, p.publish_year, p.publish_month, p.publish_day, p.url, p.doi,
                          u.id, u.username
                          FROM articles as a
                          INNER JOIN users as u on a.created_by_id = u.id
                          LEFT JOIN papers as p on a.paper_id = p.id
                          WHERE to_tsvector(a.title) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(a.summary) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(a.tags) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(a.content) @@ to_tsquery(@searchTerms);";

            var searchTerms = searchTerm;
            if (!searchTerm.Contains(" | ") && !searchTerm.Contains(" & ")
                && !searchTerm.Contains(":*") && !Regex.IsMatch(searchTerm, @"<?>"))
            {
                searchTerms = string.Join(" | ", searchTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("QolabDb"));
            var result = await connection.QueryAsync<Article, Paper, User, Article>(query,
                (article, paper, user) =>
                {
                    article.Paper = paper;
                    article.CreatedBy = user;
                    return article;
                },
                new
                {
                    searchTerms
                });

            return result.Select(article => article.ToShortDto());
        }

        public async Task<ArticleDto> CreateArticleAsync(ArticleDto articleDto)
        {
            var article = new Article()
            {
                Title = articleDto.Title,
                Summary = articleDto.Summary,
                Tags = string.Join('¦', articleDto.Tags),
                Content = articleDto.Content,
                CreatedById = Guid.Parse(articleDto.CreatedBy)
            };
            var entry = await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
            var newArticle = await _context.Articles
                                           .Include(article => article.CreatedBy)
                                           .FirstAsync(article => article.Id == entry.Entity.Id);
            return newArticle.ToDto();
        }

        public async Task<Article?> UpdateArticleAsync(ArticleDto articleDto)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == articleDto.Id);
            if (article == null) return null;

            article.Title = articleDto.Title;
            article.Summary = articleDto.Summary;
            article.Tags = string.Join('¦', articleDto.Tags);
            article.Content = articleDto.Content;

            try
            {
                _context.Entry(article).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while updating article {articleId}", articleDto.Id);
                throw;
            }
            return article;
        }

        public async Task<Article?> DeleteArticleAsync(Guid id)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == id);
            if (article == null) return null;

            try
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting article {articleId}", id);
                throw;
            }
            return article;
        }

        public async Task<Guid?> VoteArticleAsync(Guid id, Vote vote)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == id);
            if (article == null) return null;

            switch (vote)
            {
                case Vote.UpVote:
                    article.Likes += 1;
                    break;
                case Vote.DownVote:
                    article.Dislikes += 1;
                    break;
            }
            try
            {
                _context.Entry(article).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while {voting} the article {articleId}", vote == Vote.UpVote ? "up-voting" : "down-voting", id);
                throw;
            }
            return article.Id;
        }

        public async Task<ArticleDto?> AddCommentAsync(Guid id, CommentDto comment)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == id);
            if (article == null) return null;

            try
            {
                _context.Comments.Add(new Comment
                {
                    Article = article,
                    Content = comment.Content,
                    CreatedById = Guid.Parse(comment.CreatedBy)
                });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while adding a comment to article {articleId}", id);
                throw;
            }
            return await GetArticleAsync(id);
        }

        public async Task<ArticleDto?> AddCommentReplyAsync(Guid id, Guid commentId, CommentDto comment)
        {
            var originalComment = _context.Comments.FirstOrDefault(c => c.ArticleId == id && c.Id == commentId);
            if (originalComment == null) return null;

            try
            {
                _context.Comments.Add(new Comment
                {
                    ArticleId = id,
                    ReplyToCommentId = commentId,
                    Content = comment.Content,
                    CreatedById = Guid.Parse(comment.CreatedBy)
                });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while adding a reply to comment {commentId} on article {articleId}", commentId, id);
                throw;
            }
            return await GetArticleAsync(id);
        }

        private async Task VoteAsync(AbstractComment entity, Vote vote)
        {
            switch (vote)
            {
                case Vote.UpVote:
                    entity.Likes += 1;
                    break;
                case Vote.DownVote:
                    entity.Dislikes += 1;
                    break;
            }
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Guid?> VoteCommentAsync(Guid id, Guid commentId, Vote vote)
        {
            var comment = _context.Comments.FirstOrDefault(c => c.ArticleId == id && c.Id == commentId);
            if (comment == null) return null;

            try
            {
                await VoteAsync(comment, vote);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while {voting} the comment {commentId} on article {articleId}", vote == Vote.UpVote ? "up-voting" : "down-voting", commentId, id);
                throw;
            }
            return comment.Id;
        }

        public async Task<ArticleDto?> AddQuestionAsync(Guid id, QuestionDto question)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == id);
            if (article == null) return null;

            try
            {
                _context.Questions.Add(new Question
                {
                    ArticleId = id,
                    Content = question.Content,
                    CreatedById = Guid.Parse(question.CreatedBy)
                });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while adding a question to article {articleId}", id);
                throw;
            }
            return await GetArticleAsync(id);
        }

        public async Task<Guid?> VoteQuestionAsync(Guid id, Guid questionId, Vote vote)
        {
            var question = _context.Questions.FirstOrDefault(c => c.ArticleId == id && c.Id == questionId);
            if (question == null) return null;

            try
            {
                await VoteAsync(question, vote);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while {voting} the question {questionId} on article {articleId}", vote == Vote.UpVote ? "up-voting" : "down-voting", questionId, id);
                throw;
            }
            return question.Id;
        }

        public async Task<ArticleDto?> AddAnswerAsync(Guid id, Guid questionId, AnswerDto answer)
        {
            var question = _context.Questions.FirstOrDefault(q => q.ArticleId == id && q.Id == questionId);
            if (question == null) return null;

            try
            {
                _context.Answers.Add(new Answer
                {
                    ArticleId = id,
                    QuestionId = questionId,
                    Content = answer.Content,
                    CreatedById = Guid.Parse(answer.CreatedBy)
                });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while adding an answer to question {questionId} on article {articleId}", questionId, id);
                throw;
            }
            return await GetArticleAsync(id);
        }

        public async Task<Guid?> VoteAnswerAsync(Guid id, Guid questionId, Guid answerId, Vote vote)
        {
            var answer = _context.Answers.FirstOrDefault(a => a.ArticleId == id && a.QuestionId == questionId && a.Id == answerId);
            if (answer == null) return null;

            try
            {
                await VoteAsync(answer, vote);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while {voting} the answer {answerId} on article {articleId}", vote == Vote.UpVote ? "up-voting" : "down-voting", answerId, id);
                throw;
            }
            return answer.Id;
        }

        public async Task<Guid?> MarkAcceptedAnswerAsync(Guid id, Guid questionId, Guid answerId)
        {
            var answer = _context.Answers
                .Include(answer => answer.Question)
                .FirstOrDefault(a => a.ArticleId == id && a.QuestionId == questionId && a.Id == answerId);

            if (answer == null) return null;

            if (answer.Question.ResolvedOn.HasValue)
            {
                return Guid.Empty;
            }

            answer.IsAcceptedAnswer = true;
            answer.Question.ResolvedOn = DateTime.UtcNow;

            try
            {
                _context.Entry(answer).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while marking answer {answerId} as accepted.", answerId);
                throw;
            }
            return answer.Id;
        }
    }
}
