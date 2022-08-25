using Microsoft.EntityFrameworkCore;
using Poison.Data;
using Poison.Models;
using Poison.Models.Enums;
using Poison.Services.Interfaces;

namespace Poison.Services
{
    public class PoisonTicketService : IPoisonTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPoisonRolesService _poisonRolesService;
        private readonly IPoisonProjectService _projectService;


        public PoisonTicketService(ApplicationDbContext context, IPoisonRolesService poisonRolesService, IPoisonProjectService projectService)
        {
            _context = context;
            _poisonRolesService = poisonRolesService;
            _projectService = projectService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {

                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            List<Ticket> tickets = new List<Ticket>();

            tickets = await _context.Projects
                                        .Where(p => p.CompanyId == companyId)
                                        .SelectMany(p => p.Tickets!)
                                            .Include(p => p.Attachments)
                                            .Include(p => p.Comments)
                                            .Include(p => p.DeveloperUser)
                                            .Include(p => p.History)
                                            .Include(p => p.SubmitterUser)
                                            .Include(p => p.TicketPriority)
                                            .Include(p => p.TicketStatus)
                                            .Include(p => p.TicketType)
                                            .Include(p => p.Project)
                                            .Where(p => p.Archived == false)
                                        .ToListAsync();

            //var companyTickets = _context.Tickets
            //                                   .Include(t => t.DeveloperUser)
            //                                   .Include(t => t.Project)
            //                                   .Include(t => t.SubmitterUser)
            //                                   .Include(t => t.TicketPriority)
            //                                   .Include(t => t.TicketStatus)
            //                                   .Include(t => t.TicketType)
            //                                   .Where(t => t.ProjectId == companyId);
            //return await companyTickets.ToListAsync();

            return tickets;
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = new List<Ticket>();

                tickets = await _context.Projects
                                            .Where(p => p.CompanyId == companyId)
                                            .SelectMany(p => p.Tickets!)
                                                .Include(p => p.Attachments)
                                                .Include(p => p.Comments)
                                                .Include(p => p.DeveloperUser)
                                                .Include(p => p.History)
                                                .Include(p => p.SubmitterUser)
                                                .Include(p => p.TicketPriority)
                                                .Include(p => p.TicketStatus)
                                                .Include(p => p.TicketType)
                                                .Include(p => p.Project)
                                                .Where(p => p.Archived == true)
                                            .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public async Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId)
        {
            try
            {
                return await _context.Tickets
                                     .Include(t => t.DeveloperUser)
                                     .Include(t => t.Project)
                                     .Include(t => t.TicketPriority)
                                     .Include(t => t.TicketStatus)
                                     .Include(t => t.TicketType)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(t => t.Id == ticketId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = new();

                ticket = await _context.Tickets
                                                .Include(t => t.Project)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.History)
                                                .Include(t => t.SubmitterUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .FirstOrDefaultAsync(t => t.Id == ticketId);

                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            List<Ticket> result = new();
            List<Ticket> tickets = new();

            try
            {
                tickets = await _context.Tickets.Include(t => t.Project).Where(t => t.DeveloperUserId == null).ToListAsync();

                foreach (Ticket ticket in tickets)
                {
                    result.Add(ticket);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        public async Task<List<Ticket>> GetTicketByUserIdAync(string userId, int compId)
        {
            BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket>? tickets = new();

            try
            {
                if (await _poisonRolesService.IsUserInRoleAsync(btUser!, nameof(PoisonRoles.Admin)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(compId))
                                                    .SelectMany(p => p.Tickets!).ToList();
                }
                else if (await _poisonRolesService.IsUserInRoleAsync(btUser!, nameof(PoisonRoles.Developer)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(compId))
                                                    .SelectMany(p => p.Tickets!)
                                                    .Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (await _poisonRolesService.IsUserInRoleAsync(btUser!, nameof(PoisonRoles.Submitter)))
                {
                    tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(compId)).SelectMany(p => p.Tickets!)
                                                    .Where(t => t.SubmitterUserId == userId).ToList();
                }
                else if (await _poisonRolesService.IsUserInRoleAsync(btUser!, nameof(PoisonRoles.ProjectManager)))
                {
                    List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId)).SelectMany(t => t.Tickets!).ToList();
                    List<Ticket>? submittedTickets = (await _projectService.GetAllProjectsByCompanyIdAsync(compId))
                                                    .SelectMany(p => p.Tickets!).Where(t => t.SubmitterUserId == userId).ToList();
                    tickets = projectTickets.Concat(submittedTickets).ToList();
                }

                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RetoreArchivedTicketsAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = false;
                await UpdateTicketAsync(ticket);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
