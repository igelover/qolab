using Qolab.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public record Article : BaseEntity
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Summary { get; set; }

        [Required]
        public string Tags { get; set; }

        [Required]
        public string Content { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public IEnumerable<Comment>? Comments { get; set; }
        
        public IEnumerable<Question>? Questions { get; set; }

        public Paper? Paper { get; set; }

        public ArticleShortDto ToShortDto()
        {
            return new ArticleShortDto
            {
                Id = Id,
                Title = Title,
                Summary = Summary,
                Tags = Tags.Split('¦'),
                Likes = Likes,
                Dislikes = Dislikes,
                CreatedBy = CreatedBy!.Username,
                LastUpdated = LastUpdated,
                Paper = Paper is null ? null : new PaperDto
                {
                    Id = Paper.Id,
                    Title = Paper.Title,
                    Authors = Paper.Authors.Split('¦'),
                    PublishDate = GetPublishDate(Paper.PublishYear, Paper.PublishMonth, Paper.PublishDay),
                    Url = Paper.Url,
                    DOI = Paper.DOI
                }
            };
        }

        public ArticleDto ToDto()
        {
            return new ArticleDto
            {
                Id = Id,
                Title = Title,
                Summary = Summary,
                Tags = Tags.Split('¦'),
                Content = Content,
                Likes = Likes,
                Dislikes = Dislikes,
                CreatedBy = CreatedBy!.Username,
                LastUpdated = LastUpdated,
                Paper = Paper is null ? null: new PaperDto
                {
                    Id = Paper.Id,
                    Title = Paper.Title,
                    Authors = Paper.Authors.Split('¦'),
                    Abstract = Paper.Abstract,
                    PublishDate = GetPublishDate(Paper.PublishYear, Paper.PublishMonth, Paper.PublishDay),
                    Url = Paper.Url,
                    DOI = Paper.DOI
                },
                Comments = GetComments(),
                Questions = GetQuestions()
            };
        }

        private IEnumerable<CommentDto>? GetComments()
        {
            var comments = Comments?.Where(comment => comment.ReplyToCommentId is null)
                                    .OrderByDescending(comment => comment.Likes - comment.Dislikes)
                                    .ThenByDescending(comment => comment.LastUpdated)
                                    .ToList();

            if (comments is not null)
            {
                for (int i = 0; i < comments.Count; i++)
                {
                    var replies = Comments!.Where(comment => comment.ReplyToCommentId == comments[i].Id)
                                           .OrderByDescending(comment => (comment.Likes - comment.Dislikes))
                                           .ThenByDescending(comment => comment.LastUpdated);
                    comments.InsertRange(i + 1, replies);
                }
            }
            return comments?.Select(comment => new CommentDto
            {
                Id = comment.Id,
                ReplyToComentId = comment.ReplyToCommentId,
                Content = comment.Content,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
                CreatedBy = comment.CreatedBy!.Username,
                LastUpdated = comment.LastUpdated
            });
        }

        private IEnumerable<QuestionDto>? GetQuestions()
        {
            var questions = Questions?.OrderByDescending(question => question.Likes - question.Dislikes)
                                      .ThenByDescending(question => question.LastUpdated)
                                      .ToList();

            return questions?.Select(question => new QuestionDto
            {
                Id = question.Id,
                Content = question.Content,
                Likes = question.Likes,
                Dislikes = question.Dislikes,
                CreatedBy = question.CreatedBy!.Username,
                LastUpdated = question.LastUpdated,
                Answers = question.Answers?.Select(answer => new AnswerDto
                {
                    Id = answer.Id,
                    IsAcceptedAnswer = answer.IsAcceptedAnswer,
                    Content = answer.Content,
                    Likes = answer.Likes,
                    Dislikes = answer.Dislikes,
                    CreatedBy = answer.CreatedBy!.Username,
                    LastUpdated = answer.LastUpdated,
                }).OrderByDescending(answer => answer.Likes - answer.Dislikes)
                  .ThenByDescending(answer => answer.LastUpdated)
            });
        }

        private static string? GetPublishDate(int? publishYear, int? publishMonth, int? publishDay)
        {
            if (publishYear.HasValue && publishMonth.HasValue && publishDay.HasValue)
            {
                var aux = new DateTime(publishYear.Value, publishMonth.Value, publishDay.Value);
                return aux.ToString("MMM dd, yyyy");
            }
            if (publishYear.HasValue && publishMonth.HasValue)
            {
                var aux = new DateTime(publishYear.Value, publishMonth.Value, 1);
                return aux.ToString("MMM, yyyy");
            }
            return publishYear?.ToString();
        }
    }
}
