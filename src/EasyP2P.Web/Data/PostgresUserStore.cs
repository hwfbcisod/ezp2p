using EasyP2P.Web.Data.Models;
using EasyP2P.Web.Enums;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Data;
using System.Security.Claims;

namespace EasyP2P.Web.Data.Stores;

public class PostgresUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>, IUserClaimStore<ApplicationUser>
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresUserStore> _logger;

    public PostgresUserStore(IConfiguration configuration, ILogger<PostgresUserStore> logger)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnectionString")
            ?? throw new ArgumentNullException("Connection string not found");
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = @"
                INSERT INTO users (id, username, normalized_username, email, normalized_email, 
                                 email_confirmed, password_hash, security_stamp, phone_number, 
                                 phone_number_confirmed, two_factor_enabled, lockout_end, 
                                 lockout_enabled, access_failed_count, first_name, last_name, 
                                 department, role, created_date, is_active)
                VALUES (@id, @username, @normalized_username, @email, @normalized_email, 
                        @email_confirmed, @password_hash, @security_stamp, @phone_number, 
                        @phone_number_confirmed, @two_factor_enabled, @lockout_end, 
                        @lockout_enabled, @access_failed_count, @first_name, @last_name, 
                        @department, @role, @created_date, @is_active)";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("id", user.Id);
            command.Parameters.AddWithValue("username", user.UserName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("normalized_username", user.NormalizedUserName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("email", user.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("normalized_email", user.NormalizedEmail ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("email_confirmed", user.EmailConfirmed);
            command.Parameters.AddWithValue("password_hash", user.PasswordHash ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("security_stamp", user.SecurityStamp ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("phone_number", user.PhoneNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("phone_number_confirmed", user.PhoneNumberConfirmed);
            command.Parameters.AddWithValue("two_factor_enabled", user.TwoFactorEnabled);
            command.Parameters.AddWithValue("lockout_end", user.LockoutEnd?.UtcDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("lockout_enabled", user.LockoutEnabled);
            command.Parameters.AddWithValue("access_failed_count", user.AccessFailedCount);
            command.Parameters.AddWithValue("first_name", user.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("last_name", user.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("department", user.Department ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("role", user.Role.ToString());
            command.Parameters.AddWithValue("created_date", user.CreatedDate);
            command.Parameters.AddWithValue("is_active", user.IsActive);

            await command.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", user.Email);
            return IdentityResult.Failed(new IdentityError { Description = "Failed to create user" });
        }
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = @"
                UPDATE users SET 
                    username = @username, normalized_username = @normalized_username, 
                    email = @email, normalized_email = @normalized_email, 
                    email_confirmed = @email_confirmed, password_hash = @password_hash, 
                    security_stamp = @security_stamp, phone_number = @phone_number, 
                    phone_number_confirmed = @phone_number_confirmed, 
                    two_factor_enabled = @two_factor_enabled, lockout_end = @lockout_end, 
                    lockout_enabled = @lockout_enabled, access_failed_count = @access_failed_count, 
                    first_name = @first_name, last_name = @last_name, department = @department, 
                    role = @role, is_active = @is_active
                WHERE id = @id";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("id", user.Id);
            command.Parameters.AddWithValue("username", user.UserName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("normalized_username", user.NormalizedUserName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("email", user.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("normalized_email", user.NormalizedEmail ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("email_confirmed", user.EmailConfirmed);
            command.Parameters.AddWithValue("password_hash", user.PasswordHash ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("security_stamp", user.SecurityStamp ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("phone_number", user.PhoneNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("phone_number_confirmed", user.PhoneNumberConfirmed);
            command.Parameters.AddWithValue("two_factor_enabled", user.TwoFactorEnabled);
            command.Parameters.AddWithValue("lockout_end", user.LockoutEnd?.UtcDateTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("lockout_enabled", user.LockoutEnabled);
            command.Parameters.AddWithValue("access_failed_count", user.AccessFailedCount);
            command.Parameters.AddWithValue("first_name", user.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("last_name", user.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("department", user.Department ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("role", user.Role.ToString());
            command.Parameters.AddWithValue("is_active", user.IsActive);

            await command.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Email}", user.Email);
            return IdentityResult.Failed(new IdentityError { Description = "Failed to update user" });
        }
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = "DELETE FROM users WHERE id = @id";
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("id", user.Id);

            await command.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Email}", user.Email);
            return IdentityResult.Failed(new IdentityError { Description = "Failed to delete user" });
        }
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = "SELECT * FROM users WHERE id = @id";
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("id", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapToUser(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by id {UserId}", userId);
        }

        return null;
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = "SELECT * FROM users WHERE normalized_username = @normalized_username";
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("normalized_username", normalizedUserName);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapToUser(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by name {UserName}", normalizedUserName);
        }

        return null;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = "SELECT * FROM users WHERE normalized_email = @normalized_email";
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("normalized_email", normalizedEmail);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapToUser(reader);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by email {Email}", normalizedEmail);
        }

        return null;
    }

    // Password store methods
    public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    // Email store methods
    public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    // Basic user store methods
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    // Claims store methods
    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new("UserRole", user.Role.ToString()),
            new("UserId", user.Id),
            new("Department", user.Department ?? "")
        };

        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim("FirstName", user.FirstName));

        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim("LastName", user.LastName));

        return claims;
    }

    public Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        // For this simple implementation, we don't store additional claims separately
        // Role and other info are stored as user properties
        return Task.CompletedTask;
    }

    public Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        // Handle role updates
        if (claim.Type == "UserRole" && Enum.TryParse<UserRole>(newClaim.Value, out var newRole))
        {
            user.Role = newRole;
        }
        return Task.CompletedTask;
    }

    public Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        // Simple implementation - only handle role claims
        if (claim.Type == "UserRole" && Enum.TryParse<UserRole>(claim.Value, out var role))
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                var query = "SELECT * FROM users WHERE role = @role";
                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("role", role.ToString());

                var users = new List<ApplicationUser>();
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    users.Add(MapToUser(reader));
                }
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for claim {ClaimType}:{ClaimValue}", claim.Type, claim.Value);
            }
        }

        return new List<ApplicationUser>();
    }

    private ApplicationUser MapToUser(NpgsqlDataReader reader)
    {
        return new ApplicationUser
        {
            Id = reader.GetString("id"),
            UserName = reader.IsDBNull("username") ? null : reader.GetString("username"),
            NormalizedUserName = reader.IsDBNull("normalized_username") ? null : reader.GetString("normalized_username"),
            Email = reader.IsDBNull("email") ? null : reader.GetString("email"),
            NormalizedEmail = reader.IsDBNull("normalized_email") ? null : reader.GetString("normalized_email"),
            EmailConfirmed = reader.GetBoolean("email_confirmed"),
            PasswordHash = reader.IsDBNull("password_hash") ? null : reader.GetString("password_hash"),
            SecurityStamp = reader.IsDBNull("security_stamp") ? null : reader.GetString("security_stamp"),
            PhoneNumber = reader.IsDBNull("phone_number") ? null : reader.GetString("phone_number"),
            PhoneNumberConfirmed = reader.GetBoolean("phone_number_confirmed"),
            TwoFactorEnabled = reader.GetBoolean("two_factor_enabled"),
            LockoutEnd = reader.IsDBNull("lockout_end") ? null : DateTime.SpecifyKind(reader.GetDateTime("lockout_end"), DateTimeKind.Utc),
            LockoutEnabled = reader.GetBoolean("lockout_enabled"),
            AccessFailedCount = reader.GetInt32("access_failed_count"),
            FirstName = reader.IsDBNull("first_name") ? null : reader.GetString("first_name"),
            LastName = reader.IsDBNull("last_name") ? null : reader.GetString("last_name"),
            Department = reader.IsDBNull("department") ? null : reader.GetString("department"),
            Role = Enum.TryParse<UserRole>(reader.GetString("role"), out var role) ? role : UserRole.Requestor,
            CreatedDate = reader.GetDateTime("created_date"),
            IsActive = reader.GetBoolean("is_active")
        };
    }

    public void Dispose() { }
}