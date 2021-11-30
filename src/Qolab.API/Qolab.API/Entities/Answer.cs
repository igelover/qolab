namespace Qolab.API.Entities
{
    public record Answer : AbstractComment
    {
        public Question Question { get; set; }

        public bool IsAcceptedAnswer { get; set; }
    }
}
