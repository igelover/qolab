namespace Qolab.API.Entities
{
    public record Answer : Comment
    {
        public Question Question { get; set; }

        public bool IsAcceptedAnswer { get; set; }
    }
}
