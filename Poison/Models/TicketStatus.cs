using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Poison.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Priority Name")]
        public string? Name { get; set; }
    }
}
