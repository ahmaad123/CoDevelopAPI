using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new permission
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), 400)]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto createPermissionDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<PermissionResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _permissionService.CreatePermissionAsync(createPermissionDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(
                nameof(GetPermissionById),
                new { permId = result.Data.PermId },
                result);
        }

        /// <summary>
        /// Retrieves all permissions with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionResponseDto>>), 200)]
        public async Task<IActionResult> GetAllPermissions(
            [FromQuery] int? categoryId = null,
            [FromQuery] string? module = null)
        {
            if (categoryId.HasValue)
            {
                var result = await _permissionService.GetPermissionsByCategoryAsync(categoryId.Value);
                return Ok(result);
            }

            if (!string.IsNullOrWhiteSpace(module))
            {
                var result = await _permissionService.GetPermissionsByModuleAsync(module);
                return Ok(result);
            }

            var allPermissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(allPermissions);
        }

        /// <summary>
        /// Retrieves a specific permission by ID
        /// </summary>
        [HttpGet("{permId}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), 404)]
        public async Task<IActionResult> GetPermissionById(int permId)
        {
            var result = await _permissionService.GetPermissionByIdAsync(permId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing permission
        /// </summary>
        [HttpPut("{permId}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponseDto>), 404)]
        public async Task<IActionResult> UpdatePermission(int permId, [FromBody] UpdatePermissionDto updatePermissionDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<PermissionResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _permissionService.UpdatePermissionAsync(permId, updatePermissionDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a permission
        /// </summary>
        [HttpDelete("{permId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> DeletePermission(int permId)
        {
            var result = await _permissionService.DeletePermissionAsync(permId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Check if permission name exists
        /// </summary>
        [HttpGet("check-name")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> CheckPermissionNameExists(
            [FromQuery] string permName,
            [FromQuery] int? excludePermId = null)
        {
            var result = await _permissionService.CheckPermissionNameExistsAsync(permName, excludePermId);
            return Ok(result);
        }
    }
}