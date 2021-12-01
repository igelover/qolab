using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qolab.API.Entities
{
    public record AbstractComment : BaseEntity
    {
        [Required]
        public Article Article { get; set; }

        [Required]
        public Guid ArticleId { get; set; }

        [Required]
        public string Content { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }
    }
}
