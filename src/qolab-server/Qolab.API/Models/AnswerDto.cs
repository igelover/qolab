namespace Qolab.API.Models
{
    public class AnswerDto
    {
        public Guid Id { get; set; }
        public bool IsAcceptedAnswer { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public Guid CreatedById { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
