using System.ComponentModel.DataAnnotations;

namespace CoDevelopAPI.Models.DTOs
{
    public class CreateRoleDto
    {
        //[Required(ErrorMessage = "Role name is required")]
        //[MaxLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        public string RoleName { get; set; }


        //[Range(0, int.MaxValue, ErrorMessage = "Level must be a positive number")]
        public int Level { get; set; } = 0;

        //[MaxLength(100)]
        public string? RoleCode { get; set; }

        public int? IsActive { get; set; } = 1;

        //[MaxLength(255)]
        public string? Description { get; set; }
    }

    public class RoleResponseDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string? ParentRoleName { get; set; }
        public int Level { get; set; }
        public string? RoleCode { get; set; }
        public bool? IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime? Created { get; set; }
        public int ChildRolesCount { get; set; }
    }

    public class UpdateRoleDto
    {
        //[Required(ErrorMessage = "Role name is required")]
        //[MaxLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        public string RoleName { get; set; }

        //[Range(0, int.MaxValue)]
        public int Level { get; set; }

        //[MaxLength(100)]
        public string? RoleCode { get; set; }

        public int? IsActive { get; set; }

        //[MaxLength(255)]
        public string? Description { get; set; }
    }
}
