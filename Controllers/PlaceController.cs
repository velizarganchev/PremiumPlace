using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PremiumPlace_API.Data;
using PremiumPlace_API.Models.DTO;
using PremiumPlace_API.Services;
using PremiumPlace_API.Services.Places;

namespace PremiumPlace_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;
        public PlacesController(IPlaceService placeService, ApplicationDbContext db, IMapper mapper) 
        {
            _placeService = placeService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<PlaceDTO>>>> GetPlaces()
        {
            var response = await _placeService.GetAllPlaces();
            if (response.Success == false)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlaceDTO>> GetPlaceById(int id)
        {
            try {
                var response = await _placeService.GetPlaceById(id);
                if (response.Success == false)
                {
                    return NotFound(response);
                }
                return Ok(response);
            }
            catch (Exception) {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PlaceDTO>> CreatePlace(PlaceCreateDTO placeDTO)
        {
            try
            {
                var response = await _placeService.CreatePlace(placeDTO);

                if (response.Success == false)
                {
                    return BadRequest(response);
                }

                return CreatedAtAction(nameof(CreatePlace), response);
            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PlaceCreateDTO>> UpdatePlace(int id, PlaceUpdateDTO placeDTO)
        {
            try
            {
                var response = await _placeService.UpdatePlace(id, placeDTO);

                if (response.Success == false)
                {
                    return BadRequest(response);
                }
                if (response.Success == false && response.Data != null)
                {
                    return Conflict(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult<PlaceDTO>> DeletePlace(int id)
        {
            try
            {
                var response = await _placeService.DeletePlace(id);
                if (response.Success == false)
                {
                    return NotFound(response);
                }
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
