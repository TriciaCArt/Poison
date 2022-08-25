using Microsoft.EntityFrameworkCore;
using Poison.Data;
using Poison.Models;
using Poison.Models.Enums;

namespace Poison.Services.Interfaces
{
    public class PoisonProjectService : IPoisonProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPoisonRolesService _poisonRoleService;

        public PoisonProjectService(ApplicationDbContext context, IPoisonRolesService poisonRoleService)
        {
            _context = context;
            _poisonRoleService = poisonRoleService;
        }

        //CRUD Create
        public async Task AddNewProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BTUser currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                try
                {
                    // Remove Project Managers
                    await RemoveProjectManagerAsync(projectId);

                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error removing current PM. - Error: {ex.Message}");
                    return false;
                }               
            }

            //Add the new PM
            try
            {
                await AddUserToProjectAsync(userId, projectId);
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BTUser? bTUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (bTUser != null)
            {
                Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                if(!await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project!.Members!.Add(bTUser);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        //CRUD Archive (Delete)
        #region Archive Project Async
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);

                foreach(Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            try
            {
                List<BTUser> developers = await GetProjectMemberByRoleAsync(projectId, nameof(PoisonRoles.Developer));
                List<BTUser> submitters = await GetProjectMemberByRoleAsync(projectId, nameof(PoisonRoles.Submitter));
                List<BTUser> admins = await GetProjectMemberByRoleAsync(projectId, nameof(PoisonRoles.Admin));

                List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

                return teamMembers;
            }
            catch (Exception)
            {

                throw;
            }
        }
        


        
        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project>? projects = new();
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
           

            //var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.Priority).Where(p => p.CompanyId == companyId);
            //return await applicationDbContext.ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            List<Project>? projects = new();
            try
            {
                projects = await _context.Projects.Where(p=>p.CompanyId == companyId)
                                                  .Include(p => p.ProjectPriority).ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            List<Project>? archivedprojects = new();
            archivedprojects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true).ToListAsync();

            return archivedprojects;
        }

        //CRUD - Read
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = new();

                project = await _context.Projects
                                    .Include(p => p.Tickets)!
                                        .ThenInclude(t => t.TicketPriority)
                                    .Include(p => p.Tickets)!
                                        .ThenInclude(t => t.TicketStatus)
                                    .Include(p => p.Tickets)!
                                        .ThenInclude(t => t.TicketType)
                                    .Include(p => p.Tickets)!
                                        .ThenInclude(t => t.DeveloperUser)
                                    .Include(p => p.Tickets)!
                                        .ThenInclude(t => t.SubmitterUser)
                                    .Include(p => p.Members)
                                    .Include(p => p.ProjectPriority)
                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                                    
                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(c => c.Members).FirstOrDefaultAsync(c => c.Id == projectId);

                foreach (BTUser member in project?.Members!)
                {
                    if (await _poisonRoleService.IsUserInRoleAsync(member, nameof(PoisonRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<List<BTUser>> GetProjectMemberByRoleAsync(int projectId, string roleName)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                List<BTUser> members = new();
                foreach(BTUser user in project.Members!)
                {
                    if (await _poisonRoleService.IsUserInRoleAsync(user, roleName))
                    {
                        members.Add(user);
                    }
                }
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> result = new();
            List<Project> projects = new();
            try
            {
                projects = await _context.Projects
                                         .Include(p => p.ProjectPriority)
                                         .Where(p => p.CompanyId == companyId).ToListAsync();
                foreach (Project project in projects)
                {
                    if ((await GetProjectMemberByRoleAsync(project.Id, nameof(PoisonRoles.ProjectManager))).Count == 0)
                    {
                        result.Add(project);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public async Task<List<Project>> GetUserProjectsAsync(string? userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users
                                                                .Include(u => u.Projects)!
                                                                    .ThenInclude(p=>p.Company)
                                                                .Include(u=>u.Projects)!
                                                                    .ThenInclude(p=>p.Members)!
                                                                .Include(u => u.Projects)!
                                                                    .ThenInclude(p => p.Tickets)
                                                                .Include(u => u.Projects)!
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t=>t.DeveloperUser)
                                                                .Include(u=>u.Projects)!
                                                                    .ThenInclude(t=>t.Tickets)
                                                                        .ThenInclude(t=>t.SubmitterUser)
                                                                .Include(u => u.Projects)!
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t => t.TicketPriority)
                                                                .Include(u => u.Projects)!
                                                                    .ThenInclude(p => p.Tickets)
                                                                        .ThenInclude(t => t.TicketStatus)
                                                                .Include(u=>u.Projects)!
                                                                    .ThenInclude(t=>t.Tickets)
                                                                        .ThenInclude(t=>t.TicketType)
                                                               .FirstOrDefaultAsync(u => u.Id! == userId)).Projects!.ToList();
                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            try
            {
                List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId) && u.CompanyId == companyId).ToListAsync();
                return users;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(m => m.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                bool result = false;

                if (project != null)
                {
                    result = project.Members!.Any(m => m.Id == userId);
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(m => m.Members).FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (BTUser member in project?.Members!)
                {
                    if(await _poisonRoleService.IsUserInRoleAsync(member, nameof(PoisonRoles.ProjectManager).ToString()))
                    {
                        await RemoveUserFromProjectAsync(member.Id, projectId); 
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser? bTUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project? project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if(await IsUserOnProjectAsync(userId, projectId))
                    {
                        project?.Members?.Remove(bTUser!);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    
                    throw;
                }
                
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public async Task RestoreArchivedProjectsAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        //CRUD Edit Project
        #region Update Project Async
        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            
        }
        #endregion
    }
}
