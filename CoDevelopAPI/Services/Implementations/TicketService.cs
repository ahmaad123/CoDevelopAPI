using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketService> _logger;

        public TicketService(ApplicationDbContext context, ILogger<TicketService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<TicketResponseDto>> CreateTicketAsync(CreateTicketDto createTicketDto)
        {
            try
            {
                // Validate reporter exists
                var reporter = await _context.Users.FindAsync(createTicketDto.ReporterId);
                if (reporter == null)
                {
                    return ApiResponse<TicketResponseDto>.ErrorResponse(
                        $"Reporter with ID {createTicketDto.ReporterId} not found", 400);
                }

                // Validate assignee if provided
                if (createTicketDto.AssigneeId.HasValue)
                {
                    var assignee = await _context.Users.FindAsync(createTicketDto.AssigneeId.Value);
                    if (assignee == null)
                    {
                        return ApiResponse<TicketResponseDto>.ErrorResponse(
                            $"Assignee with ID {createTicketDto.AssigneeId} not found", 400);
                    }
                }

                // Validate client if provided
                if (createTicketDto.ClientId.HasValue)
                {
                    var client = await _context.Users.FindAsync(createTicketDto.ClientId.Value);
                    if (client == null)
                    {
                        return ApiResponse<TicketResponseDto>.ErrorResponse(
                            $"Client with ID {createTicketDto.ClientId} not found", 400);
                    }
                }

                // Create new ticket
                var ticket = new Ticket
                {
                    Subject = createTicketDto.Subject,
                    Reporterid = createTicketDto.ReporterId,
                    Assigneeid = createTicketDto.AssigneeId,
                    Clientid = createTicketDto.ClientId,
                    Status = createTicketDto.Status ?? "Open",
                    Priority = createTicketDto.Priority ?? "Medium"
                };

                await _context.Tickets.AddAsync(ticket);
                await _context.SaveChangesAsync();

                // Load related data
                var createdTicket = await GetTicketWithDetailsAsync(ticket.Ticketid);
                var ticketResponse = MapToTicketResponseDto(createdTicket);

                _logger.LogInformation($"Ticket created successfully: {ticket.Ticketid} by User {createTicketDto.ReporterId}");
                return ApiResponse<TicketResponseDto>.SuccessResponse(
                    ticketResponse, "Ticket created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return ApiResponse<TicketResponseDto>.ErrorResponse(
                    "An error occurred while creating the ticket", 500);
            }
        }

        public async Task<ApiResponse<TicketResponseDto>> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                var ticket = await GetTicketWithDetailsAsync(ticketId);

                if (ticket == null)
                {
                    return ApiResponse<TicketResponseDto>.ErrorResponse(
                        $"Ticket with ID {ticketId} not found", 404);
                }

                var ticketResponse = MapToTicketResponseDto(ticket);
                return ApiResponse<TicketResponseDto>.SuccessResponse(ticketResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving ticket with ID: {ticketId}");
                return ApiResponse<TicketResponseDto>.ErrorResponse(
                    "An error occurred while retrieving the ticket", 500);
            }
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetAllTicketsAsync()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Reporter)
                    .Include(t => t.Assignee)
                    .Include(t => t.Client)
                    .OrderByDescending(t => t.Ticketid)
                    .ToListAsync();

                var ticketResponses = tickets.Select(MapToTicketResponseDto).ToList();
                return ApiResponse<List<TicketResponseDto>>.SuccessResponse(ticketResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tickets");
                return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving tickets", 500);
            }
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetMyAssignedTicketsAsync(int userId)
        {
            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Userid == userId);
                if (!userExists)
                {
                    return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                        $"User with ID {userId} not found", 404);
                }

                var tickets = await _context.Tickets
                    .Include(t => t.Reporter)
                    .Include(t => t.Assignee)
                    .Include(t => t.Client)
                    .Where(t => t.Assigneeid == userId)
                    .OrderByDescending(t => t.Ticketid)
                    .ToListAsync();

                var ticketResponses = tickets.Select(MapToTicketResponseDto).ToList();
                return ApiResponse<List<TicketResponseDto>>.SuccessResponse(ticketResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving assigned tickets for user ID: {userId}");
                return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving tickets", 500);
            }
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetMyReportedTicketsAsync(int userId)
        {
            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Userid == userId);
                if (!userExists)
                {
                    return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                        $"User with ID {userId} not found", 404);
                }

                var tickets = await _context.Tickets
                    .Include(t => t.Reporter)
                    .Include(t => t.Assignee)
                    .Include(t => t.Client)
                    .Where(t => t.Reporterid == userId)
                    .OrderByDescending(t => t.Ticketid)
                    .ToListAsync();

                var ticketResponses = tickets.Select(MapToTicketResponseDto).ToList();
                return ApiResponse<List<TicketResponseDto>>.SuccessResponse(ticketResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving reported tickets for user ID: {userId}");
                return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving tickets", 500);
            }
        }

        public async Task<ApiResponse<TicketResponseDto>> UpdateTicketAsync(int ticketId, UpdateTicketDto updateTicketDto)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);

                if (ticket == null)
                {
                    return ApiResponse<TicketResponseDto>.ErrorResponse(
                        $"Ticket with ID {ticketId} not found", 404);
                }

                // Validate assignee if provided
                if (updateTicketDto.AssigneeId.HasValue)
                {
                    var assignee = await _context.Users.FindAsync(updateTicketDto.AssigneeId.Value);
                    if (assignee == null)
                    {
                        return ApiResponse<TicketResponseDto>.ErrorResponse(
                            $"Assignee with ID {updateTicketDto.AssigneeId} not found", 400);
                    }
                }

                // Validate client if provided
                if (updateTicketDto.ClientId.HasValue)
                {
                    var client = await _context.Users.FindAsync(updateTicketDto.ClientId.Value);
                    if (client == null)
                    {
                        return ApiResponse<TicketResponseDto>.ErrorResponse(
                            $"Client with ID {updateTicketDto.ClientId} not found", 400);
                    }
                }

                // Update ticket properties
                ticket.Subject = updateTicketDto.Subject;
                ticket.Assigneeid = updateTicketDto.AssigneeId;
                ticket.Clientid = updateTicketDto.ClientId;
                ticket.Status = updateTicketDto.Status ?? ticket.Status;
                ticket.Priority = updateTicketDto.Priority ?? ticket.Priority;

                await _context.SaveChangesAsync();

                // Load related data
                var updatedTicket = await GetTicketWithDetailsAsync(ticketId);
                var ticketResponse = MapToTicketResponseDto(updatedTicket);

                _logger.LogInformation($"Ticket updated successfully: {ticketId}");
                return ApiResponse<TicketResponseDto>.SuccessResponse(
                    ticketResponse, "Ticket updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating ticket with ID: {ticketId}");
                return ApiResponse<TicketResponseDto>.ErrorResponse(
                    "An error occurred while updating the ticket", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteTicketAsync(int ticketId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);

                if (ticket == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        $"Ticket with ID {ticketId} not found", 404);
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ticket deleted successfully: {ticketId}");
                return ApiResponse<bool>.SuccessResponse(true, "Ticket deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting ticket with ID: {ticketId}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while deleting the ticket", 500);
            }
        }

        public async Task<ApiResponse<TicketResponseDto>> AssignTicketAsync(int ticketId, AssignTicketDto assignTicketDto)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);

                if (ticket == null)
                {
                    return ApiResponse<TicketResponseDto>.ErrorResponse(
                        $"Ticket with ID {ticketId} not found", 404);
                }

                var assignee = await _context.Users.FindAsync(assignTicketDto.AssigneeId);
                if (assignee == null)
                {
                    return ApiResponse<TicketResponseDto>.ErrorResponse(
                        $"User with ID {assignTicketDto.AssigneeId} not found", 400);
                }

                ticket.Assigneeid = assignTicketDto.AssigneeId;

                // If status is Open, change to In Progress when assigned
                if (ticket.Status == "Open")
                {
                    ticket.Status = "In Progress";
                }

                await _context.SaveChangesAsync();

                var updatedTicket = await GetTicketWithDetailsAsync(ticketId);
                var ticketResponse = MapToTicketResponseDto(updatedTicket);

                _logger.LogInformation($"Ticket {ticketId} assigned to user {assignTicketDto.AssigneeId}");
                return ApiResponse<TicketResponseDto>.SuccessResponse(
                    ticketResponse, "Ticket assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning ticket with ID: {ticketId}");
                return ApiResponse<TicketResponseDto>.ErrorResponse(
                    "An error occurred while assigning the ticket", 500);
            }
        }

        public async Task<ApiResponse<TicketResponseDto>> UpdateTicketStatusAsync(int ticketId, UpdateTicketStatusDto updateStatusDto)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);

                if (ticket == null)
                {
                    return ApiResponse<TicketResponseDto>.ErrorResponse(
                        $"Ticket with ID {ticketId} not found", 404);
                }

                ticket.Status = updateStatusDto.Status;
                await _context.SaveChangesAsync();

                var updatedTicket = await GetTicketWithDetailsAsync(ticketId);
                var ticketResponse = MapToTicketResponseDto(updatedTicket);

                _logger.LogInformation($"Ticket {ticketId} status updated to: {updateStatusDto.Status}");
                return ApiResponse<TicketResponseDto>.SuccessResponse(
                    ticketResponse, "Ticket status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for ticket ID: {ticketId}");
                return ApiResponse<TicketResponseDto>.ErrorResponse(
                    "An error occurred while updating ticket status", 500);
            }
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetTicketsByStatusAsync(string status)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Reporter)
                    .Include(t => t.Assignee)
                    .Include(t => t.Client)
                    .Where(t => t.Status.ToLower() == status.ToLower())
                    .OrderByDescending(t => t.Ticketid)
                    .ToListAsync();

                var ticketResponses = tickets.Select(MapToTicketResponseDto).ToList();
                return ApiResponse<List<TicketResponseDto>>.SuccessResponse(ticketResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tickets with status: {status}");
                return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving tickets", 500);
            }
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetTicketsByPriorityAsync(string priority)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Reporter)
                    .Include(t => t.Assignee)
                    .Include(t => t.Client)
                    .Where(t => t.Priority.ToLower() == priority.ToLower())
                    .OrderByDescending(t => t.Ticketid)
                    .ToListAsync();

                var ticketResponses = tickets.Select(MapToTicketResponseDto).ToList();
                return ApiResponse<List<TicketResponseDto>>.SuccessResponse(ticketResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tickets with priority: {priority}");
                return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving tickets", 500);
            }
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetTicketsByClientAsync(int clientId)
        {
            try
            {
                var clientExists = await _context.Users.AnyAsync(u => u.Userid == clientId);
                if (!clientExists)
                {
                    return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                        $"Client with ID {clientId} not found", 404);
                }

                var tickets = await _context.Tickets
                    .Include(t => t.Reporter)
                    .Include(t => t.Assignee)
                    .Include(t => t.Client)
                    .Where(t => t.Clientid == clientId)
                    .OrderByDescending(t => t.Ticketid)
                    .ToListAsync();

                var ticketResponses = tickets.Select(MapToTicketResponseDto).ToList();
                return ApiResponse<List<TicketResponseDto>>.SuccessResponse(ticketResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tickets for client ID: {clientId}");
                return ApiResponse<List<TicketResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving tickets", 500);
            }
        }

        // Helper methods
        private async Task<Ticket> GetTicketWithDetailsAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Reporter)
                .Include(t => t.Assignee)
                .Include(t => t.Client)
                .FirstOrDefaultAsync(t => t.Ticketid == ticketId);
        }

        private TicketResponseDto MapToTicketResponseDto(Ticket ticket)
        {
            return new TicketResponseDto
            {
                TicketId = ticket.Ticketid,
                Subject = ticket.Subject,
                ReporterId = ticket.Reporterid,
                ReporterName = ticket.Reporter != null
                    ? $"{ticket.Reporter.Firstname} {ticket.Reporter.Lastname}"
                    : "Unknown",
                ReporterEmail = ticket.Reporter?.Email,
                AssigneeId = ticket.Assigneeid,
                AssigneeName = ticket.Assignee != null
                    ? $"{ticket.Assignee.Firstname} {ticket.Assignee.Lastname}"
                    : null,
                AssigneeEmail = ticket.Assignee?.Email,
                ClientId = ticket.Clientid,
                ClientName = ticket.Client != null
                    ? $"{ticket.Client.Firstname} {ticket.Client.Lastname}"
                    : null,
                ClientEmail = ticket.Client?.Email,
                Status = ticket.Status,
                Priority = ticket.Priority
            };
        }
    }
}