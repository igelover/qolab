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

        public void FromDto(ArticleDto articleDto)
        {
            Title = articleDto.Title;
            Summary = articleDto.Summary;
            Tags = string.Join('¦', articleDto.Tags);
            Content = articleDto.Content;
            CreatedById = articleDto.CreatedById;
        }

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
                CreatedById = CreatedBy!.Id,
                CreatedBy = CreatedBy!.Username,
                LastUpdated = LastUpdated,
                Paper = Paper?.ToDto()
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
                CreatedById = CreatedBy!.Id,
                CreatedBy = CreatedBy!.Username,
                LastUpdated = LastUpdated,
                Paper = Paper?.ToDto(),
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
                CreatedById = comment.CreatedBy!.Id,
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
                CreatedById = question.CreatedBy!.Id,
                CreatedBy = question.CreatedBy!.Username,
                ResolvedOn = question.ResolvedOn,
                LastUpdated = question.LastUpdated,
                Answers = question.Answers?.Select(answer => new AnswerDto
                {
                    Id = answer.Id,
                    IsAcceptedAnswer = answer.IsAcceptedAnswer,
                    Content = answer.Content,
                    Likes = answer.Likes,
                    Dislikes = answer.Dislikes,
                    CreatedById = answer.CreatedBy!.Id,
                    CreatedBy = answer.CreatedBy!.Username,
                    LastUpdated = answer.LastUpdated,
                }).OrderByDescending(answer => answer.Likes - answer.Dislikes)
                  .ThenByDescending(answer => answer.LastUpdated)
            });
        }
    }
}
