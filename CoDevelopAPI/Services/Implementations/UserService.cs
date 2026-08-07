using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;
using CoDevelopAPI.Services.Helpers;

namespace CoDevelopAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<UserService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<UserResponseDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == createUserDto.Email.ToLower());

                if (emailExists)
                {
                    return ApiResponse<UserResponseDto>.ErrorResponse(
                        $"User with email '{createUserDto.Email}' already exists", 400);
                }

                if (createUserDto.Phone.HasValue)
                {
                    var phoneExists = await _context.Users
                        .AnyAsync(u => u.Phone == createUserDto.Phone.Value);

                    if (phoneExists)
                    {
                        return ApiResponse<UserResponseDto>.ErrorResponse(
                            $"User with phone '{createUserDto.Phone.Value}' already exists", 400);
                    }
                }

                if (createUserDto.RoleId.HasValue)
                {
                    var roleExists = await _context.Roles
                        .AnyAsync(r => r.Roleid == createUserDto.RoleId.Value);

                    if (!roleExists)
                    {
                        return ApiResponse<UserResponseDto>.ErrorResponse(
                            $"Role with ID {createUserDto.RoleId} not found", 400);
                    }
                }

                var generatedPassword = PasswordHelper.GenerateRandomPassword(12);

                var passwordHash = PasswordHelper.HashPassword(generatedPassword);

                var user = new User
                {
                    Email = createUserDto.Email,
                    Passwordhash = passwordHash,
                    Firstname = createUserDto.FirstName,
                    Lastname = createUserDto.LastName,
                    Department = createUserDto.Department,
                    Phone = createUserDto.Phone,
                    IsActive = createUserDto.IsActive,
                    Roleid = createUserDto.RoleId
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                await _context.Entry(user).Reference(u => u.Role).LoadAsync();

                try
                {
                    await _emailService.SendPasswordEmailAsync(
                        user.Email,
                        $"{user.Firstname} {user.Lastname}",
                        generatedPassword);

                    _logger.LogInformation($"Password email sent to {user.Email}");
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, $"Failed to send password email to {user.Email}. User created but email not sent.");
                }

                var userResponse = MapToUserResponseDto(user);

                _logger.LogInformation($"User created successfully: {user.Email} (ID: {user.Userid})");
                return ApiResponse<UserResponseDto>.SuccessResponse(
                    userResponse, "User created successfully. Password has been sent to their email.", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return ApiResponse<UserResponseDto>.ErrorResponse(
                    "An error occurred while creating the user", 500);
            }
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                if (user == null)
                {
                    return ApiResponse<UserResponseDto>.ErrorResponse(
                        $"User with ID {userId} not found", 404);
                }

                var userResponse = MapToUserResponseDto(user);
                return ApiResponse<UserResponseDto>.SuccessResponse(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID: {userId}");
                return ApiResponse<UserResponseDto>.ErrorResponse(
                    "An error occurred while retrieving the user", 500);
            }
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .OrderBy(u => u.Firstname)
                    .ThenBy(u => u.Lastname)
                    .ToListAsync();

                var userResponses = users.Select(MapToUserResponseDto).ToList();

                return ApiResponse<List<UserResponseDto>>.SuccessResponse(userResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return ApiResponse<List<UserResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving users", 500);
            }
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetUsersByRoleAsync(int roleId)
        {
            try
            {
                // Check if role exists
                var roleExists = await _context.Roles.AnyAsync(r => r.Roleid == roleId);
                if (!roleExists)
                {
                    return ApiResponse<List<UserResponseDto>>.ErrorResponse(
                        $"Role with ID {roleId} not found", 404);
                }

                var users = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Roleid == roleId)
                    .OrderBy(u => u.Firstname)
                    .ThenBy(u => u.Lastname)
                    .ToListAsync();

                var userResponses = users.Select(MapToUserResponseDto).ToList();

                return ApiResponse<List<UserResponseDto>>.SuccessResponse(userResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving users for role ID: {roleId}");
                return ApiResponse<List<UserResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving users", 500);
            }
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.IsActive == true)
                    .OrderBy(u => u.Firstname)
                    .ThenBy(u => u.Lastname)
                    .ToListAsync();

                var userResponses = users.Select(MapToUserResponseDto).ToList();

                return ApiResponse<List<UserResponseDto>>.SuccessResponse(userResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users");
                return ApiResponse<List<UserResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving active users", 500);
            }
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetInactiveUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.IsActive == false)
                    .OrderBy(u => u.Firstname)
                    .ThenBy(u => u.Lastname)
                    .ToListAsync();

                var userResponses = users.Select(MapToUserResponseDto).ToList();

                return ApiResponse<List<UserResponseDto>>.SuccessResponse(userResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inactive users");
                return ApiResponse<List<UserResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving inactive users", 500);
            }
        }

        public async Task<ApiResponse<List<UserResponseDto>>> GetUsersByDepartmentAsync(string department)
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Department.ToLower() == department.ToLower())
                    .OrderBy(u => u.Firstname)
                    .ThenBy(u => u.Lastname)
                    .ToListAsync();

                var userResponses = users.Select(MapToUserResponseDto).ToList();

                return ApiResponse<List<UserResponseDto>>.SuccessResponse(userResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving users for department: {department}");
                return ApiResponse<List<UserResponseDto>>.ErrorResponse(
                    "An error occurred while retrieving users", 500);
            }
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Userid == userId);

                if (user == null)
                {
                    return ApiResponse<UserResponseDto>.ErrorResponse(
                        $"User with ID {userId} not found", 404);
                }

                // Check if new email conflicts with existing user
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == updateUserDto.Email.ToLower()
                        && u.Userid != userId);

                if (emailExists)
                {
                    return ApiResponse<UserResponseDto>.ErrorResponse(
                        $"Email '{updateUserDto.Email}' is already in use", 400);
                }
                var phoneExists = await _context.Users
                    .AnyAsync(u => u.Phone == updateUserDto.Phone
                        && u.Userid != userId);

                if (phoneExists)
                {
                    return ApiResponse<UserResponseDto>.ErrorResponse(
                        $"Phone Number '{updateUserDto.Phone}' is already in use", 400);
                }

                // Validate role if provided
                if (updateUserDto.RoleId.HasValue)
                {
                    var roleExists = await _context.Roles
                        .AnyAsync(r => r.Roleid == updateUserDto.RoleId.Value);

                    if (!roleExists)
                    {
                        return ApiResponse<UserResponseDto>.ErrorResponse(
                            $"Role with ID {updateUserDto.RoleId} not found", 400);
                    }
                }

                // Update user properties
                user.Email = updateUserDto.Email;
                user.Firstname = updateUserDto.FirstName;
                user.Lastname = updateUserDto.LastName;
                user.Department = updateUserDto.Department;
                user.Phone = updateUserDto.Phone;
                user.IsActive = updateUserDto.IsActive;
                user.Roleid = updateUserDto.RoleId;

                await _context.SaveChangesAsync();

                // Reload role if changed
                if (user.Roleid != null)
                {
                    await _context.Entry(user).Reference(u => u.Role).LoadAsync();
                }

                var userResponse = MapToUserResponseDto(user);

                _logger.LogInformation($"User updated successfully: {user.Email} (ID: {user.Userid})");
                return ApiResponse<UserResponseDto>.SuccessResponse(
                    userResponse, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID: {userId}");
                return ApiResponse<UserResponseDto>.ErrorResponse(
                    "An error occurred while updating the user", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        $"User with ID {userId} not found", 404);
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User deleted successfully: {user.Email} (ID: {userId})");
                return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID: {userId}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while deleting the user", 500);
            }
        }

        public async Task<ApiResponse<bool>> ToggleUserStatusAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        $"User with ID {userId} not found", 404);
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                var status = (bool)user.IsActive ? "activated" : "deactivated";
                _logger.LogInformation($"User {status}: {user.Email} (ID: {userId})");

                return ApiResponse<bool>.SuccessResponse(
                    true, $"User {status} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling status for user ID: {userId}");
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while updating user status", 500);
            }
        }

        private UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.Userid,
                Email = user.Email,
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Department = user.Department,
                Phone = user.Phone,
                IsActive = user.IsActive ?? true,
                RoleName = user.Role?.Rolename
            };
        }
    }
}