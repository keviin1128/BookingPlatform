using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Services.Commands.CreateService;
using BookingPlatform.Application.Services.Commands.DeleteService;
using BookingPlatform.Application.Services.Commands.UpdateService;
using BookingPlatform.Application.Services.Queries.GetServiceById;
using BookingPlatform.Application.Services.Queries.GetServices;
using BookingPlatform.Application.Workers.Commands.CreateWorker;
using BookingPlatform.Application.Workers.DTOs;
using BookingPlatform.Application.Workers.Queries.GetWorkerById;
using BookingPlatform.Application.Workers.Queries.GetWorkers;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Tests.Catalog;

public class Phase2CatalogTests
{
    [Fact]
    public async Task ServiceCrud_WorksEndToEnd()
    {
        var serviceRepository = new InMemoryServiceRepository();

        var createHandler = new CreateServiceCommandHandler(serviceRepository, new CreateServiceCommandValidator());
        var created = await createHandler.Handle(
            new CreateServiceCommand("Corte", "Corte clasico", 45, 25m, true),
            CancellationToken.None);

        var getByIdHandler = new GetServiceByIdQueryHandler(serviceRepository);
        var found = await getByIdHandler.Handle(new GetServiceByIdQuery(created.Id), CancellationToken.None);

        Assert.Equal("Corte", found.Nombre);

        var updateHandler = new UpdateServiceCommandHandler(serviceRepository, new UpdateServiceCommandValidator());
        var updated = await updateHandler.Handle(new UpdateServiceCommand
        {
            Id = created.Id,
            Nombre = "Corte Premium",
            Descripcion = "Corte premium con lavado",
            Duracion = 60,
            Precio = 35m,
            Activo = true
        }, CancellationToken.None);

        Assert.Equal("Corte Premium", updated.Nombre);
        Assert.Equal(60, updated.Duracion);

        var getAllHandler = new GetServicesQueryHandler(serviceRepository);
        var list = await getAllHandler.Handle(new GetServicesQuery(), CancellationToken.None);
        Assert.Single(list);

        var deleteHandler = new DeleteServiceCommandHandler(serviceRepository, new DeleteServiceCommandValidator());
        await deleteHandler.Handle(new DeleteServiceCommand { Id = created.Id }, CancellationToken.None);

        var emptyList = await getAllHandler.Handle(new GetServicesQuery(), CancellationToken.None);
        Assert.Empty(emptyList);
    }

    [Fact]
    public async Task CreateWorker_AssignsServicesAndAgenda()
    {
        var userRepository = new InMemoryUserRepository();
        var serviceRepository = new InMemoryServiceRepository();
        var workerRepository = new InMemoryWorkerRepository();

        var service = new Service
        {
            Id = Guid.NewGuid(),
            Nombre = "Barba",
            Descripcion = "Perfilado de barba",
            Duracion = 30,
            Precio = 15m,
            Activo = true
        };
        await serviceRepository.AddAsync(service, CancellationToken.None);

        var createWorkerHandler = new CreateWorkerCommandHandler(
            userRepository,
            workerRepository,
            serviceRepository,
            new CreateWorkerCommandValidator());

        var created = await createWorkerHandler.Handle(new CreateWorkerCommand(
            "555-123-4567",
            "Juan",
            "juan@example.com",
            "Barbero",
            new[] { service.Id },
            new Dictionary<int, IReadOnlyList<ScheduleRangeDto>>
            {
                [1] = new[] { new ScheduleRangeDto("09:00", "13:00") }
            }), CancellationToken.None);

        Assert.Equal("Juan", created.Nombre);
        Assert.Equal("Barbero", created.Especialidad);
        Assert.Single(created.Servicios);
        Assert.NotNull(created.Agenda);
        Assert.True(created.Agenda!.ContainsKey(1));

        var getWorkersHandler = new GetWorkersQueryHandler(workerRepository);
        var workersByService = await getWorkersHandler.Handle(new GetWorkersQuery(null, service.Id), CancellationToken.None);
        Assert.Single(workersByService);

        var workerId = workersByService[0].Id;
        var getWorkerByIdHandler = new GetWorkerByIdQueryHandler(workerRepository);
        var workerById = await getWorkerByIdHandler.Handle(new GetWorkerByIdQuery(workerId), CancellationToken.None);
        Assert.Equal(workerId, workerById.Id);
    }

    private sealed class InMemoryServiceRepository : IServiceRepository
    {
        private readonly List<Service> _services = new();

        public Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Service>)_services.ToList());
        }

        public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_services.FirstOrDefault(x => x.Id == id));
        }

        public Task AddAsync(Service service, CancellationToken cancellationToken = default)
        {
            _services.Add(service);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
        {
            var index = _services.FindIndex(x => x.Id == service.Id);
            if (index >= 0)
            {
                _services[index] = service;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Service service, CancellationToken cancellationToken = default)
        {
            _services.RemoveAll(x => x.Id == service.Id);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_services.Any(x => x.Id == id));
        }
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var index = _users.FindIndex(x => x.Id == user.Id);
            if (index >= 0)
            {
                _users[index] = user;
            }

            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.FirstOrDefault(x => x.Id == id));
        }

        public Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.FirstOrDefault(x => x.Telefono == phone));
        }

        public Task<IReadOnlyList<User>> GetCustomersAsync(string? search, CancellationToken cancellationToken = default)
        {
            var query = _users.Where(x => x.Role == Role.Customer);

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

    private sealed class InMemoryWorkerRepository : IWorkerRepository
    {
        private readonly List<Worker> _workers = new();

        public Task AddAsync(Worker worker, CancellationToken cancellationToken = default)
        {
            _workers.Add(worker);
            return Task.CompletedTask;
        }

        public Task<Worker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workers.FirstOrDefault(x => x.Id == id));
        }

        public Task<Worker?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workers.FirstOrDefault(x => x.UserId == userId));
        }

        public Task<IReadOnlyList<Worker>> GetAllAsync(string? search, Guid? serviceId, CancellationToken cancellationToken = default)
        {
            var query = _workers.AsQueryable();

            if (serviceId.HasValue)
            {
                query = query.Where(x => x.WorkerServices.Any(ws => ws.ServiceId == serviceId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    (x.User.Nombre ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    x.User.Telefono.Contains(term) ||
                    x.Especialidad.ToLowerInvariant().Contains(term));
            }

            return Task.FromResult((IReadOnlyList<Worker>)query.ToList());
        }

        public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workers.Any(x => x.UserId == userId));
        }
    }
}
