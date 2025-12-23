using Microsoft.EntityFrameworkCore;
using PremiumPlace_API.Data;
using PremiumPlace_API.Models;
using PremiumPlace_API.Models.DTO;
using PremiumPlace_API.Services.Places;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(o =>
{
    o.CreateMap<Place, PlaceCreateDTO>().ReverseMap();
    o.CreateMap<Place, PlaceUpdateDTO>().ReverseMap();
    o.CreateMap<Place, PlaceDTO>().ReverseMap();
});
builder.Services.AddScoped<IPlaceService, PlaceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
