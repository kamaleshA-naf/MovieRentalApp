using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MovieRentalApp.Contexts;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Repositories;
using MovieRentalApp.Services;
using System.Text;
using AspNetCoreRateLimit;

namespace MovieRentalApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ──────────────────────────────────────────
            builder.Services.AddDbContext<MovieContext>(options =>
                options.UseSqlServer(
                    builder.Configuration
                           .GetConnectionString("Development")));

            // ── Repositories (Generic) ────────────────────────────

            builder.Services.AddScoped<IRepository<int, User>, Repository<int, User>>();
            builder.Services.AddScoped<IRepository<int, Movie>, Repository<int, Movie>>();
            builder.Services.AddScoped<IRepository<int, Rental>, Repository<int, Rental>>();
            builder.Services.AddScoped<IRepository<int, Payment>, Repository<int, Payment>>();
            builder.Services.AddScoped<IRepository<int, Wishlist>, Repository<int, Wishlist>>();
            builder.Services.AddScoped<IRepository<int, Genre>, Repository<int, Genre>>();
            builder.Services.AddScoped<IRepository<int, MovieGenre>, Repository<int, MovieGenre>>();
            builder.Services.AddScoped<IRepository<int, AuditLog>, Repository<int, AuditLog>>();
            builder.Services.AddScoped<IRepository<int, Notification>,Repository<int, Notification> > ();


            // ── Services ──────────────────────────────────────────
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRentalService, RentalService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IGenreService, GenreService>();

            // ── JWT Authentication ─────────────────────────────────
            var jwtKey = builder.Configuration["Keys:Jwt"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException(
                    "JWT Key is missing from appsettings.json.");

            builder.Services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "MovieRentalApp",
                            ValidAudience = "MovieRentalApp",
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtKey))
                        };
                });

            // ── Controllers ───────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // ── Swagger ───────────────────────────────────────────
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Movie Rental API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter your JWT token."
                    });

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id   = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
            });

            var app = builder.Build();

            // ── Pipeline ──────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        "/swagger/v1/swagger.json",
                        "Movie Rental API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();   // ← Before Authorization
            app.UseAuthorization();
            app.UseIpRateLimiting();
            app.MapControllers();
            app.Run();

        }
    }
}