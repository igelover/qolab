namespace Qolab.API.Models
{
    public class PaperDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public string Abstract { get; set; }
        public string? PublishDate { get; set; }
        public string? Url { get; set; }
        public string? DOI { get; set; }
    }
}
