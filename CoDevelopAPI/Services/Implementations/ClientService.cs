using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientService> _logger;

        public ClientService(ApplicationDbContext context, ILogger<ClientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<ClientResponseDto>> CreateClientAsync(CreateClientDto createClientDto)
        {
            try
            {
                // Check if email already exists
                var emailExists = await _context.Clients
                    .AnyAsync(c => c.Email.ToLower() == createClientDto.Email.ToLower());

                if (emailExists)
                {
                    return ApiResponse<ClientResponseDto>.ErrorResponse(
                        $"Client with email '{createClientDto.Email}' already exists", 400);
                }

                // Check if mobile already exists
                var mobileExists = await _context.Clients
                    .AnyAsync(c => c.Mobile == createClientDto.Mobile);

                if (mobileExists)
                {
                    return ApiResponse<ClientResponseDto>.ErrorResponse(
                        $"Client with mobile '{createClientDto.Mobile}' already exists", 400);
                }

                // Create new client
                var client = new Client
                {
                    Businesstype = createClientDto.BusinessType,
                    Businessname = createClientDto.BusinessName,
                    Firstname = createClientDto.FirstName,
                    Lastname = createClientDto.LastName,
                    Email = createClientDto.Email,
                    Mobile = createClientDto.Mobile,
                    City = createClientDto.City,
                    Address = createClientDto.Address,
                    Monthlyprice = createClientDto.MonthlyPrice,
                    Status = createClientDto.Status ?? "Active"
                };

                await _context.Clients.AddAsync(client);
                await _context.SaveChangesAsync();

                var clientResponse = MapToClientResponseDto(client);

                _logger.LogInformation($"Client created successfully: {client.Businessname} (ID: {client.Clientid})");
                return ApiResponse<ClientResponseDto>.SuccessResponse(
                    clientResponse, "Client created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return ApiResponse<ClientResponseDto>.ErrorResponse(
                    "An error occurred while creating the client", 500);
            }
        }

        public async Task<ApiResponse<ClientResponseDto>> GetClientByIdAsync(int clientId)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Projects)
                    .Include(c => c.Invoices)
                    .FirstOrDefaultAsync(c => c.Clientid == clientId);

                if (client == null)
                {
                    return ApiResponse<ClientResponseDto>.ErrorResponse(
                        $"Client with ID {clientId} not found", 404);
                }

                var clientResponse = MapToClientResponseDto(client);
                return ApiResponse<ClientResponseDto>.SuccessResponse(clientResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving client with ID: {clientId}");
                return ApiResponse<ClientResponseDto>.ErrorResponse(
                    "An error occurred while retrieving the client", 500);
            }
        }

        public async Task<ApiResponse<List<ClientResponseDto>>> GetAllClientsAsync()
        {
            try
            {
                var clients = await _context.Clients
                    .Include(c => c.Projects)
                    .Include(c => c.Invoices)
                    .OrderBy(c => c.Businessname)
                    .ToListAsync();

                var clientResponses = clients.Select(MapToClientResponseDto).ToList();
                return ApiResponse<List<ClientResponseDto>>.SuccessResponse(clientResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clients");
                return ApiResponse<List<ClientResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving clients", 500);
            }
        }

        public async Task<ApiResponse<List<ClientResponseDto>>> GetActiveClientsAsync()
        {
            try
            {
                var clients = await _context.Clients
                    .Include(c => c.Projects)
                    .Include(c => c.Invoices)
                    .Where(c => c.Status == "Active")
                    .OrderBy(c => c.Businessname)
                    .ToListAsync();

                var clientResponses = clients.Select(MapToClientResponseDto).ToList();
                return ApiResponse<List<ClientResponseDto>>.SuccessResponse(clientResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active clients");
                return ApiResponse<List<ClientResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving active clients", 500);
            }
        }

        public async Task<ApiResponse<List<ClientResponseDto>>> GetClientsByStatusAsync(string status)
        {
            try
            {
                var clients = await _context.Clients
                    .Include(c => c.Projects)
                    .Include(c => c.Invoices)
                    .Where(c => c.Status.ToLower() == status.ToLower())
                    .OrderBy(c => c.Businessname)
                    .ToListAsync();

                var clientResponses = clients.Select(MapToClientResponseDto).ToList();
                return ApiResponse<List<ClientResponseDto>>.SuccessResponse(clientResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving clients with status: {status}");
                return ApiResponse<List<ClientResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving clients", 500);
            }
        }

        public async Task<ApiResponse<ClientResponseDto>> UpdateClientAsync(int clientId, UpdateClientDto updateClientDto)
        {
            try
            {
                var client = await _context.Clients.FindAsync(clientId);

                if (client == null)
                {
                    return ApiResponse<ClientResponseDto>.ErrorResponse(
                        $"Client with ID {clientId} not found", 404);
                }

                // Check if new email conflicts with existing client
                var emailExists = await _context.Clients
                    .AnyAsync(c => c.Email.ToLower() == updateClientDto.Email.ToLower()
                        && c.Clientid != clientId);

                if (emailExists)
                {
                    return ApiResponse<ClientResponseDto>.ErrorResponse(
                        $"Email '{updateClientDto.Email}' is already in use by another client", 400);
                }

                // Check if new mobile conflicts with existing client
                var mobileExists = await _context.Clients
                    .AnyAsync(c => c.Mobile == updateClientDto.Mobile
                        && c.Clientid != clientId);

                if (mobileExists)
                {
                    return ApiResponse<ClientResponseDto>.ErrorResponse(
                        $"Mobile '{updateClientDto.Mobile}' is already in use by another client", 400);
                }

                // Update client properties
                client.Businesstype = updateClientDto.BusinessType;
                client.Businessname = updateClientDto.BusinessName;
                client.Firstname = updateClientDto.FirstName;
                client.Lastname = updateClientDto.LastName;
                client.Email = updateClientDto.Email;
                client.Mobile = updateClientDto.Mobile;
                client.City = updateClientDto.City;
                client.Address = updateClientDto.Address;
                client.Monthlyprice = updateClientDto.MonthlyPrice;
                client.Status = updateClientDto.Status ?? client.Status;

                await _context.SaveChangesAsync();

                // Reload with relationships
                var updatedClient = await _context.Clients
                    .Include(c => c.Projects)
                    .Include(c => c.Invoices)
                    .FirstOrDefaultAsync(c => c.Clientid == clientId);

                var clientResponse = MapToClientResponseDto(updatedClient);

                _logger.LogInformation($"Client updated successfully: {client.Businessname} (ID: {client.Clientid})");
                return ApiResponse<ClientResponseDto>.SuccessResponse(
                    clientResponse, "Client updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating client with ID: {clientId}");
                return ApiResponse<ClientResponseDto>.ErrorResponse(
                    "An error occurred while updating the client", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteClientAsync(int clientId)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Projects)
                    .Include(c => c.Invoices)
                    .FirstOrDefaultAsync(c => c.Clientid == clientId);

                if (client == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        $"Client with ID {clientId} not found", 404);
                }

                // Check if client has projects
                if (client.Projects != null && client.Projects.Any())
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete client with existing projects. Delete projects first.", 400);
                }

                // Check if client has invoices
                if (client.Invoices != null && client.Invoices.Any())
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete client with existing invoices. Delete invoices first.", 400);
                }

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Client deleted successfully: {client.Businessname} (ID: {clientId})");
                return ApiResponse<bool>.SuccessResponse(true, "Client deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting client with ID: {clientId}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while deleting the client", 500);
            }
        }

        public async Task<ApiResponse<bool>> CheckEmailExistsAsync(string email, int? excludeClientId = null)
        {
            try
            {
                var query = _context.Clients.AsQueryable();

                if (excludeClientId.HasValue)
                    query = query.Where(c => c.Clientid != excludeClientId.Value);

                var exists = await query.AnyAsync(c => c.Email.ToLower() == email.ToLower());

                return ApiResponse<bool>.SuccessResponse(exists,
                    exists ? "Email already exists" : "Email is available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking email existence: {email}");
                return ApiResponse<bool>.ErrorResponse("Error checking email availability", 500);
            }
        }

        public async Task<ApiResponse<bool>> CheckMobileExistsAsync(string mobile, int? excludeClientId = null)
        {
            try
            {
                var query = _context.Clients.AsQueryable();

                if (excludeClientId.HasValue)
                    query = query.Where(c => c.Clientid != excludeClientId.Value);

                var exists = await query.AnyAsync(c => c.Mobile == mobile);

                return ApiResponse<bool>.SuccessResponse(exists,
                    exists ? "Mobile already exists" : "Mobile is available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking mobile existence: {mobile}");
                return ApiResponse<bool>.ErrorResponse("Error checking mobile availability", 500);
            }
        }

        // Helper method
        private ClientResponseDto MapToClientResponseDto(Client client)
        {
            return new ClientResponseDto
            {
                ClientId = client.Clientid,
                BusinessType = client.Businesstype,
                BusinessName = client.Businessname,
                FirstName = client.Firstname,
                LastName = client.Lastname,
                Email = client.Email,
                Mobile = client.Mobile,
                City = client.City,
                Address = client.Address,
                MonthlyPrice = client.Monthlyprice,
                Status = client.Status,
                ProjectCount = client.Projects?.Count ?? 0,
                InvoiceCount = client.Invoices?.Count ?? 0
            };
        }
    }
}
