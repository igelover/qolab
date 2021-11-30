using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public abstract record BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;

        public Guid CreatedBy { get; set; }

        public Guid ModifiedBy { get; set; }
    }
}
