using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<ApiResponse<PermissionResponseDto>> CreatePermissionAsync(CreatePermissionDto createPermissionDto);
        Task<ApiResponse<PermissionResponseDto>> GetPermissionByIdAsync(int permId);
        Task<ApiResponse<List<PermissionResponseDto>>> GetAllPermissionsAsync();
        Task<ApiResponse<List<PermissionResponseDto>>> GetPermissionsByCategoryAsync(int categoryId);
        Task<ApiResponse<List<PermissionResponseDto>>> GetPermissionsByModuleAsync(string module);
        Task<ApiResponse<PermissionResponseDto>> UpdatePermissionAsync(int permId, UpdatePermissionDto updatePermissionDto);
        Task<ApiResponse<bool>> DeletePermissionAsync(int permId);
        Task<ApiResponse<bool>> CheckPermissionNameExistsAsync(string permName, int? excludePermId = null);
    }
}