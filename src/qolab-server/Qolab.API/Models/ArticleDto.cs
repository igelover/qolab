namespace Qolab.API.Models
{
    public class ArticleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public PaperDto? Paper { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public IEnumerable<CommentDto>? Comments { get; set; }
        public IEnumerable<QuestionDto>? Questions { get; set; }
        public Guid CreatedById { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
