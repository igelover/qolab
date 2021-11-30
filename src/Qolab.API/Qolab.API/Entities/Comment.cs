namespace Qolab.API.Entities
{
    public record Comment : AbstractComment
    {
        public Guid? ReplyToComentId { get; set; }
    }
}
