using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremiumPlace.DTO;
using PremiumPlace_API.Controllers.Extensions;
using PremiumPlace_API.Services.Places;

namespace PremiumPlace_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;

        public PlacesController(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlaces()
        {
            var sr = await _placeService.GetAllPlacesAsync();
            return this.ToActionResult(sr);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPlaces([FromQuery] PlaceQueryDTO query)
        {
            var sr = await _placeService.SearchPlacesAsync(query);
            return this.ToActionResult(sr);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPlaceById(int id)
        {
            var sr = await _placeService.GetPlaceByIdAsync(id);
            return this.ToActionResult(sr);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("options")]
        public async Task<IActionResult> GetPlaceOptions()
        {
            var sr = await _placeService.GetPlaceOptionsAsync();
            return this.ToActionResult(sr);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePlace([FromBody] PlaceCreateDTO dto)
        {
            var sr = await _placeService.CreatePlaceAsync(dto);

            if (!sr.Success)
                return this.ToActionResult(sr);

            var created = sr.Data!;
            return CreatedAtAction(nameof(GetPlaceById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePlace(int id, [FromBody] PlaceUpdateDTO dto)
        {
            var sr = await _placeService.UpdatePlaceAsync(id, dto);
            return this.ToActionResult(sr);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdatePlacePartial(int id, [FromBody] PlacePatchUpdateDTO dto)
        {
            var sr = await _placeService.UpdatePlacePartialAsync(id, dto);
            return this.ToActionResult(sr);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePlace(int id)
        {
            var sr = await _placeService.DeletePlaceAsync(id);

            if (!sr.Success)
                return this.ToActionResult(sr);

            return NoContent();
        }
    }
}
