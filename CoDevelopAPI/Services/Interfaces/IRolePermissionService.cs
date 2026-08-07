using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface IRolePermissionService
    {
        Task<ApiResponse<List<RolePermissionSummaryDto>>> GetAllRolePermissionsSummaryAsync();
        Task<ApiResponse<RolePermissionResponseDto>> GetRolePermissionsAsync(int roleId);
        Task<ApiResponse<CategorizedPermissionsResponseDto>> GetRolePermissionsByCategoryAsync(int roleId);
        Task<ApiResponse<RolePermissionResponseDto>> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsDto assignPermissionsDto);
        Task<ApiResponse<bool>> RemoveAllPermissionsFromRoleAsync(int roleId);
    }
}