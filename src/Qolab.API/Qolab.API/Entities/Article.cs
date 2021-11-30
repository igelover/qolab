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

        public IList<Comment>? Comments { get; set; }
        
        public IList<Question>? Questions { get; set; }
    }
}
