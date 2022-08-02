using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Poison.Models
{
    public class TicketComment
    {
        //primary key
        public int Id { get; set; }

        [Required]
        [DisplayName("Member Comment")]
        [StringLength(2000)]
        public string? Comment { get; set; }

        [DisplayName("Created Date")]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        //This parent is by default required! The ID of the ticket Foreign Key
        public int TicketId { get; set; }

        //Also a foreign Key
        [Required]
        public string? UserId { get; set; }


        // Navigation Properties
        [DisplayName("Ticket")]
        public virtual Ticket? Ticket { get; set; }
        [DisplayName("Team Member")]
        public virtual BTUser? User { get; set; }

    }
}
