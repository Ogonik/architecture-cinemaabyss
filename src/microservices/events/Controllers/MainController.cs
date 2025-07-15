using EventsKafkaTest.Controllers.Models;
using EventsKafkaTest.Services;
using EventsKafkaTest.Services.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO.Pipelines;
using System.Text.Json;

namespace EventsKafkaTest.Controllers
{
    [ApiController]
    //[Route("api/events/")]
    [Route("/")]

    public class MainController : ControllerBase
    {
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly ILogger<MainController> _logger;
        private readonly KafkaProducerSettings _kafkaProducerSettings;

        public MainController(ILogger<MainController> logger, IKafkaProducerService producerService, IOptions<KafkaProducerSettings> kafkaSettings)
        {
            _kafkaProducerSettings = kafkaSettings.Value;
            _logger = logger;
            _kafkaProducerService = producerService;
        }

        [HttpPost("movie")]
        public async Task<IActionResult> MovieCreateEvent(MovieDTOModel movieInfo)
        {
            _logger.LogInformation("Create Movie API hit");
            movieInfo.Title = movieInfo.Title + "_mod";
            var movieJson = JsonSerializer.Serialize(movieInfo);

            var sendResult = await _kafkaProducerService.SendMessageAsync(_kafkaProducerSettings.MoviesTopicName, movieJson);
            
            _logger.LogInformation("Create Movie message sent");

            var result = new ResponseDTOModel { 
                Status = "success",
                Partition = sendResult.Partition,
                Offset = sendResult.Offset.Value,
                Event = new ResponseEvent(movieInfo.MovieId, "movie")
            };


            return new CreatedResult(string.Empty, result);
        }

        [HttpPost("payment")]
        public async Task<IActionResult> PaymentCreateEvent(PaymentDTOModel paymentInfo)
        {
            _logger.LogInformation("Create Payment API hit");
            paymentInfo.Timestamp = DateTime.Now;
            var paymentJson = JsonSerializer.Serialize(paymentInfo);

            var sendResult = await _kafkaProducerService.SendMessageAsync(_kafkaProducerSettings.PaymentTopicName, paymentJson);

            _logger.LogInformation("Create Payment message sent");

            var result = new ResponseDTOModel
            {
                Status = "success",
                Partition = sendResult.Partition,
                Offset = sendResult.Offset.Value,
                Event = new ResponseEvent(paymentInfo.PaymentId, "payment")
            };


            return new CreatedResult(string.Empty, result);

        }

        [HttpPost("user")]
        public async Task<IActionResult> UserCreateEvent(UserDTOModel userInfo)
        {
            _logger.LogInformation("Create User API hit");

            userInfo.Timestamp = DateTime.Now;
            var userJson = JsonSerializer.Serialize(userInfo);

            var sendResult = await _kafkaProducerService.SendMessageAsync(_kafkaProducerSettings.UserTopicName, userJson);

            _logger.LogInformation("Create User message sent");

            var result = new ResponseDTOModel
            {
                Status = "success",
                Partition = sendResult.Partition,
                Offset = sendResult.Offset.Value,
                Event = new ResponseEvent(userInfo.UserId, "user")
            };


            return new CreatedResult(string.Empty, result);

        }
    }
}
