using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace CoDevelopAPI.Models.DTOs
{
    public class CreateUserDto
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? Department { get; set; }

        public int? Phone { get; set; }

        public bool? IsActive { get; set; } = true;

        public int? RoleId { get; set; }
    }

    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? Department { get; set; }
        public int? Phone { get; set; }
        public bool? IsActive { get; set; }
        public string? RoleName { get; set; }
    }

    public class UpdateUserDto
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? Department { get; set; }

        public int? Phone { get; set; }

        public bool? IsActive { get; set; }

        public int? RoleId { get; set; }
    }
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
    }
}