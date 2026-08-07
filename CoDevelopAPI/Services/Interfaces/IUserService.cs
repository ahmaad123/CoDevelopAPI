using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserResponseDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId);
        Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync();
        Task<ApiResponse<List<UserResponseDto>>> GetUsersByRoleAsync(int roleId);
        Task<ApiResponse<List<UserResponseDto>>> GetActiveUsersAsync();
        Task<ApiResponse<List<UserResponseDto>>> GetInactiveUsersAsync();
        Task<ApiResponse<List<UserResponseDto>>> GetUsersByDepartmentAsync(string department);
        Task<ApiResponse<UserResponseDto>> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<ApiResponse<bool>> DeleteUserAsync(int userId);
        Task<ApiResponse<bool>> ToggleUserStatusAsync(int userId);
    }
}