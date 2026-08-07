using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;
        private readonly ILogger<RolePermissionsController> _logger;

        public RolePermissionsController(
            IRolePermissionService rolePermissionService,
            ILogger<RolePermissionsController> logger)
        {
            _rolePermissionService = rolePermissionService;
            _logger = logger;
        }

        /// <summary>
        /// Get permission summary for all roles
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<List<RolePermissionSummaryDto>>), 200)]
        public async Task<IActionResult> GetAllRolePermissionsSummary()
        {
            var result = await _rolePermissionService.GetAllRolePermissionsSummaryAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        [HttpGet("role/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionResponseDto>), 404)]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var result = await _rolePermissionService.GetRolePermissionsAsync(roleId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get permissions grouped by category for a specific role
        /// </summary>
        [HttpGet("role/{roleId}/categorized")]
        [ProducesResponseType(typeof(ApiResponse<CategorizedPermissionsResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<CategorizedPermissionsResponseDto>), 404)]
        public async Task<IActionResult> GetRolePermissionsByCategory(int roleId)
        {
            var result = await _rolePermissionService.GetRolePermissionsByCategoryAsync(roleId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Assign permissions to a role (replaces all existing permissions)
        /// </summary>
        [HttpPut("role/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionResponseDto>), 404)]
        public async Task<IActionResult> AssignPermissionsToRole(
            int roleId,
            [FromBody] AssignPermissionsDto assignPermissionsDto)
        {
            if (assignPermissionsDto == null)
            {
                return BadRequest(ApiResponse<RolePermissionResponseDto>.ErrorResponse(
                    "Permission IDs are required", 400));
            }

            var result = await _rolePermissionService.AssignPermissionsToRoleAsync(roleId, assignPermissionsDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Remove all permissions from a role
        /// </summary>
        [HttpDelete("role/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> RemoveAllPermissionsFromRole(int roleId)
        {
            var result = await _rolePermissionService.RemoveAllPermissionsFromRoleAsync(roleId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}