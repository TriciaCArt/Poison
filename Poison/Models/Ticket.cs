using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Poison.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Ticket Title")]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000)]
        [DisplayName("Ticket Description")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Updated")]
        public DateTime? Updated { get; set; }

        [DisplayName("Archived")]
        public bool Archived { get; set; }

        [DisplayName("Archived by Project")]
        public bool ArchivedByProject { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [DisplayName("Priority")]
        public int TicketPriorityId { get; set; }

        [DisplayName("Status")]
        public int TicketStatusId { get; set; }

        [DisplayName("Type")]
        public int TicketTypeId { get; set; }


        // Foreign Keys
        [Required]
        public string? SubmitterUserId { get; set; }

        public string? DeveloperUserId { get; set; }

        //Navigational Properties
        public virtual Project? Project { get; set; }

        [DisplayName("Priority")]
        public virtual TicketPriority? TicketPriority { get; set; }

        [DisplayName("Status")]
        public virtual TicketStatus? TicketStatus { get; set; }

        [DisplayName("Type")]
        public virtual TicketType? TicketType { get; set; }

        [DisplayName("Submitted By")]
        public virtual BTUser? SubmitterUser { get; set; }

        [DisplayName("Ticket Developer")]
        public virtual BTUser? DeveloperUser { get; set; }

        //Navigation Collections
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        public virtual ICollection<Notification>? Notifications { get; set; } = new HashSet<Notification>();

        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();


    }
}
