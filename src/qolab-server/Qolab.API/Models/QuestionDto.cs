namespace Qolab.API.Models
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public IEnumerable<AnswerDto>? Answers { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
