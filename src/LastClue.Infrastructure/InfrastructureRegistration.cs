using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<ICaseRepository, CaseRepository>();
        services.AddScoped<IScoreRepository, ScoreRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
