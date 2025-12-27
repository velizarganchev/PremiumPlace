using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PremiumPlace_API.Controllers.Extensions;
using PremiumPlace_API.Data;
using PremiumPlace.DTO;
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
        public async Task<IActionResult> GetPlaces()
        {
            var sr = await _placeService.GetAllPlacesAsync();
            return this.ToActionResult(sr);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPlaceById(int id)
        {
            var sr = await _placeService.GetPlaceByIdAsync(id);
            return this.ToActionResult(sr);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlace([FromBody] PlaceCreateDTO dto)
        {
            var sr = await _placeService.CreatePlaceAsync(dto);

            if (!sr.Success)
                return this.ToActionResult(sr);

            // ако service връща PlaceDTO с Id -> връщаме Created към GetPlaceById
            var created = sr.Data!;
            return CreatedAtAction(nameof(GetPlaceById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePlace(int id, [FromBody] PlaceUpdateDTO dto)
        {
            var sr = await _placeService.UpdatePlaceAsync(id, dto);
            return this.ToActionResult(sr);
        }

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
