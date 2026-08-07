using System.ComponentModel.DataAnnotations;

namespace CoDevelopAPI.Models.DTOs
{
    public class CreatePermissionDto
    {
        public string PermName { get; set; }

        public int? CategoryId { get; set; }

        public string? Module { get; set; }

        public string? Action { get; set; }

        public string? Resource { get; set; }
    }

    public class PermissionResponseDto
    {
        public int PermId { get; set; }
        public string PermName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Module { get; set; }
        public string? Action { get; set; }
        public string? Resource { get; set; }
    }

    public class UpdatePermissionDto
    {
        public string PermName { get; set; }

        public int? CategoryId { get; set; }

        public string? Module { get; set; }

        public string? Action { get; set; }

        public string? Resource { get; set; }
    }
}