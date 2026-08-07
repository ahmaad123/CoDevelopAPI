using System.ComponentModel.DataAnnotations;

namespace CoDevelopAPI.Models.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDetailDto User { get; set; }
    }

    public class UserDetailDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? Department { get; set; }
        public int? Phone { get; set; }
        public bool IsActive { get; set; }
        public string? RoleName { get; set; }
    }
}