using Microsoft.EntityFrameworkCore;
using Qolab.API.Data;
using Qolab.API.Models;

namespace Qolab.API.Managers
{
    public class ArticlesManager
    {
        private readonly DataContext _context;

        public ArticlesManager(DataContext context)
        {
            _context = context;
        }

        public async Task<ArticleDto> GetArticleAsync(Guid id)
        {
            var article = _context.Articles
                                  .Include(article => article.CreatedBy)
                                  .Include(article => article.Comments)
                                      .ThenInclude(comment => comment.CreatedBy)
                                  .Include(article => article.Questions)
                                  .ThenInclude(question => question.CreatedBy)
                                  .Include(article => article.Questions)
                                      .ThenInclude(question => question.Answers)
                                      .ThenInclude(answer => answer.CreatedBy)
                                  .FirstOrDefault(x => x.Id == id);
            
            if (article == null)
            {
                return null;
            }

            return new ArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Summary = article.Summary,
                Tags = article.Tags?.Split(';'),
                Content = article.Content,
                Likes = article.Likes,
                Dislikes = article.Dislikes,
                CreatedBy = article.CreatedBy.UserName,
                LastUpdated = article.LastUpdated,
                Comments = article.Comments?.Select(comment => new CommentDto
                {
                    Id = comment.Id,
                    ReplyToComentId = comment.ReplyToCommentId,
                    Content = comment.Content,
                    Likes = comment.Likes,
                    Dislikes = comment.Dislikes,
                    CreatedBy = comment.CreatedBy.UserName,
                    LastUpdated = comment.LastUpdated
                }),
                Questions = article.Questions?.Select(question => new QuestionDto
                {
                    Id = question.Id,
                    Content = question.Content,
                    Likes = question.Likes,
                    Dislikes = question.Dislikes,
                    CreatedBy = question.CreatedBy.UserName,
                    LastUpdated = question.LastUpdated,
                    Answers = question.Answers?.Select(answer => new AnswerDto
                    {
                        Id = answer.Id,
                        IsAcceptedAnswer = answer.IsAcceptedAnswer,
                        Content = answer.Content,
                        Likes = answer.Likes,
                        Dislikes = answer.Dislikes,
                        CreatedBy = answer.CreatedBy.UserName,
                        LastUpdated = answer.LastUpdated,
                    })
                })
            };
        }
    }
}
