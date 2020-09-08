using System;
using ApiAuthTokenManagement.V1.Gateways;
using ApiAuthTokenManagement.V1.Infrastructure;
using ApiAuthTokenManagement.V1.UseCase;
using ApiAuthTokenManagement.V1.UseCase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAuthTokenManagement
{
    public static class ConfigureServices
    {
        public static void Configure(this IServiceCollection services)
        {
            ConfigureDbContext(services);
            RegisterGateways(services);
            RegisterUseCases(services);
        }

        private static void ConfigureDbContext(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            services.AddDbContext<TokenDatabaseContext>(
                opt => opt.UseNpgsql(connectionString));
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddScoped<IAuthTokenDatabaseGateway, AuthTokenDatabaseGateway>();
            services.AddScoped<IAwsApiGateway, AwsApiGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.AddScoped<IGenerateJwt, GenerateJwt>();
            services.AddScoped<IPostTokenUseCase, PostTokenUseCase>();
            services.AddScoped<IVerifyAccessUseCase, VerifyAccessUseCase>();
        }
    }
}
