
using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Gateway.DTOs;
using CoffeeNChill.Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoffeeNChill.Gateway.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TableClient _userTableClient;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<object> _passwordHasher = new();

        private static readonly string[] AllowedRoles =
        {
            "Admin",
            "Staff",
            "Customer"
        };

        public AuthController(
            TableServiceClient tableServiceClient,
            IConfiguration configuration)
        {
            _userTableClient =
                tableServiceClient.GetTableClient("Users");

            _userTableClient.CreateIfNotExists();

            _configuration = configuration;
        }

        // REGISTER USER
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto dto)
        {
            if (!AllowedRoles.Contains(dto.Role,
                StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid user role.");
            }

            string role = AllowedRoles.First(r =>
                r.Equals(dto.Role, StringComparison.OrdinalIgnoreCase));

            string email = dto.Email.Trim().ToLowerInvariant();

            string hashedPassword =
                _passwordHasher.HashPassword(new object(), dto.Password);

            var userEntity = new UserEntity
            {
                PartitionKey = role,
                RowKey = email,
                PasswordHash = hashedPassword,
                FullName = dto.FullName.Trim()
            };

            try
            {
                await _userTableClient.AddEntityAsync(userEntity);

                return Ok(new
                {
                    message = "User registered successfully."
                });
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                return Conflict(new
                {
                    message = "An account with these details already exists."
                });
            }
        }

        // LOGIN USER
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
            if (!AllowedRoles.Contains(dto.Role,
                StringComparer.OrdinalIgnoreCase))
            {
                return Unauthorized("Invalid credentials.");
            }

            string role = AllowedRoles.First(r =>
                r.Equals(dto.Role, StringComparison.OrdinalIgnoreCase));

            string email = dto.Email.Trim().ToLowerInvariant();

            try
            {
                var response =
                    await _userTableClient.GetEntityAsync<UserEntity>(
                        role, email);

                var user = response.Value;

                var result = _passwordHasher.VerifyHashedPassword(
                    new object(),
                    user.PasswordHash,
                    dto.Password
                );

                if (result == PasswordVerificationResult.Failed)
                {
                    return Unauthorized("Invalid credentials.");
                }

                string token = GenerateJwtToken(user);

                return Ok(new
                {
                    message = "Login successful.",
                    token = token,
                    fullName = user.FullName,
                    role = user.PartitionKey
                });
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return Unauthorized("Invalid credentials.");
            }
        }

        // GENERATE JWT TOKEN
        private string GenerateJwtToken(UserEntity user)
        {
            var jwtSettings =
                _configuration.GetSection("JwtSettings");

            string secret = jwtSettings["Secret"]
                ?? throw new InvalidOperationException(
                    "JWT secret is missing.");

            string issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is missing.");

            string audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is missing.");

            byte[] key = Encoding.UTF8.GetBytes(secret);

            if (key.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT secret must be at least 32 bytes.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.RowKey),
                new Claim(ClaimTypes.Role, user.PartitionKey)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                Expires = DateTime.UtcNow.AddHours(2),

                Issuer = issuer,

                Audience = audience,

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
