using Microsoft.EntityFrameworkCore;

namespace LoviBackend.Data.Extensions
{
    public static class MigrationExtensions
    {
        public static WebApplication MigrateDatabase<TContext>(this WebApplication app)
            where TContext : ApplicationDbContext
        {
            // Create a scope to resolve services
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("Attempting to migrate database for context {DbContextName}", typeof(TContext).Name);

                    // CORE ACTION: Apply any pending migrations
                    context!.Database.Migrate();

                    logger.LogInformation("Database migration complete for context {DbContextName}", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during database migration for context {DbContextName}", typeof(TContext).Name);
                    // You might want to re-throw here to halt startup if migration is critical
                    // throw;
                }
            }
            return app;
        }
    }
}
