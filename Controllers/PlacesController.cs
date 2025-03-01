using Microsoft.AspNetCore.Mvc;
using TraWell.Models;
using TraWell.Services;

namespace TraWell.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly PlaceService _placeService;
    private readonly VideoService _videoService;

    public PlacesController(PlaceService placeService, VideoService videoService)
    {
        _placeService = placeService;
        _videoService = videoService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Place>> GetPlace(int id)
    {
        try
        {
            var place = await _placeService.GetPlaceById(id);
            return Ok(place);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving place: " + ex.Message);
        }
    }

    [HttpGet("{id}/videos")]
    public async Task<ActionResult<List<Video>>> GetPlaceVideos(int id)
    {
        try
        {
            var place = await _placeService.GetPlaceById(id);
            var videos = await _videoService.GetVideosForPlace(place.Name);
            return Ok(videos);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving videos: " + ex.Message);
        }
    }
}
