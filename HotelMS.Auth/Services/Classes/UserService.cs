using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelMS.Auth.DTOs;
using HotelMS.Auth.Exceptions;
using HotelMS.Auth.Models;
using HotelMS.Auth.Models.Enums;
using HotelMS.Auth.Repositories.Interfaces;
using HotelMS.Auth.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HotelMS.Auth.Services.Classes;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var secret = _config["JwtSettings:Secret"] ?? "HotelManagementSystemSecretKey2026_SecureMinimum32CharsLength!";
        var issuer = _config["JwtSettings:Issuer"] ?? "HotelMS";
        var audience = _config["JwtSettings:Audience"] ?? "HotelMSClients";

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("role", user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60), // 60-minute expiry per spec
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserService(IUserRepository repo, ITokenService tokenService)
    {
        _repo = repo;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request)
    {
        var user = await _repo.GetByUsernameAsync(request.Username.Trim());
        if (user == null)
            throw new UnauthorizedException("Invalid username or password.");

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Invalid username or password.");

        var token = _tokenService.GenerateToken(user);

        return new LoginResponseDto
        {
            Message = "Login successful",
            Token = token,
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _repo.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException($"User with ID {id} was not found.");
        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var exists = await _repo.ExistsByUsernameAsync(dto.Username.Trim());
        if (exists) throw new ConflictException($"Username '{dto.Username}' already exists.");

        if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            throw new ValidationException("Invalid role. Allowed values: Owner, Manager, Receptionist.");

        var user = new User
        {
            Username = dto.Username.Trim(),
            Role = role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        var created = await _repo.CreateAsync(user);
        return MapToDto(created);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException($"User with ID {id} was not found.");

        if (!string.IsNullOrEmpty(dto.Role))
        {
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
                throw new ValidationException("Invalid role. Allowed values: Owner, Manager, Receptionist.");
            user.Role = role;
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        }

        var updated = await _repo.UpdateAsync(user);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException($"User with ID {id} was not found.");
        return await _repo.DeleteAsync(id);
    }

    private static UserDto MapToDto(User u) => new()
    {
        UserId = u.UserId,
        Username = u.Username,
        Role = u.Role.ToString()
    };
}
