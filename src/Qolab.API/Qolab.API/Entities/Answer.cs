using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public record Answer : AbstractComment
    {
        [Required]
        public Question Question { get; set; }
        
        [Required]
        public Guid QuestionId { get; set; }

        public bool IsAcceptedAnswer { get; set; }
    }
}
