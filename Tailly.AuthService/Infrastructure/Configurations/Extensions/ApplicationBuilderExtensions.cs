using Tailly.AuthService.Web.Middlewares;

namespace Tailly.AuthService.Infrastructure.Configurations.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseExceptionHandlerMiddleware();
        app.UseHttpsRedirection();
        app.UseSwaggerSetup();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();

        return app;
    }

    private static IApplicationBuilder UseSwaggerSetup(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tailly Auth API V1");
            });
        }

        return app;
    }

    private static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        return app;
    }
}