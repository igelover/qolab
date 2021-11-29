using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public record Article : BaseEntity
    {
        [Required]

        public string Title { get; set; }

        public string Summary { get; set; }

        [Required]
        public IEnumerable<string> Tags { get; set; }

        [Required]
        public string Content { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public IList<Comment> Comments { get; set; }
    }
}
