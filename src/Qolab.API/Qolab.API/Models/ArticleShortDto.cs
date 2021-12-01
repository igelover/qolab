namespace Qolab.API.Models
{
    public class ArticleShortDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public PaperDto? Paper { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
