using System.ComponentModel.DataAnnotations;

namespace CoDevelopAPI.Models.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Task name is required")]
        [MaxLength(255, ErrorMessage = "Task name cannot exceed 255 characters")]
        public string TaskName { get; set; }

        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDateTime { get; set; }

        [Required(ErrorMessage = "Assigned to is required")]
        public int AssignedTo { get; set; }

        [Required(ErrorMessage = "Assigned by is required")]
        public int AssignedBy { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } = "Pending";
    }

    public class TaskResponseDto
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Status { get; set; }
        public int? AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public int? AssignedBy { get; set; }
        public string AssignedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required(ErrorMessage = "Task name is required")]
        [MaxLength(255, ErrorMessage = "Task name cannot exceed 255 characters")]
        public string TaskName { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDateTime { get; set; }

        public int? AssignedTo { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; }
    }

    public class UpdateTaskStatusDto
    {
        public string Status { get; set; }
    }
}
