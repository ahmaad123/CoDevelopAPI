using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Helpers;
using CoDevelopAPI.Services.Interfaces;
using CoDevelopAPI.Models.Entities;

namespace CoDevelopAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email with role and permissions
                var user = await _context.Users
                    .Include(u => u.Role)
                        .ThenInclude(r => r.Rolepermissions)
                            .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

                // Check if user exists
                if (user == null)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResponse(
                        "Invalid email or password", 401);
                }

                // Check if user is active
                if (user.IsActive == false)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResponse(
                        "Your account has been deactivated. Please contact administrator.", 401);
                }

                // Verify password
                if (!PasswordHelper.VerifyPassword(loginDto.Password, user.Passwordhash))
                {
                    return ApiResponse<LoginResponseDto>.ErrorResponse(
                        "Invalid email or password", 401);
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

                // Get user permissions
                var permissions = user.Role?.Rolepermissions
                    ?.Where(rp => rp.Isallowed == true)
                    ?.Select(rp => rp.Permission?.Permname)
                    ?.Where(p => p != null)
                    ?.ToList() ?? new List<string>();

                // Build response
                var loginResponse = new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserDetailDto
                    {
                        UserId = user.Userid,
                        Email = user.Email,
                        FirstName = user.Firstname,
                        LastName = user.Lastname,
                        Department = user.Department,
                        Phone = user.Phone,
                        IsActive = user.IsActive ?? true,
                        RoleName = user.Role?.Rolename
                    }
                };

                _logger.LogInformation($"User logged in successfully: {user.Email} (ID: {user.Userid})");
                return ApiResponse<LoginResponseDto>.SuccessResponse(
                    loginResponse, "Login successful", 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login failed for email: {loginDto.Email}");
                return ApiResponse<LoginResponseDto>.ErrorResponse(
                    "An error occurred during login", 500);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.Firstname),
                new Claim(ClaimTypes.Surname, user.Lastname),
                new Claim("FullName", $"{user.Firstname} {user.Lastname}"),
                new Claim("Role", user.Role?.Rolename ?? "No Role"),
            };

            // Add permission claims
            var permissions = user.Role?.Rolepermissions
                ?.Where(rp => rp.Isallowed == true)
                ?.Select(rp => rp.Permission?.Permname)
                ?.Where(p => p != null)
                ?? new List<string>();

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}