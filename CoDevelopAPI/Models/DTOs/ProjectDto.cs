using System.ComponentModel.DataAnnotations;

namespace CoDevelopAPI.Models.DTOs
{
    public class CreateProjectDto
    {
        public string ProjectName { get; set; }

        public int ClientId { get; set; }

        public string Developer { get; set; }

        public string Manager { get; set; }

        public int? Progress { get; set; } = 0;

        public decimal Budget { get; set; }

        public DateTime Deadline { get; set; }

        public string? Status { get; set; } = "Active";

        public string? Description { get; set; }

        public int CreatedBy { get; set; }

        public List<int> AssigneeIds { get; set; } = new List<int>();
    }

    public class ProjectResponseDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Developer { get; set; }
        public string Manager { get; set; }
        public int? Progress { get; set; }
        public decimal Budget { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }
        public string? Description { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public List<ProjectAssigneeDto> Assignees { get; set; } = new List<ProjectAssigneeDto>();
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
    }

    public class ProjectAssigneeDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? AssignedBy { get; set; }
        public string? AssignedByName { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class ProjectCardDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public int? Progress { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public int AssigneeCount { get; set; }
    }

    public class UpdateProjectDto
    {
        public string ProjectName { get; set; }

        public int ClientId { get; set; }

        public string Developer { get; set; }

        public string Manager { get; set; }

        public int? Progress { get; set; }

        public decimal Budget { get; set; }

        public DateTime Deadline { get; set; }

        public string? Status { get; set; }

        public string? Description { get; set; }

        public List<int> AssigneeIds { get; set; } = new List<int>();
    }

    public class AssignProjectMembersDto
    {
        public List<int> UserIds { get; set; } = new List<int>();
        public int AssignedBy { get; set; }
    }

    public class UpdateProjectStatusDto
    {
        public string Status { get; set; }
    }

    public class UpdateProjectProgressDto
    {
        public int Progress { get; set; }
    }
}
