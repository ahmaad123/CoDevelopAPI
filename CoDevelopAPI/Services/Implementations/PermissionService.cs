using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(ApplicationDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<PermissionResponseDto>> CreatePermissionAsync(CreatePermissionDto createPermissionDto)
        {
            try
            {
                // Check if permission name already exists
                var permExists = await _context.Permissions
                    .AnyAsync(p => p.Permname.ToLower() == createPermissionDto.PermName.ToLower());

                if (permExists)
                {
                    return ApiResponse<PermissionResponseDto>.ErrorResponse(
                        $"Permission with name '{createPermissionDto.PermName}' already exists", 400);
                }

                // Validate category if provided
                if (createPermissionDto.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Categoryid == createPermissionDto.CategoryId.Value);

                    if (!categoryExists)
                    {
                        return ApiResponse<PermissionResponseDto>.ErrorResponse(
                            $"Category with ID {createPermissionDto.CategoryId} not found", 400);
                    }
                }

                // Create new permission
                var permission = new Permission
                {
                    Permname = createPermissionDto.PermName,
                    Categoryid = createPermissionDto.CategoryId,
                    Module = createPermissionDto.Module,
                    Action = createPermissionDto.Action,
                    Resource = createPermissionDto.Resource
                };

                await _context.Permissions.AddAsync(permission);
                await _context.SaveChangesAsync();

                // Load category information
                if (permission.Categoryid.HasValue)
                {
                    await _context.Entry(permission).Reference(p => p.Category).LoadAsync();
                }

                var permissionResponse = MapToPermissionResponseDto(permission);

                _logger.LogInformation($"Permission created successfully: {permission.Permname} (ID: {permission.Permid})");
                return ApiResponse<PermissionResponseDto>.SuccessResponse(
                    permissionResponse, "Permission created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return ApiResponse<PermissionResponseDto>.ErrorResponse(
                    "An error occurred while creating the permission", 500);
            }
        }

        public async Task<ApiResponse<PermissionResponseDto>> GetPermissionByIdAsync(int permId)
        {
            try
            {
                var permission = await _context.Permissions
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Permid == permId);

                if (permission == null)
                {
                    return ApiResponse<PermissionResponseDto>.ErrorResponse(
                        $"Permission with ID {permId} not found", 404);
                }

                var permissionResponse = MapToPermissionResponseDto(permission);
                return ApiResponse<PermissionResponseDto>.SuccessResponse(permissionResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving permission with ID: {permId}");
                return ApiResponse<PermissionResponseDto>.ErrorResponse(
                    "An error occurred while retrieving the permission", 500);
            }
        }

        public async Task<ApiResponse<List<PermissionResponseDto>>> GetAllPermissionsAsync()
        {
            try
            {
                var permissions = await _context.Permissions
                    .Include(p => p.Category)
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.Permname)
                    .ToListAsync();

                var permissionResponses = permissions.Select(MapToPermissionResponseDto).ToList();

                return ApiResponse<List<PermissionResponseDto>>.SuccessResponse(permissionResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all permissions");
                return ApiResponse<List<PermissionResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving permissions", 500);
            }
        }

        public async Task<ApiResponse<List<PermissionResponseDto>>> GetPermissionsByCategoryAsync(int categoryId)
        {
            try
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Categoryid == categoryId);
                if (!categoryExists)
                {
                    return ApiResponse<List<PermissionResponseDto>>.ErrorResponse(
                        $"Category with ID {categoryId} not found", 404);
                }

                var permissions = await _context.Permissions
                    .Include(p => p.Category)
                    .Where(p => p.Categoryid == categoryId)
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.Permname)
                    .ToListAsync();

                var permissionResponses = permissions.Select(MapToPermissionResponseDto).ToList();

                return ApiResponse<List<PermissionResponseDto>>.SuccessResponse(permissionResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving permissions for category ID: {categoryId}");
                return ApiResponse<List<PermissionResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving permissions", 500);
            }
        }

        public async Task<ApiResponse<List<PermissionResponseDto>>> GetPermissionsByModuleAsync(string module)
        {
            try
            {
                var permissions = await _context.Permissions
                    .Include(p => p.Category)
                    .Where(p => p.Module.ToLower() == module.ToLower())
                    .OrderBy(p => p.Permname)
                    .ToListAsync();

                var permissionResponses = permissions.Select(MapToPermissionResponseDto).ToList();

                return ApiResponse<List<PermissionResponseDto>>.SuccessResponse(permissionResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving permissions for module: {module}");
                return ApiResponse<List<PermissionResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving permissions", 500);
            }
        }

        public async Task<ApiResponse<PermissionResponseDto>> UpdatePermissionAsync(int permId, UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                var permission = await _context.Permissions
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Permid == permId);

                if (permission == null)
                {
                    return ApiResponse<PermissionResponseDto>.ErrorResponse(
                        $"Permission with ID {permId} not found", 404);
                }

                // Check if new name conflicts with existing permission
                var nameExists = await _context.Permissions
                    .AnyAsync(p => p.Permname.ToLower() == updatePermissionDto.PermName.ToLower()
                        && p.Permid != permId);

                if (nameExists)
                {
                    return ApiResponse<PermissionResponseDto>.ErrorResponse(
                        $"Permission name '{updatePermissionDto.PermName}' already exists", 400);
                }

                // Validate category if provided
                if (updatePermissionDto.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Categoryid == updatePermissionDto.CategoryId.Value);

                    if (!categoryExists)
                    {
                        return ApiResponse<PermissionResponseDto>.ErrorResponse(
                            $"Category with ID {updatePermissionDto.CategoryId} not found", 400);
                    }
                }

                // Update permission properties
                permission.Permname = updatePermissionDto.PermName;
                permission.Categoryid = updatePermissionDto.CategoryId;
                permission.Module = updatePermissionDto.Module;
                permission.Action = updatePermissionDto.Action;
                permission.Resource = updatePermissionDto.Resource;

                await _context.SaveChangesAsync();

                // Reload category if changed
                if (permission.Categoryid.HasValue)
                {
                    await _context.Entry(permission).Reference(p => p.Category).LoadAsync();
                }

                var permissionResponse = MapToPermissionResponseDto(permission);

                _logger.LogInformation($"Permission updated successfully: {permission.Permname} (ID: {permission.Permid})");
                return ApiResponse<PermissionResponseDto>.SuccessResponse(
                    permissionResponse, "Permission updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permission with ID: {permId}");
                return ApiResponse<PermissionResponseDto>.ErrorResponse(
                    "An error occurred while updating the permission", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeletePermissionAsync(int permId)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(permId);

                if (permission == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        $"Permission with ID {permId} not found", 404);
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Permission deleted successfully: {permission.Permname} (ID: {permId})");
                return ApiResponse<bool>.SuccessResponse(true, "Permission deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting permission with ID: {permId}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while deleting the permission", 500);
            }
        }

        public async Task<ApiResponse<bool>> CheckPermissionNameExistsAsync(string permName, int? excludePermId = null)
        {
            try
            {
                var query = _context.Permissions.AsQueryable();

                if (excludePermId.HasValue)
                {
                    query = query.Where(p => p.Permid != excludePermId.Value);
                }

                var exists = await query.AnyAsync(p => p.Permname.ToLower() == permName.ToLower());

                return ApiResponse<bool>.SuccessResponse(exists,
                    exists ? "Permission name already exists" : "Permission name is available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permission name existence: {permName}");
                return ApiResponse<bool>.ErrorResponse("Error checking permission name availability", 500);
            }
        }

        // Helper method to map Permission entity to PermissionResponseDto
        private PermissionResponseDto MapToPermissionResponseDto(Permission permission)
        {
            return new PermissionResponseDto
            {
                PermId = permission.Permid,
                PermName = permission.Permname,
                CategoryId = permission.Categoryid,
                CategoryName = permission.Category?.Categoryname,
                Module = permission.Module,
                Action = permission.Action,
                Resource = permission.Resource
            };
        }
    }
}