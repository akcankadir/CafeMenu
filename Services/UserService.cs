using System.Threading.Tasks;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using CafeMenu.Models;
using Dapper;

namespace CafeMenu.Services
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;

        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User> ValidateUserAsync(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@username", username);
                parameters.Add("@password", password);

                var userDictionary = new Dictionary<int, User>();

                var users = await connection.QueryAsync<User, Role, User>(
                    "ValidateUser",
                    (user, role) =>
                    {
                        if (!userDictionary.TryGetValue(user.UserId, out User userEntry))
                        {
                            userEntry = user;
                            userEntry.UserRoles = new List<UserRole>();
                            userDictionary.Add(user.UserId, userEntry);
                        }

                        if (role != null)
                        {
                            userEntry.UserRoles.Add(new UserRole { Role = role });
                        }

                        return userEntry;
                    },
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    splitOn: "RoleId");

                return userDictionary.Values.FirstOrDefault();
            }
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@username", user.Username);
                parameters.Add("@password", password);
                parameters.Add("@email", user.Email);
                parameters.Add("@firstName", user.Name);
                parameters.Add("@lastName", user.Surname);

                var userId = await connection.ExecuteScalarAsync<int>(
                    "CreateUser",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                user.UserId = userId;
                return user;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);
                parameters.Add("@oldPassword", oldPassword);
                parameters.Add("@newPassword", newPassword);

                var result = await connection.ExecuteScalarAsync<int>(
                    "UpdatePassword",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result == 1;
            }
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var count = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM Users WHERE Username = @username AND IsActive = 1",
                    new { username });

                return count == 0;
            }
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var userDictionary = new Dictionary<int, User>();

                var users = await connection.QueryAsync<User, Role, User>(
                    "GetUsers",
                    (user, role) =>
                    {
                        if (!userDictionary.TryGetValue(user.UserId, out User userEntry))
                        {
                            userEntry = user;
                            userEntry.UserRoles = new List<UserRole>();
                            userDictionary.Add(user.UserId, userEntry);
                        }

                        if (role != null)
                        {
                            userEntry.UserRoles.Add(new UserRole { Role = role });
                        }

                        return userEntry;
                    },
                    commandType: CommandType.StoredProcedure,
                    splitOn: "RoleId");

                return userDictionary.Values;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);

                var userDictionary = new Dictionary<int, User>();

                var users = await connection.QueryAsync<User, Role, User>(
                    "GetUserById",
                    (user, role) =>
                    {
                        if (!userDictionary.TryGetValue(user.UserId, out User userEntry))
                        {
                            userEntry = user;
                            userEntry.UserRoles = new List<UserRole>();
                            userDictionary.Add(user.UserId, userEntry);
                        }

                        if (role != null)
                        {
                            userEntry.UserRoles.Add(new UserRole { Role = role });
                        }

                        return userEntry;
                    },
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    splitOn: "RoleId");

                return userDictionary.Values.FirstOrDefault();
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", user.UserId);
                parameters.Add("@email", user.Email);
                parameters.Add("@firstName", user.Name);
                parameters.Add("@lastName", user.Surname);
                parameters.Add("@isActive", user.IsActive);

                await connection.ExecuteAsync(
                    "UpdateUser",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return user;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);

                await connection.ExecuteAsync(
                    "DeleteUser",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return true;
            }
        }

        public async Task<bool> UpdateUserRolesAsync(int userId, int[] roleIds)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);
                parameters.Add("@roleIds", string.Join(",", roleIds));

                await connection.ExecuteAsync(
                    "UpdateUserRoles",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return true;
            }
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<Role>(
                    "GetAllRoles",
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<User> UpdateProfileAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", user.UserId);
                parameters.Add("@email", user.Email);
                parameters.Add("@firstName", user.Name);
                parameters.Add("@lastName", user.Surname);

                var userDictionary = new Dictionary<int, User>();

                var users = await connection.QueryAsync<User, Role, User>(
                    "UpdateProfile",
                    (u, role) =>
                    {
                        if (!userDictionary.TryGetValue(u.UserId, out User userEntry))
                        {
                            userEntry = u;
                            userEntry.UserRoles = new List<UserRole>();
                            userDictionary.Add(u.UserId, userEntry);
                        }

                        if (role != null)
                        {
                            userEntry.UserRoles.Add(new UserRole { Role = role });
                        }

                        return userEntry;
                    },
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    splitOn: "RoleId");

                return userDictionary.Values.FirstOrDefault();
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? userId = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@email", email);
                parameters.Add("@userId", userId);

                return await connection.ExecuteScalarAsync<bool>(
                    "IsEmailUnique",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
} 