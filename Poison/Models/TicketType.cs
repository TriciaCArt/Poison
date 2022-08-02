using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Poison.Models
{
    public class TicketType
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Display Name")]
        public string? Name { get; set; }


    }
}
