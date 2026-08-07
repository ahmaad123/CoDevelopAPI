using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user with optional role assignment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 400)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _userService.CreateUserAsync(createUserDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(
                nameof(GetUserById),
                new { userId = result.Data.UserId },
                result);
        }

        /// <summary>
        /// Retrieves all users with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserResponseDto>>), 200)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int? roleId = null,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? department = null)
        {
            if (activeOnly == true)
            {
                var result = await _userService.GetActiveUsersAsync();
                return Ok(result);
            }

            if (activeOnly == false)
            {
                var result = await _userService.GetInactiveUsersAsync();
                return Ok(result);
            }

            if (roleId.HasValue)
            {
                var result = await _userService.GetUsersByRoleAsync(roleId.Value);
                return Ok(result);
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                var result = await _userService.GetUsersByDepartmentAsync(department);
                return Ok(result);
            }

            var allUsers = await _userService.GetAllUsersAsync();
            return Ok(allUsers);
        }

        /// <summary>
        /// Retrieves a specific user by ID
        /// </summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 404)]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var result = await _userService.GetUserByIdAsync(userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        [HttpPut("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), 404)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _userService.UpdateUserAsync(userId, updateUserDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        [HttpDelete("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var result = await _userService.DeleteUserAsync(userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Toggles user active/inactive status
        /// </summary>
        [HttpPatch("{userId}/toggle-status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            var result = await _userService.ToggleUserStatusAsync(userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}