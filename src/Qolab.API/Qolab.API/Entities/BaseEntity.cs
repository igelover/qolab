using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public abstract record BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public User CreatedBy { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        [Required]
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    }
}
