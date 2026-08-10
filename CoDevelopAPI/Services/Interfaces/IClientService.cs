using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface IClientService
    {
        Task<ApiResponse<ClientResponseDto>> CreateClientAsync(CreateClientDto createClientDto);
        Task<ApiResponse<ClientResponseDto>> GetClientByIdAsync(int clientId);
        Task<ApiResponse<List<ClientResponseDto>>> GetAllClientsAsync();
        Task<ApiResponse<List<ClientResponseDto>>> GetActiveClientsAsync();
        Task<ApiResponse<List<ClientResponseDto>>> GetClientsByStatusAsync(string status);
        Task<ApiResponse<ClientResponseDto>> UpdateClientAsync(int clientId, UpdateClientDto updateClientDto);
        Task<ApiResponse<bool>> DeleteClientAsync(int clientId);
        Task<ApiResponse<bool>> CheckEmailExistsAsync(string email, int? excludeClientId = null);
        Task<ApiResponse<bool>> CheckMobileExistsAsync(string mobile, int? excludeClientId = null);
    }
}
