using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PremiumPlace.DTO;
using PremiumPlace_API.Data;
using PremiumPlace_API.Infrastructure.Payments.PayPal;
using PremiumPlace_API.Models;
using PremiumPlace_API.Services.Auth;
using PremiumPlace_API.Services.Bookings;
using PremiumPlace_API.Services.PayPal;
using PremiumPlace_API.Services.Places;
using Scalar.AspNetCore;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<Place, PlaceCreateDTO>().ReverseMap();
    cfg.CreateMap<Place, PlaceUpdateDTO>().ReverseMap();

    cfg.CreateMap<PlaceFeatures, PlaceFeaturesDTO>().ReverseMap();

    cfg.CreateMap<Amenity, AmenityDTO>().ReverseMap();
    cfg.CreateMap<City, CityDTO>().ReverseMap();

    cfg.CreateMap<Review, ReviewDTO>()
        .ForMember(d => d.Username, opt => opt.MapFrom(r => r.User.Username))
        .ReverseMap();

    cfg.CreateMap<Place, PlaceDTO>()
        .ForMember(d => d.City, opt => opt.MapFrom(p => p.City.Name))
        .ForMember(d => d.Amenitys, opt => opt.MapFrom(p => p.Amenitys.Select(a => a.Name).ToList()))
        .ForMember(d => d.Features, opt => opt.MapFrom(p => p.Features))
        .ForMember(d => d.ReviewSummary, opt => opt.MapFrom(s =>
            s.Reviews.Any()
                ? new ReviewSummaryDTO
                {
                    Count = s.Reviews.Count,
                    Avg = Math.Round(s.Reviews.Average(r => r.Rating), 1)
                }
                : new ReviewSummaryDTO { Count = 0, Avg = 0 }
        ))
        .ReverseMap();

    cfg.CreateMap<Place, PlaceDetailsDTO>()
    .ForMember(d => d.City, opt => opt.MapFrom(s => s.City.Name))
    .ForMember (d => d.Amenitys, opt => opt.MapFrom(s => s.Amenitys.Select(a => a.Name).ToList()))
    .ForMember(d => d.Reviews, opt => opt.MapFrom(s => s.Reviews))
    .ForMember(d => d.ReviewSummary, opt => opt.MapFrom(s =>
        s.Reviews.Any()
            ? new ReviewSummaryDTO
            {
                Count = s.Reviews.Count,
                Avg = Math.Round(s.Reviews.Average(r => r.Rating), 1)
            }
            : new ReviewSummaryDTO { Count = 0, Avg = 0 }
    ));
});

// JwtOptions от config
// JwtOptions from config
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

if (string.IsNullOrWhiteSpace(jwt.SigningKey))
    throw new InvalidOperationException("Jwt:SigningKey is missing. Check appsettings.Development.json / env / user-secrets.");


// Authentication + JWT bearer
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Important: read access token from HttpOnly cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Cookies["pp_access"];
                if (!string.IsNullOrWhiteSpace(accessToken))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClients", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:55120", // Angular dev
                "https://localhost:55120", // Angular dev
                "http://localhost:7073",  // MVC dev
                "https://localhost:7073"  // MVC dev
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("PayPal"));

// Named HttpClient за PayPal
builder.Services.AddHttpClient(PayPalHttpClient.HttpClientName, (sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PayPalOptions>>().Value;
    client.BaseAddress = options.BaseUri;
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("PremiumPlaceAPI/1.0");
})
.ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));


builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddScoped<IPayPalAuthClient, PayPalAuthClient>();
builder.Services.AddScoped<IPayPalOrdersClient, PayPalOrdersClient>();
builder.Services.AddScoped<IPayPalPaymentVerifier, PayPalPaymentVerifier>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, passwordService);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("FrontendClients");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
