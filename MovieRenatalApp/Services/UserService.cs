using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;
using MovieRentalApp.Repositories;

namespace MovieRentalApp.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;

        public UserService(
            UserRepository userRepository,
            ITokenService tokenService,
            IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        // ── Register ──────────────────────────────────────────────
        public async Task<UserResponseDto> Register(UserCreateDto dto)
        {
            if (await _userRepository.EmailExists(dto.Email))
                throw new DuplicateEntityException(
                    $"A user with email '{dto.Email}' already exists.");

            var hashedPassword = _passwordService.HashPassword(
                dto.Password, null, out byte[]? hashkey);

            var user = new User
            {
                UserName = dto.Name,
                UserEmail = dto.Email,
                Password = hashedPassword,
                PasswordSaltValue = hashkey!,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _userRepository.Add(user);
            if (created == null)
                throw new UnableToCreateEntityException("User");

            return MapToDto(created);
        }

        // ── Login ─────────────────────────────────────────────────
        public async Task<LoginResponseDto> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByEmail(dto.Email);
            if (user == null)
                throw new EntityNotFoundException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedException("Your account has been deactivated.");

            var hashedPassword = _passwordService.HashPassword(
                dto.Password, user.PasswordSaltValue, out _);

            if (!hashedPassword.SequenceEqual(user.Password))
                throw new UnauthorizedException("Invalid email or password.");

            var token = _tokenService.CreateToken(new TokenPayloadDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Role = user.Role.ToString()
            });

            return new LoginResponseDto
            {
                Token = token,
                Name = user.UserName,
                Email = user.UserEmail,
                Role = user.Role.ToString(),
                Message = "Login successful."
            };
        }

        // ── Get Single User ───────────────────────────────────────
        public async Task<UserResponseDto> GetUser(int id)
        {
            var user = await _userRepository.Get(id);
            if (user == null)
                throw new EntityNotFoundException("User", id);

            return MapToDto(user);
        }

        // ── Get All Users ─────────────────────────────────────────
        public async Task<IEnumerable<UserResponseDto>> GetAllUsers()
        {
            var users = await _userRepository.GetAll();
            if (users == null || !users.Any())
                throw new EntityNotFoundException("No users found.");

            return users.Select(MapToDto);
        }

        // ── Update User ───────────────────────────────────────────
        public async Task<UserResponseDto> UpdateUser(int id, UserUpdateDto dto)
        {
            var existing = await _userRepository.Get(id);
            if (existing == null)
                throw new EntityNotFoundException("User", id);

            if (dto.Email != null && dto.Email != existing.UserEmail)
            {
                if (await _userRepository.EmailExists(dto.Email))
                    throw new DuplicateEntityException(
                        $"Email '{dto.Email}' is already taken.");
                existing.UserEmail = dto.Email;
            }

            if (dto.Name != null) existing.UserName = dto.Name;

            var updated = await _userRepository.Update(id, existing);
            if (updated == null)
                throw new UnableToCreateEntityException("User", "Update failed.");

            return MapToDto(updated);
        }

        // ── Delete User ───────────────────────────────────────────
        public async Task<UserResponseDto> DeleteUser(int id)
        {
            var user = await _userRepository.Get(id);
            if (user == null)
                throw new EntityNotFoundException("User", id);

            var deleted = await _userRepository.Delete(id);
            return MapToDto(deleted!);
        }

        // ── Mapper ────────────────────────────────────────────────
        private static UserResponseDto MapToDto(User user) => new()
        {
            Id = user.UserId,       // ← UserId
            Name = user.UserName,     // ← UserName
            Email = user.UserEmail,    // ← UserEmail
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}