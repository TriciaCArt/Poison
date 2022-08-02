using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Poison.Data;
using Poison.Models;
using Poison.Services.Interfaces;

namespace Poison.Services
{
    public class PoisonCompanyInfoService : IPoisonCompanyInfoService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public PoisonCompanyInfoService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            try
            {
                List<BTUser>? members = new();

                members = (await _context.Companies.Include(c=>c.Members).FirstOrDefaultAsync(c=>c.Id == companyId))!.Members.ToList();

                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsAsync(int? companyId)
        {
            List<Project>? projects = new();

            try
            {
                projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                            .Include(p => p.Members)
                                            .Include(p => p.Tickets)!
                                                 .ThenInclude(t => t.Comments)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Attachments)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.History)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.Notifications)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.DeveloperUser)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.SubmitterUser)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)!
                                                .ThenInclude(t => t.TicketType)
                                            .Include(p => p.ProjectPriority)
                                        .ToListAsync();
                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Company> GetCompanyInfoById(int? companyId)
        {
            try
            {
                Company? company = new();

                if(companyId != null)
                {
                    company = await _context.Companies.Include(c => c.Members).Include(c => c.Projects).Include(c => c.Invites).FirstOrDefaultAsync(c => c.Id == companyId);

                }
                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
