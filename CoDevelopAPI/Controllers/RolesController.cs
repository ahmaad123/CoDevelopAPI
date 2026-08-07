using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new role
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), 400)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<RoleResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _roleService.CreateRoleAsync(createRoleDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(
                nameof(GetRoleById),
                new { roleId = result.Data.RoleId },
                result);
        }

        /// <summary>
        /// Retrieves all roles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RoleResponseDto>>), 200)]
        public async Task<IActionResult> GetAllRoles([FromQuery] int? level = null)
        {
            if (level.HasValue)
            {
                var result = await _roleService.GetRolesByLevelAsync(level.Value);
                return Ok(result);
            }

            var allRoles = await _roleService.GetAllRolesAsync();
            return Ok(allRoles);
        }

        /// <summary>
        /// Retrieves a specific role by ID
        /// </summary>
        [HttpGet("{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), 404)]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            var result = await _roleService.GetRoleByIdAsync(roleId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing role
        /// </summary>
        [HttpPut("{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<RoleResponseDto>), 404)]
        public async Task<IActionResult> UpdateRole(int roleId, [FromBody] UpdateRoleDto updateRoleDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<RoleResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _roleService.UpdateRoleAsync(roleId, updateRoleDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a role
        /// </summary>
        [HttpDelete("{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var result = await _roleService.DeleteRoleAsync(roleId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

       
    }
}