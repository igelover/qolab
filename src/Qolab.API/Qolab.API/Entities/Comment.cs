namespace Qolab.API.Entities
{
    public record Comment : AbstractComment
    {
        public Guid? ReplyToCommentId { get; set; }
    }
}
