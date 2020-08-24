using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.Infrastructure;
using ApiAuthTokenGenerator.V1.UseCase;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ApiAuthTokenGenerator
{
    public static class ConfigureServices
    {
        public static void Configure(this IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            services.AddDbContext<TokenDatabaseContext>(
                opt => opt.UseNpgsql(connectionString));

            services.AddScoped<IAuthTokenDatabaseGateway, AuthTokenDatabaseGateway>();
            services.AddScoped<IAwsApiGateway, AwsApiGateway>();

            services.AddScoped<IVerifyAccessUseCase, VerifyAccessUseCase>(sp =>
            {
                return new VerifyAccessUseCase(sp.GetService<IAuthTokenDatabaseGateway>(),
                   sp.GetService<IAwsApiGateway>());
            });
        }
    }
}
