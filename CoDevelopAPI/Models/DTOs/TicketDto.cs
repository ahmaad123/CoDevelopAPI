using System.ComponentModel.DataAnnotations;

namespace CoDevelopAPI.Models.DTOs
{
    public class CreateTicketDto
    {
        public string Subject { get; set; }

        public int ReporterId { get; set; }

        public int? AssigneeId { get; set; }

        public int? ClientId { get; set; }

        public string? Status { get; set; } = "Open";

        public string? Priority { get; set; } = "Medium";
    }

    public class TicketResponseDto
    {
        public int TicketId { get; set; }
        public string Subject { get; set; }
        public int ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string ReporterEmail { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public string? AssigneeEmail { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
    }

    public class UpdateTicketDto
    {
        public string Subject { get; set; }

        public int? AssigneeId { get; set; }

        public int? ClientId { get; set; }

        public string? Status { get; set; }

        public string? Priority { get; set; }
    }

    public class AssignTicketDto
    {
        public int AssigneeId { get; set; }
    }

    public class UpdateTicketStatusDto
    {
        public string Status { get; set; }
    }
}