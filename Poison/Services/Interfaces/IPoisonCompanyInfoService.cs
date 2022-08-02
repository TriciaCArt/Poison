using Poison.Models;

namespace Poison.Services.Interfaces
{
    public interface IPoisonCompanyInfoService
    {
        public Task<List<BTUser>> GetAllMembersAsync(int companyId);

        public Task<Company> GetCompanyInfoById(int? companyId);

        public Task<List<Project>> GetAllProjectsAsync(int? companyId);

    }
}
