using Microsoft.AspNetCore.Identity;
using Poison.Models;

namespace Poison.Services.Interfaces
{
    public interface IPoisonRolesService
    {
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);

        public Task<List<IdentityRole>> GetPoisonRolesAsync();

        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName);

        public Task<string> GetRoleNameByIdAsync(string roleId);

        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);

        public Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId);

        public Task<string> GetUserRoleAsync(BTUser user);

        public Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);

        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);

        public Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles);
    }
}
