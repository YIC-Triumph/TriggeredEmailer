using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TriggeredEmailer.Data;
using TriggeredEmailer.Helpers;
using TriggeredEmailer.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                var service = services.GetRequiredService<SessionService>();

                await service.SendEmailToProviderForIncompleteSessionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureServices((_, services) =>
        {
            // Retrieve the connection string from configuration
            var connectionString = _.Configuration.GetConnectionString("DBConnectionString");

            services.Configure<AppSettings>(_.Configuration.GetSection("AppSettings"));

            // Add DbContext with the retrieved connection string
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<SessionService>();
            services.AddScoped<IEmailService, EmailService>();
        });

}
