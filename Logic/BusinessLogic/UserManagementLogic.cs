using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class UserManagementLogic : IUserManagementLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogWriter _auditLogWriter;

        private static readonly HashSet<long> ValidRoleIds = new()
        {
            ERole.Complainant, ERole.SapsOfficer, ERole.StationCommander, ERole.SystemAdministrator
        };

        public UserManagementLogic(ApplicationDbContext context, IAuditLogWriter auditLogWriter)
        {
            _context = context;
            _auditLogWriter = auditLogWriter;
        }

        // Officers and commanders are tied to a physical station; complainants and
        // administrators are not, so the requirement is conditional on the role.
        private static bool RequiresStation(long roleId) => roleId == ERole.SapsOfficer || roleId == ERole.StationCommander;

        private async Task<string?> ValidateRoleAndStationAsync(long roleId, long? stationId)
        {
            if (!ValidRoleIds.Contains(roleId))
            {
                return "Invalid role id.";
            }

            if (RequiresStation(roleId))
            {
                if (stationId == null)
                {
                    return "Station id is required for SAPS Officer and Station Commander accounts.";
                }

                var stationExists = await _context.Stations.AnyAsync(s => s.Id == stationId);
                if (!stationExists)
                {
                    return "Station not found.";
                }
            }

            return null;
        }

        public async Task<ResultModel<User>> CreateUserAsync(long performedByAdminId, CreateUserDTO dto)
        {
            try
            {
                var validationError = await ValidateRoleAndStationAsync(dto.RoleId, dto.StationId);
                if (validationError != null)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = validationError };
                }

                var requiresStation = RequiresStation(dto.RoleId);

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email || u.IdNumber == dto.IdNumber);
                if (existingUser != null)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = "A user with this email or ID number already exists." };
                }

                var user = new User
                {
                    FullName = dto.FullName,
                    IdNumber = dto.IdNumber,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = dto.RoleId,
                    StationId = requiresStation ? dto.StationId : null,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = performedByAdminId,
                    ActionType = $"CreatedUser:{user.Id}:{ERole.GetName(user.RoleId)}",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel<User> { ResultId = EResultValue.Success, Message = "User created successfully.", Data = user };
            }
            catch (Exception ex)
            {
                return new ResultModel<User> { ResultId = EResultValue.Fail, Message = $"An error occurred while creating the user: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Include(u => u.NavStation).OrderBy(u => u.FullName).ToListAsync();
        }

        public async Task<PagedResult<User>> GetAllPagedAsync(int page, int pageSize, long? roleId = null, long? stationId = null, bool? isActive = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Users.Include(u => u.NavStation).AsQueryable();

            if (roleId != null)
            {
                query = query.Where(u => u.RoleId == roleId);
            }
            if (stationId != null)
            {
                query = query.Where(u => u.StationId == stationId);
            }
            if (isActive != null)
            {
                query = query.Where(u => u.IsActive == isActive);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<User> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<User?> GetByIdAsync(long id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ResultModel<User>> UpdateUserAsync(long performedByAdminId, long targetUserId, UpdateUserDTO dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
                if (user == null)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = "User not found." };
                }

                var validationError = await ValidateRoleAndStationAsync(dto.RoleId, dto.StationId);
                if (validationError != null)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = validationError };
                }

                var duplicateEmail = await _context.Users.AnyAsync(u => u.Id != targetUserId && u.Email == dto.Email);
                if (duplicateEmail)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = "Another user already uses this email." };
                }

                user.FullName = dto.FullName;
                user.Email = dto.Email;
                user.PhoneNumber = dto.PhoneNumber;
                user.RoleId = dto.RoleId;
                user.StationId = RequiresStation(dto.RoleId) ? dto.StationId : null;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = performedByAdminId,
                    ActionType = $"UpdatedUser:{user.Id}",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel<User> { ResultId = EResultValue.Success, Message = "User updated successfully.", Data = user };
            }
            catch (Exception ex)
            {
                return new ResultModel<User> { ResultId = EResultValue.Fail, Message = $"An error occurred while updating the user: {ex.Message}" };
            }
        }

        public async Task<ResultModel<User>> UpdateOwnProfileAsync(long userId, UpdateProfileDTO dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = "User not found." };
                }

                var duplicateEmail = await _context.Users.AnyAsync(u => u.Id != userId && u.Email == dto.Email);
                if (duplicateEmail)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = "Another user already uses this email." };
                }

                // Changing the address on file means whoever's typing it now
                // hasn't proven they can read mail sent there yet — the caller
                // is responsible for sending a fresh verification email
                // whenever this comes back with the email actually changed.
                if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.EmailVerified = false;
                }

                user.FullName = dto.FullName;
                user.Email = dto.Email;
                user.PhoneNumber = dto.PhoneNumber;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = userId,
                    ActionType = "UpdatedOwnProfile",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel<User> { ResultId = EResultValue.Success, Message = "Profile updated successfully.", Data = user };
            }
            catch (Exception ex)
            {
                return new ResultModel<User> { ResultId = EResultValue.Fail, Message = $"An error occurred while updating your profile: {ex.Message}" };
            }
        }

        public async Task<ResultModel> ChangePasswordAsync(long userId, ChangePasswordDTO dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return new ResultModel { ResultId = EResultValue.Fail, Message = "User not found." };
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    return new ResultModel { ResultId = EResultValue.Fail, Message = "Your current password is incorrect." };
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = userId,
                    ActionType = "ChangedOwnPassword",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel { ResultId = EResultValue.Success, Message = "Password changed successfully." };
            }
            catch (Exception ex)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = $"An error occurred while changing your password: {ex.Message}" };
            }
        }

        public async Task<ResultModel<User>> SetActiveStatusAsync(long performedByAdminId, long targetUserId, bool isActive)
        {
            try
            {
                if (targetUserId == performedByAdminId && !isActive)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = "You cannot deactivate your own account." };
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
                if (user == null)
                {
                    return new ResultModel<User> { ResultId = EResultValue.Fail, Message = "User not found." };
                }

                user.IsActive = isActive;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = performedByAdminId,
                    ActionType = isActive ? $"ActivatedUser:{user.Id}" : $"DeactivatedUser:{user.Id}",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel<User> { ResultId = EResultValue.Success, Message = isActive ? "User activated successfully." : "User deactivated successfully.", Data = user };
            }
            catch (Exception ex)
            {
                return new ResultModel<User> { ResultId = EResultValue.Fail, Message = $"An error occurred while updating the user's status: {ex.Message}" };
            }
        }
    }
}
