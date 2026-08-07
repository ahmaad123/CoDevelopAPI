using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse<RoleResponseDto>> CreateRoleAsync(CreateRoleDto createRoleDto);
        Task<ApiResponse<RoleResponseDto>> GetRoleByIdAsync(int roleId);
        Task<ApiResponse<List<RoleResponseDto>>> GetAllRolesAsync();
        Task<ApiResponse<List<RoleResponseDto>>> GetRolesByLevelAsync(int level);
        Task<ApiResponse<RoleResponseDto>> UpdateRoleAsync(int roleId, UpdateRoleDto updateRoleDto);
        Task<ApiResponse<bool>> DeleteRoleAsync(int roleId);

    }
}