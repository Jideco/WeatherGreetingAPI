using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace WeatherGreetingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreetingController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;


        public GreetingController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpGet("hello")]
        public async Task<IActionResult> Hello()
        {
            try
            {
                var clientIp = await GetClientIpAddressAsync();
                var locationInfo = await GetLocationFromIpAddress(clientIp);
                var weatherData = await GetWeatherData(locationInfo.City);


                return Ok(new
                {
                    client_ip = clientIp,
                    location = locationInfo.City,
                    greeting = $"Hello, Mark! The temperature is {weatherData.Main.Temp:F1} degrees Celsius in {locationInfo.City}, {locationInfo.CountryCode}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        private async Task<string> GetClientIpAddressAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://api64.ipify.org?format=json");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var ipInfo = JsonConvert.DeserializeObject<IpifyResponse>(content);
            return ipInfo.Ip;
        }




        private async Task<LocationInfo> GetLocationFromIpAddress(string ipAddress)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"http://ip-api.com/json/{ipAddress}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LocationInfo>(content);
        }

        private async Task<WeatherResponse> GetWeatherData(string city)
        {
            var apiKey = _configuration["OpenWeatherMap:ApiKey"];
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherResponse>(content);
        }

    }

        public class WeatherResponse
        {
            public MainData Main { get; set; }
        }

        public class MainData
        {
            public float Temp { get; set; }
        }
}
