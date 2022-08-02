using Microsoft.AspNetCore.Mvc.Rendering;

namespace Poison.Models.ViewModels
{
    public class AssignDevToTicketViewModel
    {
        public Ticket? Ticket { get; set; }
        public string? DeveloperId { get; set; }
        public SelectList? Developers { get; set; }
    }
}
