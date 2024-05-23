using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace API
{
    public class WeatherForecastApi

    {
        private readonly ILogger _logger;

        public WeatherForecastApi(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WeatherForecastApi>();
        }

        [Function("WeatherForecast")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // log the jwt token
            if (req.Headers.TryGetValues("Authorization", out var values))
            {
                _logger.LogInformation($"Authorization: {values.First()}");
                // parse the jwt 
                var verify = new JwtSecurityTokenHandler();
                var jwt = verify.ReadJwtToken(values.First().Substring(7)); // remove "Bearer "

                //log the claims
                foreach (var claim in jwt.Claims)
                {
                    _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
                }
            }
            else
            {
               _logger.LogInformation("Authorization: not found");
                var forbidden = req.CreateResponse(HttpStatusCode.Unauthorized);
                return forbidden;
            }


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var list = new List<WeatherForecast>();
            for (int i = 0; i < 5; i++)
            {
                var wf = new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = "Freezing " + i
                };
                list.Add(wf);
            }

            response.WriteString(JsonSerializer.Serialize(list));

            return response;
        }
    }

    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public string? Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
