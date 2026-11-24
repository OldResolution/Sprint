using FluentAssertions;
using HotelMS.Auth.DTOs;
using HotelMS.Auth.Exceptions;
using HotelMS.Auth.Models;
using HotelMS.Auth.Models.Enums;
using HotelMS.Auth.Repositories.Interfaces;
using HotelMS.Auth.Services.Classes;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace HotelMS.Auth.Tests;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepoMock = null!;
    private IConfiguration _configuration = null!;
    private TokenService _tokenService = null!;
    private UserService _userService = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepoMock = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "HotelManagementSystemSecretKey2026_SecureMinimum32CharsLength!" },
            { "JwtSettings:Issuer", "HotelMS" },
            { "JwtSettings:Audience", "HotelMSClients" },
            { "JwtSettings:ExpiryInMinutes", "60" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenService = new TokenService(_configuration);
        _userService = new UserService(_userRepoMock.Object, _tokenService);
    }

    [Test]
    public async Task TC01_Login_WithValidCredentials_ReturnsLoginResponseWithValidToken()
    {
        // Arrange
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var user = new User
        {
            UserId = 1,
            Username = "admin",
            Role = UserRole.Owner
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");

        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(user);

        var request = new LoginRequestDto { Username = "admin", Password = "Password123!" };

        // Act
        var result = await _userService.AuthenticateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("admin");
        result.Role.Should().Be("Owner");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task TC02_Login_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var user = new User
        {
            UserId = 1,
            Username = "admin",
            Role = UserRole.Owner
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "CorrectPassword!");

        _userRepoMock.Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(user);

        var request = new LoginRequestDto { Username = "admin", Password = "WrongPassword!" };

        // Act
        var act = async () => await _userService.AuthenticateAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid username or password.");
    }

    [Test]
    public async Task TC02_Login_WithNonExistentUser_ThrowsUnauthorizedException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByUsernameAsync("ghost"))
            .ReturnsAsync((User?)null);

        var request = new LoginRequestDto { Username = "ghost", Password = "Password123!" };

        // Act
        var act = async () => await _userService.AuthenticateAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid username or password.");
    }

    [Test]
    public async Task TC03_CreateUser_WithDuplicateUsername_ThrowsConflictException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.ExistsByUsernameAsync("existingUser"))
            .ReturnsAsync(true);

        var request = new CreateUserDto { Username = "existingUser", Password = "Password123!", Role = "Receptionist" };

        // Act
        var act = async () => await _userService.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Username 'existingUser' already exists.");
    }

    [Test]
    public async Task TC17_CreateUser_HashesPasswordSecurely()
    {
        // Arrange
        _userRepoMock.Setup(r => r.ExistsByUsernameAsync("newUser"))
            .ReturnsAsync(false);

        User? capturedUser = null;
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        var request = new CreateUserDto { Username = "newUser", Password = "PlainTextPassword123!", Role = "Manager" };

        // Act
        var result = await _userService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe("PlainTextPassword123!");
        capturedUser.PasswordHash.Length.Should().BeGreaterThan(20);
    }

    [Test]
    public async Task TC18_UpdateUser_UpdatesRoleAndPasswordSuccessfully()
    {
        // Arrange
        var user = new User
        {
            UserId = 10,
            Username = "user10",
            Role = UserRole.Receptionist,
            PasswordHash = "initialHash"
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var updateDto = new UpdateUserDto { Role = "Manager", Password = "NewStrongPassword123!" };

        // Act
        var result = await _userService.UpdateAsync(10, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("Manager");
        user.Role.Should().Be(UserRole.Manager);
        user.PasswordHash.Should().NotBe("initialHash");
    }
}
