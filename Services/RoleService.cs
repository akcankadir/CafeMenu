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
    public class RoleService : IRoleService
    {
        private readonly string _connectionString;

        public RoleService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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

        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleId", roleId);

                var roleDictionary = new Dictionary<int, Role>();

                var roles = await connection.QueryAsync<Role, User, Role>(
                    "GetRoleById",
                    (role, user) =>
                    {
                        if (!roleDictionary.TryGetValue(role.RoleId, out Role roleEntry))
                        {
                            roleEntry = role;
                            roleEntry.UserRoles = new List<UserRole>();
                            roleDictionary.Add(role.RoleId, roleEntry);
                        }

                        if (user != null)
                        {
                            roleEntry.UserRoles.Add(new UserRole { User = user });
                        }

                        return roleEntry;
                    },
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    splitOn: "UserId");

                return roleDictionary.Values.FirstOrDefault();
            }
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleName", role.Name);
                parameters.Add("@description", role.Description);

                var roleId = await connection.ExecuteScalarAsync<int>(
                    "CreateRole",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                role.RoleId = roleId;
                return role;
            }
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleId", role.RoleId);
                parameters.Add("@roleName", role.Name);
                parameters.Add("@description", role.Description);

                await connection.ExecuteAsync(
                    "UpdateRole",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return role;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleId", roleId);

                await connection.ExecuteAsync(
                    "DeleteRole",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return true;
            }
        }

        public async Task<bool> IsRoleNameUniqueAsync(string roleName, int? roleId = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@roleName", roleName);
                parameters.Add("@roleId", roleId);

                return await connection.ExecuteScalarAsync<bool>(
                    "IsRoleNameUnique",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
} 