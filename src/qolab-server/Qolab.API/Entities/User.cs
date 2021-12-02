using System.ComponentModel.DataAnnotations;

namespace Qolab.API.Entities
{
    public record User
    {
        [Key]
        public Guid Id { get; set; }

        public string Username { get; set; }
        
        public string? FullName { get; set; }

        public string? Email { get; set; }
    }
}
