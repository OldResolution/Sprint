using HotelMS.Auth.DTOs;
using HotelMS.Auth.Models;

namespace HotelMS.Auth.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}

public interface IUserService
{
    Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteAsync(int id);
}
