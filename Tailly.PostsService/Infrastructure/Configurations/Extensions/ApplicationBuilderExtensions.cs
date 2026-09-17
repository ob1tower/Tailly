using Tailly.PostsService.Web.Middlewares;

namespace Tailly.PostsService.Infrastructure.Configurations.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseExceptionHandlerMiddleware();
        app.UseHttpsRedirection();
        app.UseSwaggerSetup();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
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
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tailly POST API V1");
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