using Qolab.API.Models;
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

        public PaperDto ToDto()
        {
            return new PaperDto
            {
                Id = Id,
                Title = Title,
                Authors = Authors.Split('¦'),
                Abstract = Abstract,
                PublishDate = GetPublishDate(PublishYear, PublishMonth, PublishDay),
                Url = Url,
                DOI = DOI,
                CreatedById = CreatedBy!.Id,
                CreatedBy = CreatedBy!.Username,
                LastUpdated = LastUpdated
            };
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
