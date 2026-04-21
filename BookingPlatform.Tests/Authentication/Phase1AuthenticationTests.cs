using BookingPlatform.Application.Authentication.Commands.Login;
using BookingPlatform.Application.Authentication.Commands.Register;
using BookingPlatform.Application.Authentication.Commands.UpdateCurrentUser;
using BookingPlatform.Application.Authentication.DTOs;
using BookingPlatform.Application.Authentication.Interfaces;
using BookingPlatform.Application.Authentication.Queries.GetCurrentUser;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Infrastructure.Authentication;
using BookingPlatform.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookingPlatform.Tests.Authentication;

public class Phase1AuthenticationTests
{
    [Fact]
    public async Task RegisterCommand_CreatesCustomerAndReturnsAuthResponse()
    {
        var repository = new InMemoryUserRepository();
        var tokenGenerator = new FakeTokenGenerator();
        var handler = new RegisterCommandHandler(repository, tokenGenerator, new RegisterCommandValidator());

        var response = await handler.Handle(new RegisterCommand("(555) 123-4567", "Ada Lovelace", "ada@example.com"), CancellationToken.None);

        Assert.Equal("token-1", response.Token);
        Assert.Equal("Ada Lovelace", response.User.Nombre);
        Assert.Equal("ada@example.com", response.User.Email);
        Assert.Equal("5551234567", response.User.Telefono);
        Assert.Equal("cliente", response.User.Rol);
        Assert.Single(repository.Users);
    }

    [Fact]
    public async Task RegisterCommand_RejectsDuplicatePhone()
    {
        var repository = new InMemoryUserRepository();
        repository.Users.Add(new User { Id = Guid.NewGuid(), Nombre = "Existing", Telefono = "5551234567", Role = Role.Customer });
        var tokenGenerator = new FakeTokenGenerator();
        var handler = new RegisterCommandHandler(repository, tokenGenerator, new RegisterCommandValidator());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new RegisterCommand("555-123-4567", "Ada", "ada@example.com"), CancellationToken.None));

        Assert.Equal("El telefono ya esta registrado.", exception.Message);
    }

    [Fact]
    public async Task LoginCommand_ReturnsTokenForKnownPhone()
    {
        var repository = new InMemoryUserRepository();
        repository.Users.Add(new User { Id = Guid.NewGuid(), Nombre = "Ada", Email = "ada@example.com", Telefono = "5551234567", Role = Role.Customer });
        var tokenGenerator = new FakeTokenGenerator();
        var handler = new LoginCommandHandler(repository, tokenGenerator, new LoginCommandValidator());

        var response = await handler.Handle(new LoginCommand("(555) 123-4567"), CancellationToken.None);

        Assert.Equal("token-1", response.Token);
        Assert.Equal("Ada", response.User.Nombre);
        Assert.Equal("cliente", response.User.Rol);
    }

    [Fact]
    public async Task GetCurrentUserQuery_ReturnsProfile()
    {
        var repository = new InMemoryUserRepository();
        var userId = Guid.NewGuid();
        repository.Users.Add(new User { Id = userId, Nombre = "Ada", Email = "ada@example.com", Telefono = "5551234567", Role = Role.Worker });
        var handler = new GetCurrentUserQueryHandler(repository);

        var response = await handler.Handle(new GetCurrentUserQuery(userId), CancellationToken.None);

        Assert.Equal(userId, response.Id);
        Assert.Equal("Ada", response.Nombre);
        Assert.Equal("trabajador", response.Rol);
    }

    [Fact]
    public async Task UpdateCurrentUserCommand_UpdatesPhoneAndProfile()
    {
        var repository = new InMemoryUserRepository();
        var userId = Guid.NewGuid();
        repository.Users.Add(new User { Id = userId, Nombre = "Ada", Email = "ada@example.com", Telefono = "5551234567", Role = Role.Customer });
        var handler = new UpdateCurrentUserCommandHandler(repository, new UpdateCurrentUserCommandValidator());

        var response = await handler.Handle(new UpdateCurrentUserCommand
        {
            UserId = userId,
            Nombre = "Ada Updated",
            Email = "ada.updated@example.com",
            Telefono = "+1 (555) 765-4321"
        }, CancellationToken.None);

        Assert.Equal("Ada Updated", response.Nombre);
        Assert.Equal("ada.updated@example.com", response.Email);
        Assert.Equal("15557654321", response.Telefono);
        Assert.Single(repository.Users);
        Assert.Equal("Ada Updated", repository.Users[0].Nombre);
    }

    [Fact]
    public async Task UpdateCurrentUserCommand_RejectsMissingRequiredFields()
    {
        var repository = new InMemoryUserRepository();
        var userId = Guid.NewGuid();
        repository.Users.Add(new User { Id = userId, Nombre = "Ada", Email = "ada@example.com", Telefono = "5551234567", Role = Role.Customer });
        var handler = new UpdateCurrentUserCommandHandler(repository, new UpdateCurrentUserCommandValidator());

        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.Handle(new UpdateCurrentUserCommand
            {
                UserId = userId,
                Nombre = string.Empty,
                Email = string.Empty,
                Telefono = string.Empty
            }, CancellationToken.None));

        Assert.Contains("El nombre es obligatorio.", exception.Errors.Select(x => x.ErrorMessage));
        Assert.Contains("El email es obligatorio.", exception.Errors.Select(x => x.ErrorMessage));
        Assert.Contains("El telefono es obligatorio.", exception.Errors.Select(x => x.ErrorMessage));
    }

    [Fact]
    public void JwtTokenGenerator_EmitsRoleAndUserClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "BookingPlatform",
            Audience = "BookingPlatform",
            SecretKey = "BookingPlatform_test_secret_key_123456789",
            ExpiresMinutes = 60
        });
        var generator = new JwtTokenGenerator(options);

        var token = generator.GenerateToken(new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Nombre = "Ada",
            Email = "ada@example.com",
            Telefono = "5551234567",
            Role = Role.Admin
        });

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("BookingPlatform", jwt.Issuer);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == "11111111-1111-1111-1111-111111111111");
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(jwt.Claims, claim => claim.Type == "telefono" && claim.Value == "5551234567");
    }

    private sealed class FakeTokenGenerator : IJwtTokenGenerator
    {
        private int _counter;

        public string GenerateToken(User user)
        {
            _counter++;
            return $"token-{_counter}";
        }
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public List<User> Users { get; } = new();

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var index = Users.FindIndex(existing => existing.Id == user.Id);
            if (index >= 0)
            {
                Users[index] = user;
            }

            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.FirstOrDefault(x => x.Id == id));
        }

        public Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.FirstOrDefault(x => x.Telefono == phone));
        }

        public Task<IReadOnlyList<User>> GetCustomersAsync(string? search, CancellationToken cancellationToken = default)
        {
            var query = Users.Where(x => x.Role == Role.Customer);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    (x.Nombre ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (x.Email ?? string.Empty).ToLowerInvariant().Contains(term));
            }

            return Task.FromResult((IReadOnlyList<User>)query.ToList());
        }
    }
}