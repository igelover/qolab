using System.ComponentModel.DataAnnotations.Schema;

namespace Qolab.API.Entities
{
    public record Question : AbstractComment
    {
        public DateTimeOffset? ResolvedOn { get; set; }

        public IList<Answer> Answers { get; set; }
    }
}
