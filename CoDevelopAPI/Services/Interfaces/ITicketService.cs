using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface ITicketService
    {
        Task<ApiResponse<TicketResponseDto>> CreateTicketAsync(CreateTicketDto createTicketDto);
        Task<ApiResponse<TicketResponseDto>> GetTicketByIdAsync(int ticketId);
        Task<ApiResponse<List<TicketResponseDto>>> GetAllTicketsAsync();
        Task<ApiResponse<List<TicketResponseDto>>> GetMyAssignedTicketsAsync(int userId);
        Task<ApiResponse<List<TicketResponseDto>>> GetMyReportedTicketsAsync(int userId);
        Task<ApiResponse<TicketResponseDto>> UpdateTicketAsync(int ticketId, UpdateTicketDto updateTicketDto);
        Task<ApiResponse<bool>> DeleteTicketAsync(int ticketId);
        Task<ApiResponse<TicketResponseDto>> AssignTicketAsync(int ticketId, AssignTicketDto assignTicketDto);
        Task<ApiResponse<TicketResponseDto>> UpdateTicketStatusAsync(int ticketId, UpdateTicketStatusDto updateStatusDto);
        Task<ApiResponse<List<TicketResponseDto>>> GetTicketsByStatusAsync(string status);
        Task<ApiResponse<List<TicketResponseDto>>> GetTicketsByPriorityAsync(string priority);
        Task<ApiResponse<List<TicketResponseDto>>> GetTicketsByClientAsync(int clientId);
    }
}