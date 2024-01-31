using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Football.Infrastructure.Persistence;
using Football.Models.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Football.Controllers
{
    [ApiController]
    [Route("football")]
    public class FootballController : ControllerBase
    {
        private readonly ILogger<FootballController> _logger;
        private readonly ApplicationDbContext _context;

        public FootballController(ILogger<FootballController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Route("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                // Attempt to retrieve the city from the database
                var matches = await _context.Matches.ToListAsync();

                if (matches.Any() || matches.Count == 0)
                {
                    // City with the specified Id not found
                    return Ok("Microservice is functional");
                }
                else throw new Exception("Microservice is down");
              
            }
            catch (Exception ex)
            {
                // Log any errors that occur during database access
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet]
        [Route("getnextmatches")]
        public async Task<IActionResult> GetFootballMatchesNext5Days()
        {
            try
            {
                DateTime today = DateTime.Today;

                // Get the date 5 days from today
                DateTime toDate = today.AddDays(5);
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://api-football-v1.p.rapidapi.com/v3/fixtures?season=2023&league=39&from={today:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}"),
                    Headers =
            {
                { "X-RapidAPI-Key", "f95ce684c2msh5ca0330a7bc3f70p115564jsnfc599ba663d2" },
                { "X-RapidAPI-Host", "api-football-v1.p.rapidapi.com" },
            },
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Deserialize the response into a collection of FixtureResponse
                    var fixturesResponse = JsonConvert.DeserializeObject<FixtureResponse>(responseBody);

                    // Initialize a list to store the matches
                    var matches = new List<Match>();

                    // Iterate over each fixture in the response
                    foreach (var fixture in fixturesResponse.Response)
                    {
                        // Extract required information
                        var city = fixture.Fixture.Venue.City;
                        var date = fixture.Fixture.Date;
                        var homeTeam = fixture.Teams.Home.Name;
                        var awayTeam = fixture.Teams.Away.Name;

                        // Check if the match already exists in the database based on City and Date
                        var existingMatch = await _context.Matches.FirstOrDefaultAsync(m => m.City == city && m.Date == date);
                        if (existingMatch == null)
                        {
                            // Create a Match object and add it to the matches list
                            var match = new Match
                            {
                                City = city,
                                Date = date,
                                Home = homeTeam,
                                Away = awayTeam
                            };
                            _context.Matches.Add(match);    
                            matches.Add(match);
                        }
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Return the list of added matches
                    return Ok(matches);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if any
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getmatch/{home}/{away}")]
        public async Task<IActionResult> GetDetailsAboutaMatch(string home, string away)
        {
            var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync("http://127.0.0.1:5000/football/getmatches");
                if (response.IsSuccessStatusCode)
                {
                    var match = await _context.Matches.FirstOrDefaultAsync(m => m.Home == home && m.Away == away);

                    if (match != null)
                    {
                        // Match found, return the city and date
                        return Ok(match);
                    }
                    else
                    {
                        // No match found, return a message indicating that there is no such match
                        return NotFound("No matches found for the provided teams.");
                    }
                }

                else return BadRequest();
                    
            }
            catch (Exception ex)
            {
                // Handle exceptions if any
                return StatusCode(500, ex.Message);
            }
        }

    }
}
