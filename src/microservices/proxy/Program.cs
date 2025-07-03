
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using System.Reflection.Metadata.Ecma335;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace ProxyGradualMigration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            _ = bool.TryParse(Environment.GetEnvironmentVariable("GRADUAL_MIGRATION") ?? throw new Exception("GRADUAL_MIGRATION env not set"), out bool isGradualMigration);

            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(builder.Configuration)
                            .CreateLogger();

            builder.Services.AddSerilog(Log.Logger);
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            Log.Logger.Information("WebApp builder created. Starting up.");


            builder.Services.AddReverseProxy().LoadFromMemory(GetRoutes(isGradualMigration), GetClusters(isGradualMigration));

            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Response;
            });

            // Add services to the container.
            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseRouting();

            app.UseHttpsRedirection();

            app.UseHttpLogging();

            app.MapReverseProxy();

            app.MapGet("/health", () =>
            {
                return Results.Ok("Strangler Fig Proxy is healthy");
            });

            app.Run();
        }

        private static IReadOnlyList<RouteConfig> GetRoutes(bool isGradualMigration)
        {
            // если есть /api/movies/* --> то на отдельный кластер с потенциальной развесовской, убира€ "/api/movies"
            var routeMoviesWithMigration = new RouteConfig()
            {
                RouteId = "movies",
                ClusterId = "cinema_abyss_movies_gradual_migration",
                Order = 10,
                Match = new RouteMatch
                {
                    Path = "/api/movies/{**catch-all}",

                }
            };
            routeMoviesWithMigration = routeMoviesWithMigration
               // .WithTransformPathRemovePrefix("/api/movies")
                .WithTransformResponseHeader(headerName: "Source", value: "YARP", append: true, condition: ResponseCondition.Success);

            // если есть /api/events/* --> на отдельный кл
            var routeEventsConfig = new RouteConfig()
            {
                RouteId = "events",
                Order = 20,
                ClusterId = "cinema_abyss_events_testing",
                Match = new RouteMatch
                {
                    Path = "/api/events/{**catch-all}"
                },
            };
            routeEventsConfig = routeEventsConfig
                .WithTransformPathRemovePrefix("/api/events")
                .WithTransformResponseHeader(headerName: "Source", value: "YARP", append: true, condition: ResponseCondition.Success);



            // ¬ообще все на монолит
            var routeDefaultConfig = new RouteConfig()
            {
                RouteId = "all, but movies and events",
                ClusterId = "cinema_abyss_monolith",
                Match = new RouteMatch
                {
                    Path = "{**catch-all}"
                },
                Order = 100
            };


            var routeConfigs = new List<RouteConfig>
            {
                routeDefaultConfig,
                routeEventsConfig
            };

            if (isGradualMigration)
                routeConfigs.Add(routeMoviesWithMigration);

            return routeConfigs;
        }


        private static IReadOnlyList<ClusterConfig> GetClusters(bool isGradualMigration)
        {
            var monolithAddress = Environment.GetEnvironmentVariable("MONOLITH_URL") ?? throw new Exception("MONOLITH_URL env not set");
            var moviesServiceAddress = Environment.GetEnvironmentVariable("MOVIES_SERVICE_URL") ?? throw new Exception("MOVIES_SERVICE_URL env not set");
            var eventsServiceAddress = Environment.GetEnvironmentVariable("EVENTS_SERVICE_URL") ?? throw new Exception("EVENTS_SERVICE_URL env not set");
            var moviesRequestsPercentageString = Environment.GetEnvironmentVariable("MOVIES_MIGRATION_PERCENT") ?? throw new Exception("MOVIES_MIGRATION_PERCENT env not set");

            _ = int.TryParse(moviesRequestsPercentageString, out int moviesRequestsPercentage);
            Console.WriteLine($"moviesRequestsPercentageString = {moviesRequestsPercentageString}");
            if (!(moviesRequestsPercentage > 0 && moviesRequestsPercentage < 100)) { throw new Exception("MOVIES_MIGRATION_PERCENT is not correct"); }

            var moviesMigrationClusterMonolithDestination = new DestinationConfig()
            {
                Address = monolithAddress,
                Metadata = new Dictionary<string, string> {
                    { "Weight", $"{100 - moviesRequestsPercentage}" }
                }
            };
            var moviesMigrationClusterMoviesService = new DestinationConfig()
            {
                Address = moviesServiceAddress,
                Metadata = new Dictionary<string, string> {
                    { "Weight", $"{moviesRequestsPercentage}" }
                }
            };
            var moviesMigrationCluster = new ClusterConfig()
            {
                ClusterId = "cinema_abyss_movies_gradual_migration",
                Destinations = new Dictionary<string, DestinationConfig> {
                    { "monolith", moviesMigrationClusterMonolithDestination },
                    { "movies_service",moviesMigrationClusterMoviesService },
                }
            };

            var eventsClusterDestination = new DestinationConfig() {Address = eventsServiceAddress,};       
            var eventsCluster = new ClusterConfig()
            {
                ClusterId = "cinema_abyss_events_testing",
                Destinations = new Dictionary<string, DestinationConfig> {
                    { "events", eventsClusterDestination },
                }
            };

            var monolithClusterDestination = new DestinationConfig() { Address = monolithAddress, };
            var monolithCluster = new ClusterConfig()
            {
                ClusterId = "cinema_abyss_monolith",
                Destinations = new Dictionary<string, DestinationConfig> {
                    { "monolith", monolithClusterDestination },
                }
            };

            var clusterConfigs = new List<ClusterConfig>
            {
                eventsCluster,
                monolithCluster
            };

            if (isGradualMigration)
                clusterConfigs.Add(moviesMigrationCluster);

            return clusterConfigs;

        }
    }
}
