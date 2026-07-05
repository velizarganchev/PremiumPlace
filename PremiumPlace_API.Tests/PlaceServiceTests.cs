using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PremiumPlace.DTO;
using PremiumPlace_API.Models;
using PremiumPlace_API.Services.Places;
using PremiumPlace_API.Tests.Helpers;
using Xunit;

namespace PremiumPlace_API.Tests;

public class PlaceServiceTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly IMapper _mapper;

    public PlaceServiceTests()
    {
        _factory = new TestDbContextFactory();
        _mapper = CreateMapper();
        SeedBaseData();
    }

    [Fact]
    public async Task UpdatePlace_ExistingCity_UpdatesAmenitiesAndFeaturesWithoutCreatingNullCity()
    {
        using var db = _factory.CreateContext();
        var svc = new PlaceService(db, _mapper);

        var result = await svc.UpdatePlaceAsync(1, new PlaceUpdateDTO
        {
            Id = 1,
            Name = "Updated Place",
            Details = "Updated details",
            GuestCapacity = 6,
            Rate = 175m,
            Beds = 3,
            CheckInHour = 15,
            CheckOutHour = 10,
            SquareFeet = 900,
            ImageUrl = "https://example.com/updated.jpg",
            CityId = 1,
            Features = new PlaceFeaturesDTO
            {
                Internet = true,
                AirConditioned = true,
                PetsAllowed = true,
                Parking = true,
                Kitchen = true,
                Washer = true
            },
            AmenityIds = new List<int> { 2 }
        });

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated Place", result.Data.Name);
        Assert.Equal(1, result.Data.CityId);
        Assert.Equal(new List<int> { 2 }, result.Data.AmenityIds);
        Assert.True(result.Data.Features.Internet);
        Assert.True(result.Data.Features.AirConditioned);
        Assert.True(result.Data.Features.PetsAllowed);
        Assert.True(result.Data.Features.Parking);
        Assert.True(result.Data.Features.Kitchen);
        Assert.True(result.Data.Features.Washer);

        var cityCount = await db.Cities.CountAsync();
        Assert.Equal(1, cityCount);
        Assert.DoesNotContain(await db.Cities.ToListAsync(), c => string.IsNullOrWhiteSpace(c.Name));

        var persisted = await db.Places
            .Include(p => p.Amenitys)
            .FirstAsync(p => p.Id == 1);

        Assert.Single(persisted.Amenitys);
        Assert.Equal(2, persisted.Amenitys.Single().Id);
        Assert.True(persisted.Features.AirConditioned);
        Assert.True(persisted.Features.PetsAllowed);
    }

    private void SeedBaseData()
    {
        using var db = _factory.CreateContext();

        db.Cities.Add(new City { Id = 1, Name = "Berlin" });

        db.Amenitys.AddRange(
            new Amenity { Id = 1, Name = "Free Wi-Fi" },
            new Amenity { Id = 2, Name = "Parking" }
        );

        db.Places.Add(new Place
        {
            Id = 1,
            Name = "Original Place",
            Details = "Original details",
            CityId = 1,
            GuestCapacity = 2,
            Rate = 100m,
            Beds = 1,
            CheckInHour = 14,
            CheckOutHour = 11,
            SquareFeet = 500,
            ImageUrl = "https://example.com/original.jpg",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Features = new PlaceFeatures { Internet = true }
        });

        db.SaveChanges();
    }

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PlaceCreateDTO, Place>()
                .ForMember(d => d.City, opt => opt.Ignore())
                .ForMember(d => d.Amenitys, opt => opt.Ignore())
                .ForMember(d => d.Reviews, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

            cfg.CreateMap<PlaceUpdateDTO, Place>()
                .ForMember(d => d.City, opt => opt.Ignore())
                .ForMember(d => d.Amenitys, opt => opt.Ignore())
                .ForMember(d => d.Reviews, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

            cfg.CreateMap<PlaceFeatures, PlaceFeaturesDTO>().ReverseMap();
            cfg.CreateMap<Amenity, AmenityDTO>().ReverseMap();
            cfg.CreateMap<City, CityDTO>().ReverseMap();
            cfg.CreateMap<Review, ReviewDTO>()
                .ForMember(d => d.Username, opt => opt.MapFrom(r => r.User.Username))
                .ReverseMap();

            cfg.CreateMap<Place, PlaceDTO>()
                .ForMember(d => d.City, opt => opt.MapFrom(p => p.City.Name))
                .ForMember(d => d.Amenitys, opt => opt.MapFrom(p => p.Amenitys.Select(a => a.Name).ToList()))
                .ForMember(d => d.AmenityIds, opt => opt.MapFrom(p => p.Amenitys.Select(a => a.Id).ToList()))
                .ForMember(d => d.Features, opt => opt.MapFrom(p => p.Features))
                .ForMember(d => d.ReviewSummary, opt => opt.MapFrom(s =>
                    s.Reviews.Any()
                        ? new ReviewSummaryDTO
                        {
                            Count = s.Reviews.Count,
                            Avg = Math.Round(s.Reviews.Average(r => r.Rating), 1)
                        }
                        : new ReviewSummaryDTO { Count = 0, Avg = 0 }
                ));
        }, NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
