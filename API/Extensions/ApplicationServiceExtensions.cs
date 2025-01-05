
using Application.Core;
using Application.Dtos;
using Application.RoadmapActivities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddValidatorsFromAssemblyContaining<RoadmapValidatorDto>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            });

            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .WithOrigins(config["CorsSettings:BaseUrl"])
                          .AllowCredentials(); 
                });

            });

            //Mediator Register all Handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly));
            //Mapping Profiles
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            return services;
        }
    }
}
