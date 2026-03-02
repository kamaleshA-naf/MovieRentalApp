using MovieRentalApp.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MovieRentalApp.Services
{
    public class PasswordService : IPasswordService
    {
        public byte[] HashPassword(
            string password,
            byte[]? dbHashKey,
            out byte[]? hashkey)
        {
            // Step 1 - Validate input
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException(
                    "Password cannot be null or empty.",
                    nameof(password));

            HMACSHA256 hmac;

            // Step 2 - Generate or use existing key
            if (dbHashKey == null)
            {
                // New registration → generate new key
                hmac = new HMACSHA256();
                hashkey = hmac.Key;
            }
            else
            {
                // Login → use stored key
                hmac = new HMACSHA256(dbHashKey);
                hashkey = null;
            }

            // Step 3 - Hash the password
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashedPassword = hmac.ComputeHash(passwordBytes);

            // Step 4 - Dispose and return
            hmac.Dispose();
            return hashedPassword;
        }
    }
}