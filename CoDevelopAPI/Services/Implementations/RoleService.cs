using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(ApplicationDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<RoleResponseDto>> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            try
            {
                // Check if role name already exists
                var roleExists = await _context.Roles
                    .AnyAsync(r => r.Rolename == createRoleDto.RoleName.ToLower());

                if (roleExists)
                {
                    return ApiResponse<RoleResponseDto>.ErrorResponse(
                        $"Role with name '{createRoleDto.RoleName}' already exists", 400);
                }

                // Create new role
                var role = new Role
                {
                    Rolename = createRoleDto.RoleName,
                    Level = createRoleDto.Level,
                    Rolecode = createRoleDto.RoleCode,
                    IsActive = createRoleDto.IsActive ?? 1,
                    Description = createRoleDto.Description
                };

                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();

                // Map to response DTO
                var roleResponse = await MapToRoleResponseDto(role);

                _logger.LogInformation($"Role created successfully: {role.Rolename} (ID: {role.Roleid})");
                return ApiResponse<RoleResponseDto>.SuccessResponse(
                    roleResponse, "Role created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return ApiResponse<RoleResponseDto>.ErrorResponse(
                    "An error occurred while creating the role", 500);
            }
        }

        public async Task<ApiResponse<RoleResponseDto>> GetRoleByIdAsync(int Roleid)
        {
            try
            {
                var role = await _context.Roles
                    //.Include(r => r.ParentRole)
                    //.Include(r => r.InverseParent)
                    .FirstOrDefaultAsync(r => r.Roleid == Roleid);

                if (role == null)
                {
                    return ApiResponse<RoleResponseDto>.ErrorResponse(
                        $"Role with ID {Roleid} not found", 404);
                }

                var roleResponse = await MapToRoleResponseDto(role);
                return ApiResponse<RoleResponseDto>.SuccessResponse(roleResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving role with ID: {Roleid}");
                return ApiResponse<RoleResponseDto>.ErrorResponse(
                    "An error occurred while retrieving the role", 500);
            }
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _context.Roles
                    //.Include(r => r.ParentRole)
                    //.Include(r => r.InverseParent)
                    .OrderBy(r => r.Level)
                    .ThenBy(r => r.Rolename)
                    .ToListAsync();

                var roleResponses = new List<RoleResponseDto>();
                foreach (var role in roles)
                {
                    roleResponses.Add(await MapToRoleResponseDto(role));
                }

                return ApiResponse<List<RoleResponseDto>>.SuccessResponse(roleResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                return ApiResponse<List<RoleResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving roles", 500);
            }
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> GetRolesByLevelAsync(int level)
        {
            try
            {
                var roles = await _context.Roles
                    //.Include(r => r.ParentRole)
                    //.Include(r => r.InverseParent)
                    .Where(r => r.Level == level)
                    .OrderBy(r => r.Rolename)
                    .ToListAsync();

                var roleResponses = new List<RoleResponseDto>();
                foreach (var role in roles)
                {
                    roleResponses.Add(await MapToRoleResponseDto(role));
                }

                return ApiResponse<List<RoleResponseDto>>.SuccessResponse(roleResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving roles for level: {level}");
                return ApiResponse<List<RoleResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving roles", 500);
            }
        }

        public async Task<ApiResponse<RoleResponseDto>> UpdateRoleAsync(int Roleid, UpdateRoleDto updateRoleDto)
        {
            try
            {
                var role = await _context.Roles.FindAsync(Roleid);
                if (role == null)
                {
                    return ApiResponse<RoleResponseDto>.ErrorResponse(
                        $"Role with ID {Roleid} not found", 404);
                }

                // Check if new name conflicts with existing role
                var nameExists = await _context.Roles
                    .AnyAsync(r => r.Rolename.ToLower() == updateRoleDto.RoleName.ToLower()
                        && r.Roleid != Roleid);

                if (nameExists)
                {
                    return ApiResponse<RoleResponseDto>.ErrorResponse(
                        $"Role name '{updateRoleDto.RoleName}' already exists", 400);
                }

                // Update properties
                role.Rolename = updateRoleDto.RoleName;
                role.Level = updateRoleDto.Level;
                role.Rolecode = updateRoleDto.RoleCode;
                role.IsActive = updateRoleDto.IsActive;
                role.Description = updateRoleDto.Description;

                await _context.SaveChangesAsync();

                var roleResponse = await MapToRoleResponseDto(role);
                _logger.LogInformation($"Role updated successfully: {role.Rolename} (ID: {role.Roleid})");

                return ApiResponse<RoleResponseDto>.SuccessResponse(
                    roleResponse, "Role updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating role with ID: {Roleid}");
                return ApiResponse<RoleResponseDto>.ErrorResponse(
                    "An error occurred while updating the role", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteRoleAsync(int Roleid)
        {
            try
            {
                var role = await _context.Roles
                    //.Include(r => r.InverseParent)
                    .FirstOrDefaultAsync(r => r.Roleid == Roleid);

                if (role == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        $"Role with ID {Roleid} not found", 404);
                }

                // Check if role has child roles
                //if (role.InverseParent != null && role.InverseParent.Any())
                //{
                //    return ApiResponse<bool>.ErrorResponse(
                //        "Cannot delete role with child roles. Reassign or delete child roles first.", 400);
                //}

                // Check if role is assigned to users
                var hasUsers = await _context.Userroles
                    .AnyAsync(ur => ur.Roleid == Roleid);

                if (hasUsers)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete role assigned to users. Reassign users first.", 400);
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Role deleted successfully: {role.Rolename} (ID: {Roleid})");
                return ApiResponse<bool>.SuccessResponse(true, "Role deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting role with ID: {Roleid}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while deleting the role", 500);
            }
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> GetChildRolesAsync(int parentRoleid)
        {
            try
            {
                var parentExists = await _context.Roles.AnyAsync(r => r.Roleid == parentRoleid);
                if (!parentExists)
                {
                    return ApiResponse<List<RoleResponseDto>>.ErrorResponse(
                        $"Parent role with ID {parentRoleid} not found", 404);
                }

                var childRoles = await _context.Roles
                    .OrderBy(r => r.Rolename)
                    .ToListAsync();

                var roleResponses = new List<RoleResponseDto>();
                foreach (var role in childRoles)
                {
                    roleResponses.Add(await MapToRoleResponseDto(role));
                }

                return ApiResponse<List<RoleResponseDto>>.SuccessResponse(roleResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving child roles for parent ID: {parentRoleid}");
                return ApiResponse<List<RoleResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving child roles", 500);
            }
        }

        public async Task<ApiResponse<bool>> RoleExistsAsync(string Rolename)
        {
            try
            {
                var exists = await _context.Roles
                    .AnyAsync(r => r.Rolename.ToLower() == Rolename.ToLower());

                return ApiResponse<bool>.SuccessResponse(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if role exists: {Rolename}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while checking role existence", 500);
            }
        }

        // Helper method to map Role entity to RoleResponseDto
        private async Task<RoleResponseDto> MapToRoleResponseDto(Role role)
        {
            return new RoleResponseDto
            {
                RoleId = role.Roleid,
                RoleName = role.Rolename,
                Level = role.Level,
                RoleCode = role.Rolecode,
                IsActive = role.IsActive,
                Description = role.Description,
            };
        }
    }
}