using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class User
    {
        public User()
        {

        }

        public User(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public int Id { get; set; }

        [Required]
        public string FirstName {  get; set; }

        [Required]
        public string LastName {  get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? ProfilePicPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
