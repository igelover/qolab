using System.ComponentModel.DataAnnotations.Schema;

namespace Qolab.API.Entities
{
    public record Question : Comment
    {
        [NotMapped]
        public bool IsResolved
        {
            get
            {
                return ResolvedOn.HasValue;
            }
        }
        public DateTimeOffset? ResolvedOn { get; set; }

        public IList<Answer> Answers { get; set; }
    }
}
