using Microsoft.AspNetCore.Mvc;
using TraWell.Models;
using TraWell.Services;

namespace TraWell.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly RecommendationService _recommendationService;

    public RecommendationsController(RecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpPost]
    public async Task<ActionResult<List<Place>>> GetRecommendations(UserPreferences preferences)
    {
        try
        {
            var recommendations = await _recommendationService.GetRecommendations(preferences);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error getting recommendations: " + ex.Message);
        }
    }
}
