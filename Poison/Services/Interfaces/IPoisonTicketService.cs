using Poison.Models;

namespace Poison.Services.Interfaces
{
    public interface IPoisonTicketService
    {
        public Task AddNewTicketAsync(Ticket ticket);

        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task ArchiveTicketAsync(Ticket ticket);

        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

        public Task<Ticket> GetTicketByIdAsync(int ticketId);

        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId);

        public Task<List<Ticket>> GetTicketByUserIdAync(string userId, int compId);
        public Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId);

        public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);

        public Task RetoreArchivedTicketsAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);
    }
}
