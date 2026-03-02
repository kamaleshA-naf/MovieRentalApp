using Microsoft.IdentityModel.Tokens;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieRentalApp.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration configuration)
        {
            // Step 1 - Validate config
            var secret = configuration["Keys:Jwt"];
            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException(
                    "JWT key is missing from configuration.");

            // Step 2 - Build signing key
            _key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));
        }

        public string CreateToken(TokenPayloadDto payload)
        {
            // Step 1 - Validate payload
            if (payload == null)
                throw new ArgumentNullException(nameof(payload),
                    "Token payload cannot be null.");

            // Step 2 - Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
                new Claim(ClaimTypes.Name,           payload.UserName),
                new Claim(ClaimTypes.Role,           payload.Role)
            };

            // Step 3 - Build credentials
            var creds = new SigningCredentials(
                _key, SecurityAlgorithms.HmacSha256);

            // Step 4 - Build token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds,
                Issuer = "MovieRentalApp",
                Audience = "MovieRentalApp"
            };

            // Step 5 - Create and return token
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
    }
}