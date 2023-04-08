using Microsoft.OpenApi.Models;

namespace InvoiceGeneratorApi.Services;

public static class BuilderServiceExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(setup =>
        {
            setup.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "InvoiceGeneratorApi",
                    Version = "v1",
                });

            //setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            //{
            //    Name = "Authorization",
            //    Type = SecuritySchemeType.ApiKey,
            //    Scheme = "Bearer",
            //    BearerFormat = "JWT",
            //    In = ParameterLocation.Header,
            //    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\""
            //});
            //setup.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Type = ReferenceType.SecurityScheme,
            //                Id = "Bearer"
            //            }
            //        }, new string[]{}
            //    }
            //});
            var filePath = Path.Combine(AppContext.BaseDirectory, "InvoiceGeneratorApi.xml");
            setup.IncludeXmlComments(filePath);
        });
        return services;
    }
}