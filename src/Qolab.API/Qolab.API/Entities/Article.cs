using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qolab.API.Entities
{
    public record Article : BaseEntity
    {
        [Required]

        public string Title { get; set; }

        public string Summary { get; set; }

        [Required]
        protected string Keywords { get; set; }

        [NotMapped]
        public IEnumerable<string> Tags
        {
            get
            {
                return Keywords?.Split(';');
            }
            set
            {
                Keywords = string.Join(';', value);
            }
        }

        [Required]
        public string Content { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public IList<Comment> Comments { get; set; }
    }
}
