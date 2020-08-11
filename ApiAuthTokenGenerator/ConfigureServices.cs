using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.Infrastructure;
using ApiAuthTokenGenerator.V1.UseCase;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            services.AddScoped<IVerifyAccessUseCase, VerifyAccessUseCase>();
        }
    }
}
