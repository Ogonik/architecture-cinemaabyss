using EventsKafkaTest.Services;
using EventsKafkaTest.Services.Model;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EventsKafkaTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            builder.Services.Configure<KafkaConsumerSettings>(builder.Configuration.GetSection("KafkaConsumerSettings"));
            builder.Services.Configure<KafkaProducerSettings>(builder.Configuration.GetSection("KafkaProducerSettings"));
            


            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(builder.Configuration)
                            .CreateLogger();

            builder.Services.AddSerilog(Log.Logger);
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            Log.Logger.Information("WebApp builder created. Starting up.");

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
            builder.Services.AddHostedService<KafkaConsumerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseAuthorization();


            app.MapControllers();

            app.MapGet("/health", () =>
            {
                return Results.Ok(new Tuple<string, bool>("status", true));
            });

            app.Run();
        }
    }
}
