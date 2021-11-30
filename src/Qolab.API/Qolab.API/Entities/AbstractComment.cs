using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public record AbstractComment : BaseEntity
    {
        [Required]
        public Article Article { get; set; }

        [Required]
        public string Content { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }
    }
}
