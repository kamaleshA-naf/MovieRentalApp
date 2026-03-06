using MovieRentalApp.Exceptions;
using MovieRentalApp.Interfaces;
using MovieRentalApp.Models;
using MovieRentalApp.Models.DTOs;

namespace MovieRentalApp.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<int, User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;

        public UserService(
            IRepository<int, User> userRepository,
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
            // Step 1 - Check duplicate email
            var existing = await _userRepository
                .FindAsync(u => u.UserEmail == dto.Email);
            if (existing.Any())
                throw new DuplicateEntityException(
                    $"A user with email '{dto.Email}' already exists.");

            // Step 2 - Hash password
            var hashedPassword = _passwordService
                .HashPassword(dto.Password, null, out byte[]? hashkey);

            // Step 3 - Create user
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

            // Step 4 - Save
            var created = await _userRepository.AddAsync(user);
            return MapToDto(created);
        }

        // ── Login ─────────────────────────────────────────────────
        public async Task<LoginResponseDto> Login(LoginDto dto)
        {
            // Step 1 - Find user by email
            var users = await _userRepository
                .FindAsync(u => u.UserEmail == dto.Email);
            var user = users.FirstOrDefault();
            if (user == null)
                throw new EntityNotFoundException("Invalid email or password.");

            // Step 2 - Check if active
            if (!user.IsActive)
                throw new UnauthorizedException(
                    "Your account has been deactivated.");

            // Step 3 - Verify password
            var hashedPassword = _passwordService
                .HashPassword(dto.Password, user.PasswordSaltValue, out _);
            if (!hashedPassword.SequenceEqual(user.Password))
                throw new UnauthorizedException("Invalid email or password.");

            // Step 4 - Generate token
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

        // ── Get User ──────────────────────────────────────────────
        public async Task<UserResponseDto> GetUser(int id)
        {
            // Step 1 - Find user
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new EntityNotFoundException("User", id);

            return MapToDto(user);
        }

        // ── Get All Users ─────────────────────────────────────────
        public async Task<IEnumerable<UserResponseDto>> GetAllUsers()
        {
            // Step 1 - Get all
            var users = await _userRepository.GetAllAsync();
            if (!users.Any())
                throw new EntityNotFoundException("No users found.");

            return users.Select(MapToDto);
        }

        // ── Update User ───────────────────────────────────────────
        public async Task<UserResponseDto> UpdateUser(int id, UserUpdateDto dto)
        {
            // Step 1 - Find user
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new EntityNotFoundException("User", id);

            // Step 2 - Check email uniqueness
            //if (dto.Email != null && dto.Email != user.UserEmail)
            //{
            //    var emailExists = await _userRepository
            //        .FindAsync(u => u.UserEmail == dto.Email);
            //    if (emailExists.Any())
            //        throw new DuplicateEntityException(
            //            $"Email '{dto.Email}' is already taken.");

            //    user.UserEmail = dto.Email;
            //}

            // Step 3 - Update name
            if (dto.Name != null)
                user.UserName = dto.Name;

            // Step 4 - Save
            var updated = await _userRepository.UpdateAsync(id, user);
            if (updated == null)
                throw new UnableToCreateEntityException("User", "Update failed.");

            return MapToDto(updated);
        }

        // ── Delete User ───────────────────────────────────────────
        public async Task<UserResponseDto> DeleteUser(int id)
        {
            // Step 1 - Find user
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new EntityNotFoundException("User", id);

            // Step 2 - Delete
            await _userRepository.DeleteAsync(id);
            return MapToDto(user);
        }
        // ── Change Password ───────────────────────────────────────
        public async Task<string> ChangePassword(ChangePasswordDto dto)
        {
            // Step 1 - Validate userId
            if (dto.UserId <= 0)
                throw new BusinessRuleViolationException(
                    "Invalid user ID.");

            // Step 2 - Validate new password
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new BusinessRuleViolationException(
                    "New password cannot be empty.");

            // Step 3 - Find user
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new EntityNotFoundException("User", dto.UserId);

            // Step 4 - Verify old password
            var hashedOldPassword = _passwordService.HashPassword(
                dto.OldPassword,
                user.PasswordSaltValue,
                out _);
            if (!hashedOldPassword.SequenceEqual(user.Password))
                throw new UnauthorizedException(
                    "Old password is incorrect.");

            // Step 5 - Check new password is not same as old
            if (dto.OldPassword == dto.NewPassword)
                throw new BusinessRuleViolationException(
                    "New password cannot be same as old password.");

            // Step 6 - Hash new password
            var hashedNewPassword = _passwordService.HashPassword(
                dto.NewPassword,
                null,
                out byte[]? newHashKey);

            // Step 7 - Update in DB
            user.Password = hashedNewPassword;
            user.PasswordSaltValue = newHashKey!;
            await _userRepository.UpdateAsync(user.UserId, user);

            // Step 8 - Return success
            return "Password changed successfully.";
        }


        // ── Mapper ────────────────────────────────────────────────
        private static UserResponseDto MapToDto(User user) => new()
        {
            Id = user.UserId,
            Name = user.UserName,
            Email = user.UserEmail,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}