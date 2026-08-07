namespace CoDevelopAPI.Models.DTOs
{
    public class RolePermissionResponseDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public int PermissionCount { get; set; }
        public int TotalPermissions { get; set; }
        public int Percentage { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    public class PermissionDto
    {
        public int PermId { get; set; }
        public string PermName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Module { get; set; }
        public string? Action { get; set; }
        public string? Resource { get; set; }
        public bool IsAssigned { get; set; }
    }

    public class AssignPermissionsDto
    {
        public List<PermissionAssignmentDto> Permissions { get; set; } = new List<PermissionAssignmentDto>();
    }
    public class PermissionAssignmentDto
    {
        public int PermissionId { get; set; }
        public bool IsAllowed { get; set; } = true;
    }
    public class AllRolesPermissionsResponseDto
    {
        public List<RolePermissionSummaryDto> Roles { get; set; } = new List<RolePermissionSummaryDto>();
    }

    public class RolePermissionSummaryDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public int PermissionCount { get; set; }
        public int TotalPermissions { get; set; }
        public int Percentage { get; set; }
    }

    public class CategorizedPermissionsResponseDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<CategoryPermissionsDto> Categories { get; set; } = new List<CategoryPermissionsDto>();
    }

    public class CategoryPermissionsDto
    {
        public string CategoryName { get; set; }
        public int TotalInCategory { get; set; }
        public int AssignedInCategory { get; set; }
        public bool AllChecked { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
}