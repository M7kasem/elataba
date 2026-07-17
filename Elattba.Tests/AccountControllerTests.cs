using ElAtaba.Domain.Entities;
using Elattaba.API.Controllers;
using Elattaba.API.Helper;
using Elattba.Application.Auth;
using Elattba.Core.DTOs;
using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elattba.Tests;

public class FakeUserManager : UserManager<AppUser>
{
    public AppUser? UserToReturn { get; set; }
    public string TokenToReturn { get; set; } = "valid-token";
    public IdentityResult ResetPasswordResult { get; set; } = IdentityResult.Success;
    public bool GenerateCalled { get; private set; }

    public FakeUserManager() : base(new FakeUserStore(), null, null, null, null, null, null, null, null)
    {
    }

    public override Task<AppUser?> FindByEmailAsync(string email)
    {
        return Task.FromResult(UserToReturn);
    }

    public override Task<string> GeneratePasswordResetTokenAsync(AppUser user)
    {
        GenerateCalled = true;
        return Task.FromResult(TokenToReturn);
    }

    public override Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
    {
        return Task.FromResult(ResetPasswordResult);
    }
}

public class FakeUserStore : IUserStore<AppUser>
{
    public Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public void Dispose() {}
    public Task<AppUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<AppUser?>(null);
    public Task<AppUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<AppUser?>(null);
    public Task<string?> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
    public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
    public Task<string?> GetUserNameAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task SetNormalizedUserNameAsync(AppUser user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task SetUserNameAsync(AppUser user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
}

public class FakeEmailSender : IEmailSender
{
    public bool SendCalled { get; private set; }
    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        SendCalled = true;
        return Task.CompletedTask;
    }
}

public class FakeConfiguration : IConfiguration
{
    public string? this[string key] { get => null; set {} }
    public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
    public Microsoft.Extensions.Primitives.IChangeToken GetReloadToken() => null!;
    public IConfigurationSection GetSection(string key) => null!;
}

public class AccountControllerTests
{
    private AccountController CreateController(
        FakeUserManager userManager,
        FakeEmailSender emailSender)
    {
        return new AccountController(
            null!, // IUserProvisioningService
            userManager,
            null!, // IUnitOfWork
            null!, // ITokenService
            emailSender,
            new FakeConfiguration()
        );
    }

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_GeneratesTokenAndCallsEmailSender_ReturnsGenericMessage()
    {
        var userManager = new FakeUserManager { UserToReturn = new AppUser { Email = "test@test.com" } };
        var emailSender = new FakeEmailSender();
        var controller = CreateController(userManager, emailSender);

        var result = await controller.ForgotPassword(new ForgotPasswordDto("test@test.com"));

        Assert.True(userManager.GenerateCalled);
        Assert.True(emailSender.SendCalled);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseAPI>(okResult.Value);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("If an account with that email exists, a reset link has been sent.", response.Message);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_DoesNotGenerateTokenOrSendEmail_ReturnsSameGenericMessage()
    {
        var userManager = new FakeUserManager { UserToReturn = null };
        var emailSender = new FakeEmailSender();
        var controller = CreateController(userManager, emailSender);

        var result = await controller.ForgotPassword(new ForgotPasswordDto("no@test.com"));

        Assert.False(userManager.GenerateCalled);
        Assert.False(emailSender.SendCalled);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseAPI>(okResult.Value);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("If an account with that email exists, a reset link has been sent.", response.Message);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ReturnsSuccess()
    {
        var userManager = new FakeUserManager { UserToReturn = new AppUser() };
        var controller = CreateController(userManager, new FakeEmailSender());

        var result = await controller.ResetPassword(new ResetPasswordDto("test@test.com", "valid-token", "newpass"));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseAPI>(okResult.Value);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Password has been reset successfully.", response.Message);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidEmail_ReturnsBadRequest()
    {
        var userManager = new FakeUserManager { UserToReturn = null };
        var controller = CreateController(userManager, new FakeEmailSender());

        var result = await controller.ResetPassword(new ResetPasswordDto("wrong@test.com", "token", "newpass"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ResponseAPI>(badRequest.Value);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Invalid request.", response.Message);
    }

    [Fact]
    public async Task ResetPassword_WithFailedResult_ReturnsBadRequestWithErrors()
    {
        var userManager = new FakeUserManager 
        { 
            UserToReturn = new AppUser(),
            ResetPasswordResult = IdentityResult.Failed(new IdentityError { Description = "Token invalid" })
        };
        var controller = CreateController(userManager, new FakeEmailSender());

        var result = await controller.ResetPassword(new ResetPasswordDto("test@test.com", "invalid", "newpass"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ResponseAPI>(badRequest.Value);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Token invalid", response.Message);
    }
}
