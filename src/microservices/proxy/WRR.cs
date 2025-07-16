using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace ProxyGradualMigration
{
    public class WeightRoundRobinLoadBalancingPolicy : ILoadBalancingPolicy
    {
        private ILogger<WeightRoundRobinLoadBalancingPolicy> _logger;

        public string Name => "WeightRoundRobin";

        public WeightRoundRobinLoadBalancingPolicy(ILogger<WeightRoundRobinLoadBalancingPolicy> logger)
        {
            _logger = logger;
        }

        public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
        {
            var random = new Random();
            var randomNumber = random.Next(1, 99);

            var router = new Dictionary<string, int>();
            foreach (var availableDestination in availableDestinations)
            {
                var availableDestinationMetadata = availableDestination.Model.Config.Metadata;
                var weightPresented = availableDestinationMetadata.TryGetValue("Weight", out string? weightString);

                var weightInt = int.Parse(weightString);

                router.Add(availableDestination.DestinationId, weightInt);
            }

            var cumulativeWeights = new Dictionary<string, int>();
            int sum = 0;
            
            
            foreach (var route in router)
            {
                sum += route.Value;
                cumulativeWeights.Add(route.Key, sum);
            }

            // very straight for 2 possible destinations
            var result = cumulativeWeights.First(x => x.Value >= randomNumber);

            _logger.LogInformation($"PickDestination: '{randomNumber}' --> [{availableDestinations.First(x => x.DestinationId == result.Key).DestinationId}]");

            return availableDestinations.First(x => x.DestinationId == result.Key);            

            //if (Weighting.WeightedClusterWeights.TryGetValue(cluster.ClusterId, out var weightedWeights))
            //{
            //    if (weightedWeights is null)
            //    {
            //        _logger.LogInformation($"PickDestination Error: Can not get [{cluster.ClusterId}] cluster weightedWeights");
            //        return null;
            //    }

            //    if (weightedWeights.DestinationIds is null)
            //    {
            //        _logger.LogInformation($"PickDestination Error: Can not get [{cluster.ClusterId}] destination, DestinationIds is null");
            //        return null;
            //    }

            //    var destinationId = weightedWeights.DestinationIds[WeightingHelper.GetIndexByRandomWeight(weightedWeights.DestinationWeightedWeights, weightedWeights.DestinationWeights, weightedWeights.TotalWeights ?? 1D)];

            //    return availableDestinations.FirstOrDefault(destination => destination.DestinationId == destinationId);
            //}

            //_logger.LogInformation($"PickDestination Error: Can not get [{cluster.ClusterId}] cluster");
            //return null;
        }
    }


    //public class WeightConfigFilter : IProxyConfigFilter
    //{
    //    private ILogger<WeightConfigFilter> _logger;

    //    public WeightConfigFilter(ILogger<WeightConfigFilter> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig cluster, CancellationToken cancel)
    //    {
    //        _logger.LogInformation($"[{DateTime.Now}]:{nameof(WeightConfigFilter)}.{nameof(ConfigureClusterAsync)} Started");

    //        try
    //        {
    //            var weights = cluster.Destinations?.ToDictionary(destination => destination.Key, destination =>
    //            {
    //                if (destination.Value.Metadata?.TryGetValue("Weight", out var weight) ?? false)
    //                    return double.Parse(weight) / 100D;
    //                else
    //                    return 1D;
    //            });

    //            List<string> destinationIds = new();
    //            List<double> destinationWeights = new();
    //            WeightedWeight? weightedWeight = null;

    //            if (weights is not null)
    //            {
    //                foreach (var weight in weights)
    //                {
    //                    destinationIds.Add(weight.Key);
    //                    destinationWeights.Add(weight.Value);
    //                }
    //                var weightedWeights = WeightingHelper.GetWeightedWeights(destinationWeights.ToArray());
    //                weightedWeight = new()
    //                {
    //                    DestinationIds = destinationIds.ToArray(),
    //                    DestinationWeights = destinationWeights.ToArray(),
    //                    DestinationWeightedWeights = weightedWeights.Weights,
    //                    TotalWeights = weightedWeights.TotalWeight
    //                };
    //            }



    //            //if (Weighting.ClusterWeights.ContainsKey(cluster.ClusterId))
    //            //{
    //            //    Weighting.ClusterWeights[cluster.ClusterId] = weights;
    //            //    Weighting.WeightedClusterWeights[cluster.ClusterId] = weightedWeight;
    //            //}
    //            //else
    //            //{
    //            //    Weighting.ClusterWeights.Add(cluster.ClusterId, weights);
    //            //    Weighting.WeightedClusterWeights.Add(cluster.ClusterId, weightedWeight);
    //            //}

    //            //_logger.LogInformation($"[{DateTime.Now}]:{nameof(WeightConfigFilter)}.{nameof(ConfigureClusterAsync)} Set, clusterId: {cluster.ClusterId}, {JsonSerializer.Serialize(Weighting.WeightedClusterWeights[cluster.ClusterId])}");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogInformation($"[{DateTime.Now}]:{nameof(WeightConfigFilter)}.{nameof(ConfigureClusterAsync)} Error:{ex}");
    //        }

    //        _logger.LogInformation($"[{DateTime.Now}]:{nameof(WeightConfigFilter)}.{nameof(ConfigureClusterAsync)} Finished");

    //        return new ValueTask<ClusterConfig>(cluster);
    //    }

    //    public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig? cluster, CancellationToken cancel)
    //    {
    //        return new ValueTask<RouteConfig>(route);
    //    }
    //}


    //public class WeightingHelper
    //{
    //    public static (double[]? Weights, double? TotalWeight) GetWeightedWeights(double[] weights)
    //    {
    //        if (weights.Length == 0) return (null, null);
    //        else if (weights.Length == 1) return ([.. weights], weights[0]);

    //        var totalWeight = 0D;
    //        Span<double> newWeights = stackalloc double[weights.Length];

    //        for (int i = 0; i < weights.Length; i++)
    //        {
    //            totalWeight += weights[i];
    //            newWeights[i] = totalWeight;
    //        }

    //        return ([.. newWeights], totalWeight);
    //    }

    //    public static int GetIndexByRandomWeight(Span<double> weightedWeights, Span<double> weights, double totalWeight)
    //    {
    //        // Ignore weight when only one server
    //        if (weightedWeights.Length == 1) return 0;

    //        var randomWeight = Random.Shared.NextDouble() * totalWeight;
    //        var index = weightedWeights.BinarySearch(randomWeight);

    //        if (index < 0)
    //            index = -index - 1;
    //        else if (index > weightedWeights.Length)
    //            // The number of servers decreases
    //            index = GetIndexByRandomWeight(weightedWeights, weights, totalWeight);

    //        if (weights[index] != 0D)
    //            return index;
    //        else
    //            // The weight of the server is 0
    //            return GetIndexByRandomWeight(weightedWeights, weights, totalWeight);
    //    }
    //}

    //public class WeightedWeight
    //{
    //    public string[]? DestinationIds { get; set; }

    //    public double[]? DestinationWeights { get; set; }

    //    public double[]? DestinationWeightedWeights { get; set; }

    //    public double? TotalWeights { get; set; }
    //}
}
