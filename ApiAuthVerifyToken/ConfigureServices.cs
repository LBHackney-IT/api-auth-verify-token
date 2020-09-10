using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.Infrastructure;
using ApiAuthVerifyToken.V1.UseCase;
using ApiAuthVerifyToken.V1.UseCase.Interfaces;

namespace ApiAuthVerifyToken
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
            services.AddScoped<IVerifyAccessUseCase, VerifyAccessUseCase>();
        }
    }
}
