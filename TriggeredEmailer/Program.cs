using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TriggeredEmailer.Data;
using TriggeredEmailer.Helpers;
using TriggeredEmailer.Interfaces;
using TriggeredEmailer.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Program");

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var tasks = args.ToList();
                foreach (var task in tasks)
                {
                    logger.LogInformation($"{task} scheduler is running...");
                    switch (task.ToLower())
                    {
                        case "mailsession":
                            var service = services.GetRequiredService<SessionService>();
                            await service.SendEmailToProviderForIncompleteSessionAsync();
                            break;
                        case "billing":
                            var billingService = services.GetRequiredService<BillingService>();

                            //for BT, run auto billing 48hrs(Monday)
                            //for BCBA, run auto billing 72hrs(Tuesday)
                            await billingService.ConfigureSendBilling();
                            break;
                        default:
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                Console.ReadLine();
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

            services.AddScoped(typeof(IFileLogger<>), typeof(FileLoggerService<>));
            services.AddScoped<SessionService>();
            services.AddScoped<BillingService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IVWSessionsService, VWSessionsService>();
            services.AddScoped<IVWStaffService, VWStaffService>();
        });

}
