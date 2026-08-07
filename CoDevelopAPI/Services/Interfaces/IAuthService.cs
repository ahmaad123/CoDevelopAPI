using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    }
}