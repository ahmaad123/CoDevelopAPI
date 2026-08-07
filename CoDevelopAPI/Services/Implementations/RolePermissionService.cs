using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolePermissionService> _logger;

        public RolePermissionService(ApplicationDbContext context, ILogger<RolePermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<RolePermissionSummaryDto>>> GetAllRolePermissionsSummaryAsync()
        {
            try
            {
                var totalPermissions = await _context.Permissions.CountAsync();

                var roles = await _context.Roles
                    .Include(r => r.Rolepermissions)
                    .OrderBy(r => r.Rolename)
                    .ToListAsync();

                var summaries = roles.Select(role =>
                {
                    var count = role.Rolepermissions?.Count ?? 0;
                    return new RolePermissionSummaryDto
                    {
                        RoleId = role.Roleid,
                        RoleName = role.Rolename,
                        Description = role.Description,
                        PermissionCount = count,
                        TotalPermissions = totalPermissions,
                        Percentage = totalPermissions > 0
                            ? (int)Math.Round((double)count / totalPermissions * 100)
                            : 0
                    };
                }).ToList();

                return ApiResponse<List<RolePermissionSummaryDto>>.SuccessResponse(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role permissions summary");
                return ApiResponse<List<RolePermissionSummaryDto>>.ErrorResponse(
                    "An error occurred while retrieving role permissions", 500);
            }
        }

        public async Task<ApiResponse<RolePermissionResponseDto>> GetRolePermissionsAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Rolepermissions)
                        .ThenInclude(rp => rp.Permission)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(r => r.Roleid == roleId);

                if (role == null)
                {
                    return ApiResponse<RolePermissionResponseDto>.ErrorResponse(
                        $"Role with ID {roleId} not found", 404);
                }

                var totalPermissions = await _context.Permissions.CountAsync();
                var assignedCount = role.Rolepermissions?.Count ?? 0;

                var response = new RolePermissionResponseDto
                {
                    RoleId = role.Roleid,
                    RoleName = role.Rolename,
                    Description = role.Description,
                    PermissionCount = assignedCount,
                    TotalPermissions = totalPermissions,
                    Percentage = totalPermissions > 0
                        ? (int)Math.Round((double)assignedCount / totalPermissions * 100)
                        : 0,
                    Permissions = role.Rolepermissions?.Select(rp => new PermissionDto
                    {
                        PermId = rp.Permissionid,
                        PermName = rp.Permission?.Permname,
                        CategoryId = rp.Permission?.Categoryid,
                        CategoryName = rp.Permission?.Category?.Categoryname,
                        Module = rp.Permission?.Module,
                        Action = rp.Permission?.Action,
                        Resource = rp.Permission?.Resource,
                        IsAssigned = true
                    }).ToList() ?? new List<PermissionDto>()
                };

                return ApiResponse<RolePermissionResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving permissions for role ID: {roleId}");
                return ApiResponse<RolePermissionResponseDto>.ErrorResponse(
                    "An error occurred while retrieving role permissions", 500);
            }
        }

        public async Task<ApiResponse<CategorizedPermissionsResponseDto>> GetRolePermissionsByCategoryAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Rolepermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Roleid == roleId);

                if (role == null)
                {
                    return ApiResponse<CategorizedPermissionsResponseDto>.ErrorResponse(
                        $"Role with ID {roleId} not found", 404);
                }

                // Get all categories with their permissions
                var categories = await _context.Categories
                    .Include(c => c.Permissions)
                    .OrderBy(c => c.Categoryname)
                    .ToListAsync();

                var assignedPermIds = role.Rolepermissions?.Select(rp => rp.Permissionid).ToList() ?? new List<int>();

                var categoryDtos = categories.Select(category =>
                {
                    var catPermissions = category.Permissions?.ToList() ?? new List<Permission>();

                    return new CategoryPermissionsDto
                    {
                        CategoryName = category.Categoryname,
                        TotalInCategory = catPermissions.Count,
                        AssignedInCategory = catPermissions.Count(p => assignedPermIds.Contains(p.Permid)),
                        AllChecked = catPermissions.Any() && catPermissions.All(p => assignedPermIds.Contains(p.Permid)),
                        Permissions = catPermissions.Select(p => new PermissionDto
                        {
                            PermId = p.Permid,
                            PermName = p.Permname,
                            CategoryId = p.Categoryid,
                            CategoryName = category.Categoryname,
                            Module = p.Module,
                            Action = p.Action,
                            Resource = p.Resource,
                            IsAssigned = assignedPermIds.Contains(p.Permid)
                        }).ToList()
                    };
                })
                .Where(c => c.Permissions.Any())
                .ToList();

                var response = new CategorizedPermissionsResponseDto
                {
                    RoleId = role.Roleid,
                    RoleName = role.Rolename,
                    Categories = categoryDtos
                };

                return ApiResponse<CategorizedPermissionsResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving categorized permissions for role ID: {roleId}");
                return ApiResponse<CategorizedPermissionsResponseDto>.ErrorResponse(
                    "An error occurred while retrieving role permissions", 500);
            }
        }

        public async Task<ApiResponse<RolePermissionResponseDto>> AssignPermissionsToRoleAsync(
    int roleId,
    AssignPermissionsDto assignPermissionsDto)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Rolepermissions)
                    .FirstOrDefaultAsync(r => r.Roleid == roleId);

                if (role == null)
                {
                    return ApiResponse<RolePermissionResponseDto>.ErrorResponse(
                        $"Role with ID {roleId} not found", 404);
                }

                // Validate permission IDs
                if (assignPermissionsDto.Permissions.Any())
                {
                    var requestedPermIds = assignPermissionsDto.Permissions
                        .Select(p => p.PermissionId)
                        .ToList();

                    var validPermIds = await _context.Permissions
                        .Where(p => requestedPermIds.Contains(p.Permid))
                        .Select(p => p.Permid)
                        .ToListAsync();

                    var invalidIds = requestedPermIds.Except(validPermIds).ToList();
                    if (invalidIds.Any())
                    {
                        return ApiResponse<RolePermissionResponseDto>.ErrorResponse(
                            $"Invalid permission IDs: {string.Join(", ", invalidIds)}", 400);
                    }
                }

                // Remove existing permissions
                var existingPermissions = await _context.Rolepermissions
                    .Where(rp => rp.Roleid == roleId)
                    .ToListAsync();
                _context.Rolepermissions.RemoveRange(existingPermissions);

                // Add new permissions with isAllowed value
                if (assignPermissionsDto.Permissions.Any())
                {
                    foreach (var permAssignment in assignPermissionsDto.Permissions)
                    {
                        var rolePermission = new Rolepermission
                        {
                            Roleid = roleId,
                            Permissionid = permAssignment.PermissionId,
                            Isallowed = permAssignment.IsAllowed
                        };
                        await _context.Rolepermissions.AddAsync(rolePermission);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Permissions updated for role: {role.Rolename} (ID: {roleId})");

                return await GetRolePermissionsAsync(roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning permissions to role ID: {roleId}");
                return ApiResponse<RolePermissionResponseDto>.ErrorResponse(
                    "An error occurred while assigning permissions", 500);
            }
        }

        public async Task<ApiResponse<bool>> RemoveAllPermissionsFromRoleAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Rolepermissions)
                    .FirstOrDefaultAsync(r => r.Roleid == roleId);

                if (role == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        $"Role with ID {roleId} not found", 404);
                }

                if (role.Rolepermissions != null && role.Rolepermissions.Any())
                {
                    _context.Rolepermissions.RemoveRange(role.Rolepermissions);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"All permissions removed from role: {role.Rolename} (ID: {roleId})");
                return ApiResponse<bool>.SuccessResponse(true, "All permissions removed from role successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing permissions from role ID: {roleId}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while removing permissions", 500);
            }
        }
    }
}