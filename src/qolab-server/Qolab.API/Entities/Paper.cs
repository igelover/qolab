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

        public static (int? publishYear, int? publishMonth, int? publishDay) GetPublishDateInfo(string? publishDate)
        {
            int publishYear = 0;
            int publishMonth = 0;
            int publishDay = 0;
            var publishDateParts = publishDate?.Split('-');
            if (publishDateParts?.Length > 0)
            {
                _ = int.TryParse(publishDateParts[0], out publishYear);
            }
            if (publishDateParts?.Length > 1)
            {
                _ = int.TryParse(publishDateParts[1], out publishMonth);
            }
            if (publishDateParts?.Length > 2)
            {
                _ = int.TryParse(publishDateParts[2], out publishDay);
            }

            return (publishYear > 0 ? publishYear : null, publishMonth > 0 ? publishMonth : null, publishDay > 0 ? publishDay : null);
        }

        public void FromDto(PaperDto paperDto)
        {
            var (publishYear, publishMonth, publishDay) = GetPublishDateInfo(paperDto.PublishDate);
            Title = paperDto.Title;
            Authors = string.Join('¦', paperDto.Authors);
            Abstract = paperDto.Abstract;
            PublishYear = publishYear;
            PublishMonth = publishMonth;
            PublishDay = publishDay;
            Url = paperDto.Url;
            DOI = paperDto.DOI;
            CreatedById = paperDto.CreatedById;
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
