using System.ComponentModel.DataAnnotations;

namespace CoDevelopAPI.Models.DTOs
{
    public class CreateClientDto
    {
        public string BusinessType { get; set; }

        public string BusinessName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public string? City { get; set; }

        public string? Address { get; set; }

        public decimal MonthlyPrice { get; set; }

        public string? Status { get; set; } = "Active";
    }

    public class ClientResponseDto
    {
        public int ClientId { get; set; }
        public string BusinessType { get; set; }
        public string BusinessName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public decimal MonthlyPrice { get; set; }
        public string Status { get; set; }
        public int ProjectCount { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class UpdateClientDto
    {
        public string BusinessType { get; set; }

        public string BusinessName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public string? City { get; set; }

        public string? Address { get; set; }

        public decimal MonthlyPrice { get; set; }

        public string? Status { get; set; }
    }
}
