using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public record Paper : BaseEntity
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Authors { get; set; }

        [Required]
        public string Abstract { get; set; }

        public int? PublishYear { get; set; }

        public int? PublishMonth { get; set; }

        public int? PublishDay { get; set; }

        public string? Url { get; set; }

        public string? DOI { get; set; }
    }
}
