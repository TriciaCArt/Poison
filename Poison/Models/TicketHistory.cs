using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Poison.Models
{
    public class TicketHistory
    {
        // Primary Key
        public int Id { get; set; }

        [DisplayName("Updated Ticket Property")]
        public string? PropertyName { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DisplayName("Previous Value")]
        public string? OldValue { get; set; }

        [DisplayName("Current Value")]
        public string? NewValue { get; set; }

        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        //Navigation Properties

        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
